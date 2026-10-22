#!/bin/sh
set -eu
ROOT=$(CDPATH='' cd -- "$(dirname -- "$0")/.." && pwd)
: "${THSSS_DATA_ROOT:=$ROOT/game}"
export THSSS_DATA_ROOT
cd "$ROOT"

exec dotnet run --project "$ROOT/src/THSSS.csproj" -- "$@"
