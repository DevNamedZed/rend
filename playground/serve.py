#!/usr/bin/env python3
"""Simple HTTP server with correct MIME types for Blazor WASM.

Usage: python3 playground/serve.py
Then open http://localhost:8080
"""
import http.server
import os
import sys

PORT = 8080
DIRECTORY = os.path.join(os.path.dirname(__file__), "release", "wwwroot")

if not os.path.isdir(DIRECTORY):
    print(f"Error: {DIRECTORY} not found. Run 'dotnet publish' first:")
    print("  dotnet publish playground/Rend.Playground/Rend.Playground.csproj -c Release -o playground/release")
    sys.exit(1)


class WasmHandler(http.server.SimpleHTTPRequestHandler):
    extensions_map = {
        **http.server.SimpleHTTPRequestHandler.extensions_map,
        ".wasm": "application/wasm",
        ".br": "application/octet-stream",
        ".gz": "application/octet-stream",
        ".json": "application/json",
        ".dat": "application/octet-stream",
        ".dll": "application/octet-stream",
        ".blat": "application/octet-stream",
    }

    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=DIRECTORY, **kwargs)


print(f"Serving {DIRECTORY} at http://localhost:{PORT}")
print("Press Ctrl+C to stop")
http.server.HTTPServer(("", PORT), WasmHandler).serve_forever()
