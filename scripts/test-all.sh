#!/usr/bin/env bash
# Runs all backend (.NET) and frontend (Vitest) tests locally, writes artifacts
# under test-results/local/<timestamp>/, and prints a final summary.
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly ROOT

require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "error: '$1' not found in PATH" >&2
    exit 2
  fi
}

die() {
  echo "error: $*" >&2
  exit 2
}

format_duration_s() {
  local start="$1"
  local end="$2"
  echo $(( end - start ))
}

SUMMARY_ROWS=()

append_summary_row() {
  local status="$1"
  local label="$2"
  local seconds="$3"
  local log="$4"
  SUMMARY_ROWS+=("${status}|${label}|${seconds}|${log}")
}

main() {
  require_cmd dotnet
  require_cmd npm

  [[ -f "$ROOT/Coursely.slnx" ]] || die "missing $ROOT/Coursely.slnx"
  [[ -f "$ROOT/frontend/package.json" ]] || die "missing $ROOT/frontend/package.json"

  local stamp
  stamp="$(date +%Y-%m-%d_%H-%M-%S)"
  local out
  out="$ROOT/test-results/local/$stamp"
  mkdir -p "$out/backend/trx"

  local overall=0

  echo "== Coursely: running all tests =="
  echo "Artifacts directory: $out"
  echo

  local start_ts end_ts sec log code

  # --- Backend (.NET) ---
  log="$out/backend/dotnet.log"
  echo ">>> Backend: dotnet test Coursely.slnx"
  echo "log: $log"
  start_ts="$(date +%s)"
  (
    cd "$ROOT" || exit 1
    dotnet test Coursely.slnx \
      --logger trx \
      --results-directory "$out/backend/trx"
  ) 2>&1 | tee "$log"
  code="${PIPESTATUS[0]}"
  end_ts="$(date +%s)"
  sec="$(format_duration_s "$start_ts" "$end_ts")"
  if [[ "$code" -eq 0 ]]; then
    append_summary_row "PASS" "backend dotnet tests" "$sec" "$log"
  else
    append_summary_row "FAIL" "backend dotnet tests" "$sec" "$log"
    overall=1
  fi
  echo

  # --- Frontend (Vitest) ---
  log="$out/frontend-vitest.log"
  echo ">>> Frontend: npm run test (--reporter verbose)"
  echo "log: $log"
  start_ts="$(date +%s)"
  (
    cd "$ROOT/frontend" || exit 1
    npm run test -- --reporter=verbose
  ) 2>&1 | tee "$log"
  code="${PIPESTATUS[0]}"
  end_ts="$(date +%s)"
  sec="$(format_duration_s "$start_ts" "$end_ts")"
  if [[ "$code" -eq 0 ]]; then
    append_summary_row "PASS" "frontend vitest" "$sec" "$log"
  else
    append_summary_row "FAIL" "frontend vitest" "$sec" "$log"
    overall=1
  fi
  echo

  # --- Summary ---
  local report_file="$out/REPORT.txt"
  {
    echo "Coursely local test report"
    echo "Run: ${stamp}"
    echo
    local row stat label secs path
    for row in "${SUMMARY_ROWS[@]}"; do
      IFS='|' read -r stat label secs path <<<"$row"
      printf "%-5s %-28s %7s %s\n" "$stat" "$label" "${secs}s" "$path"
    done
    echo
    if [[ "$overall" -eq 0 ]]; then
      echo "Overall: PASS"
    else
      echo "Overall: FAIL"
    fi
    echo "Artifacts: $out"
    echo "(TRX files under backend/trx/ if dotnet test emitted them.)"
  } | tee "$report_file"

  exit "$overall"
}

main "$@"
