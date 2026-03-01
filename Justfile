dotnet := "~/.dotnet/dotnet"

# Build the entire solution
build:
    {{dotnet}} build Rend.sln

# Run all unit tests
test:
    {{dotnet}} test Rend.sln

# Run all unit tests without building first
test-fast:
    {{dotnet}} test Rend.sln --no-build

# Run tests matching a filter (e.g., just test-filter GridLayout)
test-filter FILTER:
    {{dotnet}} test Rend.sln --filter "{{FILTER}}"

# Run conformance tests only
conformance:
    {{dotnet}} test conformance/Rend.Html.Conformance
    {{dotnet}} test conformance/Rend.Css.Conformance

# Run visual regression suite (Chrome vs Rend)
visual:
    {{dotnet}} run --project conformance/Rend.VisualRegression

# Open the visual regression report in the default browser
report:
    open conformance/Rend.VisualRegression/output/report.html

# Clean build artifacts
clean:
    {{dotnet}} clean Rend.sln

# Restore NuGet packages
restore:
    {{dotnet}} restore Rend.sln

# Count total tests across all test projects
count:
    @echo "Counting tests across all projects..."
    @{{dotnet}} test Rend.sln --list-tests 2>/dev/null | grep -c "    " || true

# Watch and re-run tests on file changes
watch:
    {{dotnet}} watch test --project tests/Rend.Tests
