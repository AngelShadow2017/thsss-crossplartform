#!/bin/sh
set -eu
ROOT=$(CDPATH='' cd -- "$(dirname -- "$0")/.." && pwd)
cd "$ROOT/src"
for suite in ShaderAudit ShaderGpuAudit GraphicsCompatibility MatrixConventions MathCompatibility EffectCompatibility MeshCompatibility LightingCompatibility; do
    printf '\n=== %s ===\n' "$suite"
    dotnet run --project "../tests/$suite/$suite.csproj" "$@"
done
