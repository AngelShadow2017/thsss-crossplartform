#!/bin/sh
set -eu
ROOT=$(CDPATH='' cd -- "$(dirname -- "$0")/.." && pwd)
if [ "$#" -ne 1 ]; then printf 'Usage: sh tools/export-metal-shaders.sh PUBLISH_DIRECTORY\n' >&2; exit 2; fi
OUT=$(realpath "$1")
for tool in glslangValidator spirv-cross; do
    command -v "$tool" >/dev/null || { printf 'Missing build tool: %s\n' "$tool" >&2; exit 1; }
done
cd "$ROOT"
dotnet run --project tests/MetalCompatibility/MetalCompatibility.csproj -- --export "$OUT/MetalShaders"
