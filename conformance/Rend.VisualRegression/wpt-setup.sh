#!/bin/bash
# Downloads Web Platform Tests (CSS modules only) via sparse checkout.
# Usage: ./wpt-setup.sh [module...]
# Example: ./wpt-setup.sh css-flexbox css-grid css-text
# No args = downloads all priority CSS modules.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
WPT_DIR="$SCRIPT_DIR/wpt"
REPO_URL="https://github.com/web-platform-tests/wpt.git"

PRIORITY_MODULES=(
    css-backgrounds
    css-box
    css-color
    css-display
    css-flexbox
    css-fonts
    css-grid
    css-images
    css-lists
    css-multicol
    css-overflow
    css-position
    css-sizing
    css-tables
    css-text
    css-text-decor
    css-transforms
    css-values
    css-writing-modes
)

if [ $# -gt 0 ]; then
    MODULES=("$@")
else
    MODULES=("${PRIORITY_MODULES[@]}")
fi

if [ -d "$WPT_DIR/.git" ]; then
    echo "WPT repo exists at $WPT_DIR, updating sparse checkout..."
    cd "$WPT_DIR"

    # Update sparse-checkout patterns
    PATTERNS="fonts"$'\n'
    for mod in "${MODULES[@]}"; do
        PATTERNS+="css/$mod"$'\n'
    done
    echo "$PATTERNS" | git sparse-checkout set --stdin

    git pull --ff-only origin master 2>/dev/null || true
else
    echo "Cloning WPT repo (sparse) into $WPT_DIR..."
    git clone --filter=blob:none --no-checkout --depth 1 "$REPO_URL" "$WPT_DIR"
    cd "$WPT_DIR"
    git sparse-checkout init --cone

    PATTERNS="fonts"$'\n'
    for mod in "${MODULES[@]}"; do
        PATTERNS+="css/$mod"$'\n'
    done
    echo "$PATTERNS" | git sparse-checkout set --stdin

    git checkout
fi

# Count tests
TOTAL=0
for mod in "${MODULES[@]}"; do
    if [ -d "css/$mod" ]; then
        COUNT=$(find "css/$mod" -name "*.html" -not -path "*/reference/*" -not -path "*/ref/*" | wc -l)
        TOTAL=$((TOTAL + COUNT))
        echo "  $mod: $COUNT HTML files"
    else
        echo "  $mod: (not found)"
    fi
done
echo "Total: $TOTAL HTML test files"
