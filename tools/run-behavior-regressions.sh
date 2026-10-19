#!/bin/sh
set -eu
ROOT=$(CDPATH='' cd -- "$(dirname -- "$0")/.." && pwd)
cd "$ROOT/src"
for suite in ListCompatibility AudioCompatibility SaveCompatibility TextScreenshotCompatibility WindowInputCompatibility ConfigCompatibility; do
    printf '\n=== %s ===\n' "$suite"
    if [ "$suite" = WindowInputCompatibility ]; then
        dotnet run --project "../tests/$suite/$suite.csproj" -p:WarningLevel=0 -p:GenerateResourceWarnOnBinaryFormatterUse=false -- --glfw --game-smoke
    else
        dotnet run --project "../tests/$suite/$suite.csproj" -p:WarningLevel=0 -p:GenerateResourceWarnOnBinaryFormatterUse=false
    fi
done
