#!/usr/bin/env python3
"""
Convert C# visual regression test cases to YAML files.

Reads all .cs files in TestCases/ directory, extracts VisualTestCatalog.Register() blocks,
and writes grouped YAML files to TestData/ directory (one per category).
"""

import os
import re
import sys
from collections import defaultdict

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
TESTCASES_DIR = os.path.join(SCRIPT_DIR, "TestCases")
TESTDATA_DIR = os.path.join(SCRIPT_DIR, "TestData")

# Defaults — only emit non-default values
DEFAULT_VIEWPORT_WIDTH = 400
DEFAULT_VIEWPORT_HEIGHT = 300
DEFAULT_TOLERANCE = 0.01


def extract_string_value(block, field_name):
    """Extract a simple string field like Id = "value" from a block."""
    pattern = rf'{field_name}\s*=\s*"([^"]*)"'
    m = re.search(pattern, block)
    if m:
        return m.group(1)
    return None


def extract_int_value(block, field_name):
    """Extract an integer field like ViewportWidth = 595 from a block."""
    pattern = rf'{field_name}\s*=\s*(\d+)'
    m = re.search(pattern, block)
    if m:
        return int(m.group(1))
    return None


def extract_float_value(block, field_name):
    """Extract a float field like Tolerance = 0.05 from a block."""
    pattern = rf'{field_name}\s*=\s*([\d.]+)'
    m = re.search(pattern, block)
    if m:
        return float(m.group(1))
    return None


def extract_html(block):
    """Extract the Html verbatim string (@"...") from a register block.

    Handles multi-line content and C# escaped double-quotes ("" → ").
    Uses character-by-character parsing since regex can't reliably handle
    the "" escape sequence in C# verbatim strings.
    """
    # Find the start: Html = @"
    m = re.search(r'Html\s*=\s*@"', block)
    if not m:
        return None

    pos = m.end()
    chars = []

    while pos < len(block):
        ch = block[pos]
        if ch == '"':
            # Check if this is an escaped "" or the closing "
            if pos + 1 < len(block) and block[pos + 1] == '"':
                # Escaped double-quote: "" → "
                chars.append('"')
                pos += 2
            else:
                # End of verbatim string
                break
        else:
            chars.append(ch)
            pos += 1

    return ''.join(chars)


def find_register_blocks(content):
    """Find all VisualTestCatalog.Register(new VisualTestCase { ... }) blocks."""
    blocks = []
    # Match from Register( to the closing });
    # We need to handle nested braces in the Html content
    pattern = r'VisualTestCatalog\.Register\(new\s+VisualTestCase\s*\{'
    for m in re.finditer(pattern, content):
        start = m.start()
        # Find matching closing brace by counting braces
        # Start after the opening { of VisualTestCase
        pos = m.end()
        depth = 1
        in_string = False
        in_verbatim = False

        while pos < len(content) and depth > 0:
            ch = content[pos]

            if in_verbatim:
                if ch == '"':
                    # Check for "" (escaped quote in verbatim string)
                    if pos + 1 < len(content) and content[pos + 1] == '"':
                        pos += 2
                        continue
                    else:
                        in_verbatim = False
                pos += 1
                continue

            if in_string:
                if ch == '\\':
                    pos += 2  # skip escaped char
                    continue
                if ch == '"':
                    in_string = False
                pos += 1
                continue

            if ch == '@' and pos + 1 < len(content) and content[pos + 1] == '"':
                in_verbatim = True
                pos += 2
                continue

            if ch == '"':
                in_string = True
                pos += 1
                continue

            if ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1

            pos += 1

        block = content[start:pos]
        blocks.append(block)

    return blocks


def parse_test(block):
    """Parse a single register block into a test dict."""
    test = {}

    test_id = extract_string_value(block, "Id")
    if not test_id:
        return None
    test["id"] = test_id

    name = extract_string_value(block, "Name")
    if name:
        test["name"] = name

    category = extract_string_value(block, "Category")
    if category:
        test["category"] = category

    vw = extract_int_value(block, "ViewportWidth")
    if vw is not None and vw != DEFAULT_VIEWPORT_WIDTH:
        test["viewport_width"] = vw

    vh = extract_int_value(block, "ViewportHeight")
    if vh is not None and vh != DEFAULT_VIEWPORT_HEIGHT:
        test["viewport_height"] = vh

    tol = extract_float_value(block, "Tolerance")
    if tol is not None and tol != DEFAULT_TOLERANCE:
        test["tolerance"] = tol

    html = extract_html(block)
    if html:
        test["html"] = html

    return test


def category_to_filename(category):
    """Convert category name to filename: 'Basic Elements' → 'basic-elements.yaml'"""
    name = category.lower()
    name = re.sub(r'[^a-z0-9]+', '-', name)
    name = name.strip('-')
    return f"{name}.yaml"


def yaml_escape_string(s):
    """Escape a string for YAML inline (double-quoted) format."""
    s = s.replace('\\', '\\\\')
    s = s.replace('"', '\\"')
    s = s.replace('\n', '\\n')
    return f'"{s}"'


def indent_html(html, indent="      "):
    """Indent HTML lines for YAML block scalar."""
    lines = html.split('\n')
    # Strip common leading whitespace
    non_empty = [l for l in lines if l.strip()]
    if non_empty:
        # Find minimum indentation
        min_indent = min(len(l) - len(l.lstrip()) for l in non_empty)
        lines = [l[min_indent:] if len(l) >= min_indent else l for l in lines]

    # Remove trailing empty lines
    while lines and not lines[-1].strip():
        lines.pop()
    # Remove leading empty lines
    while lines and not lines[0].strip():
        lines.pop(0)

    return '\n'.join(indent + l for l in lines)


def write_yaml(filepath, category, tests):
    """Write a YAML file for a category of tests."""
    with open(filepath, 'w', encoding='utf-8', newline='\n') as f:
        f.write(f"# {category}\n")
        f.write(f"# Visual regression tests for {category.lower()}\n")
        f.write("tests:\n")

        for i, test in enumerate(tests):
            if i > 0:
                f.write("\n")

            f.write(f"  - id: {test['id']}\n")
            if 'name' in test:
                # Check if name needs quoting
                name = test['name']
                if any(c in name for c in ':{}[]&*?|>\'"%@`#,') or name.startswith('- '):
                    f.write(f'    name: "{name}"\n')
                else:
                    f.write(f"    name: {name}\n")

            if 'viewport_width' in test:
                f.write(f"    viewport_width: {test['viewport_width']}\n")
            if 'viewport_height' in test:
                f.write(f"    viewport_height: {test['viewport_height']}\n")
            if 'tolerance' in test:
                f.write(f"    tolerance: {test['tolerance']}\n")

            if 'html' in test:
                f.write("    html: |\n")
                f.write(indent_html(test['html']))
                f.write("\n")


def main():
    if not os.path.isdir(TESTCASES_DIR):
        print(f"ERROR: TestCases directory not found: {TESTCASES_DIR}", file=sys.stderr)
        sys.exit(1)

    os.makedirs(TESTDATA_DIR, exist_ok=True)

    # Collect all tests grouped by category
    categories = defaultdict(list)
    total_tests = 0
    total_files = 0

    cs_files = sorted(f for f in os.listdir(TESTCASES_DIR) if f.endswith('.cs'))

    for filename in cs_files:
        filepath = os.path.join(TESTCASES_DIR, filename)
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        blocks = find_register_blocks(content)
        file_count = 0

        for block in blocks:
            test = parse_test(block)
            if test:
                category = test.pop("category", "Uncategorized")
                categories[category].append(test)
                file_count += 1
                total_tests += 1

        if file_count > 0:
            total_files += 1
            print(f"  {filename}: {file_count} tests")
        else:
            print(f"  {filename}: WARNING - no tests found")

    print(f"\nTotal: {total_tests} tests from {total_files} files in {len(categories)} categories\n")

    # Write YAML files
    for category in sorted(categories.keys()):
        tests = categories[category]
        filename = category_to_filename(category)
        filepath = os.path.join(TESTDATA_DIR, filename)
        write_yaml(filepath, category, tests)
        print(f"  {filename}: {len(tests)} tests")

    print(f"\nDone. {len(categories)} YAML files written to {TESTDATA_DIR}")


if __name__ == "__main__":
    main()
