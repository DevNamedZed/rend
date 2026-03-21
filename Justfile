set windows-shell := ["cmd.exe", "/c"]

# Disable persistent build server daemons (they leak 7-10GB RAM)
export DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER := "1"

dotnet := "dotnet"

# List available recipes
default:
    @just --list

# Build the entire solution
build:
    {{dotnet}} build Rend.sln

# Build in Release mode
build-release:
    {{dotnet}} build Rend.sln -c Release

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

# Build visual regression project
visual-build:
    {{dotnet}} build conformance/Rend.VisualRegression/Rend.VisualRegression.csproj -c Release

vr-exe := "conformance\\Rend.VisualRegression\\bin\\Release\\net8.0\\Rend.VisualRegression.exe"

# Run visual regression suite (Chrome vs Rend)
visual: visual-build
    {{vr-exe}}

# Run visual regression with an ID filter (e.g., just visual-filter newsletter)
visual-filter FILTER: visual-build
    {{vr-exe}} --filter {{FILTER}}

# Run visual regression for a tag (e.g., just visual-tag Playground, just visual-tag WPT)
visual-tag TAG: visual-build
    {{vr-exe}} --tag {{TAG}}

# Run visual regression and update checked-in results
visual-update:
    {{dotnet}} run --project conformance/Rend.VisualRegression
    @echo "Copying results to conformance/results/..."
    @cp conformance/Rend.VisualRegression/output/$$(ls -t conformance/Rend.VisualRegression/output/ | head -1)/report.html conformance/results/report.html
    @cp conformance/Rend.VisualRegression/output/$$(ls -t conformance/Rend.VisualRegression/output/ | head -1)/results.json conformance/results/results.json
    @echo "Done. Review conformance/results/ and commit."

# Open the visual regression report in the default browser
report:
    open conformance/results/report.html

# Clean build artifacts including playground publish output
clean:
    {{dotnet}} clean Rend.sln
    {{dotnet}} clean Rend.sln -c Release

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

# Build and publish the playground WASM app (full native recompile)
playground-build:
    {{dotnet}} publish playground/Rend.Playground/Rend.Playground.csproj -c Release -o playground/release

# Quick republish playground (skips native recompile if wasm is cached)
playground-publish:
    {{dotnet}} publish playground/Rend.Playground/Rend.Playground.csproj -c Release -o playground/release

# Serve the playground locally at http://localhost:8080
playground-serve:
    @echo "Serving playground at http://localhost:8080"
    python3 -m http.server 8080 --directory playground/release/wwwroot

# Full clean build and serve the playground
playground: playground-build playground-serve

# Generate PDF test outputs from playground examples
pdf-generate:
    {{dotnet}} build Rend.sln -c Release
    {{dotnet}} test tests/Rend.Tests/Rend.Tests.csproj -c Release --filter "FullyQualifiedName~ComplexPdf" --no-build

# Render PDF test outputs to PNGs for visual inspection
pdf-render: pdf-generate
    python3 -m pytest tests/pdf_integration/ -v

# Run only the Python PDF validation tests (assumes test-output/ exists)
pdf-validate:
    python3 -m pytest tests/pdf_integration/ -v

# Pack NuGet packages
pack:
    {{dotnet}} pack Rend.sln -c Release -o nupkg
