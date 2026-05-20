#!/usr/bin/env bash
# Local test runner: default runs backend + Vitest quickly; `--full` mirrors `.github/workflows/quality.yml`.
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly ROOT

FULL=0
for arg in "$@"; do
  case "$arg" in
    --full | --ci)
      FULL=1
      ;;
    -h | --help)
      cat <<EOF
usage: scripts/test-all.sh [--full|--ci]

  (default)  dotnet test + frontend Vitest (verbose)
  --full       dotnet restore/build/test Release + Coverlet like CI;
               frontend npm ci + format:check, typecheck, lint, test, build

Artifacts: test-results/local/<timestamp>/  (ignored by Git)
EOF
      exit 0
      ;;
    *)
      echo "error: unknown option: $arg (try --help)" >&2
      exit 2
      ;;
  esac
done

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
  echo $(($2 - $1))
}

SUMMARY_ROWS=()

append_summary_row() {
  SUMMARY_ROWS+=("${1}|${2}|${3}|${4}")
}

pipe_tee_log() {
  local log="$1"
  shift
  "$@" 2>&1 | tee "$log"
  return "${PIPESTATUS[0]}"
}

main() {
  require_cmd dotnet
  require_cmd npm

  [[ -f "$ROOT/Coursely.slnx" ]] || die "missing $ROOT/Coursely.slnx"
  [[ -f "$ROOT/frontend/package.json" ]] || die "missing $ROOT/frontend/package.json"

  local stamp
  stamp="$(date +%Y-%m-%d_%H-%M-%S)"
  local out="$ROOT/test-results/local/$stamp"
  mkdir -p "$out/backend/trx"

  local overall=0 mode_label="quick"

  [[ "$FULL" -eq 1 ]] && mode_label="full (CI parity)"

  echo "== Coursely: tests ($mode_label) =="
  echo "Artifacts directory: $out"
  echo

  local start_ts end_ts sec log code

  if [[ "$FULL" -eq 1 ]]; then
    log="$out/backend/dotnet-restore.log"
    echo ">>> Backend: dotnet restore"
    echo "log: $log"
    start_ts="$(date +%s)"
    pipe_tee_log "$log" bash -lc "cd \"$ROOT\" && dotnet restore Coursely.slnx"
    code="$?"
    end_ts="$(date +%s)"
    sec="$(format_duration_s "$start_ts" "$end_ts")"
    if [[ "$code" -eq 0 ]]; then append_summary_row "PASS" "backend dotnet restore" "$sec" "$log"; else append_summary_row "FAIL" "backend dotnet restore" "$sec" "$log"; overall=1; fi
    echo

    log="$out/backend/dotnet-build.log"
    echo ">>> Backend: dotnet build Release (--no-restore)"
    echo "log: $log"
    start_ts="$(date +%s)"
    pipe_tee_log "$log" bash -lc \
      "cd \"$ROOT\" && dotnet build Coursely.slnx --configuration Release --no-restore"
    code="$?"
    end_ts="$(date +%s)"
    sec="$(format_duration_s "$start_ts" "$end_ts")"
    if [[ "$code" -eq 0 ]]; then append_summary_row "PASS" "backend dotnet build Release" "$sec" "$log"; else append_summary_row "FAIL" "backend dotnet build Release" "$sec" "$log"; overall=1; fi
    echo

    log="$out/backend/dotnet-test-coverlet.log"
    mkdir -p "$out/backend/coverlet-results"
    echo ">>> Backend: dotnet test Release (--no-build) + Coverlet"
    echo "log: $log"
    start_ts="$(date +%s)"
    pipe_tee_log "$log" bash -lc \
      "cd \"$ROOT\" && dotnet test Coursely.slnx --configuration Release --no-build \
        --verbosity normal --collect:\"XPlat Code Coverage\" \
        --results-directory \"$out/backend/coverlet-results\""
    code="$?"
    end_ts="$(date +%s)"
    sec="$(format_duration_s "$start_ts" "$end_ts")"
    if [[ "$code" -eq 0 ]]; then append_summary_row "PASS" "backend dotnet test Coverlet" "$sec" "$log"; else append_summary_row "FAIL" "backend dotnet test Coverlet" "$sec" "$log"; overall=1; fi
    echo

    log="$out/frontend-npm-ci.log"
    echo ">>> Frontend: npm ci"
    echo "log: $log"
    start_ts="$(date +%s)"
    pipe_tee_log "$log" bash -lc "cd \"$ROOT/frontend\" && npm ci"
    code="$?"
    end_ts="$(date +%s)"
    sec="$(format_duration_s "$start_ts" "$end_ts")"
    if [[ "$code" -eq 0 ]]; then append_summary_row "PASS" "frontend npm ci" "$sec" "$log"; else append_summary_row "FAIL" "frontend npm ci" "$sec" "$log"; overall=1; fi
    echo

    for step in format:check typecheck lint test build; do
      safe="${step//:/-}"
      log="$out/frontend-${safe}.log"
      echo ">>> Frontend: npm run $step"
      echo "log: $log"
      start_ts="$(date +%s)"
      pipe_tee_log "$log" bash -lc "cd \"$ROOT/frontend\" && npm run \"$step\""
      code="$?"
      end_ts="$(date +%s)"
      sec="$(format_duration_s "$start_ts" "$end_ts")"
      if [[ "$code" -eq 0 ]]; then
        append_summary_row "PASS" "frontend npm run $step" "$sec" "$log"
      else
        append_summary_row "FAIL" "frontend npm run $step" "$sec" "$log"
        overall=1
      fi
      echo
    done

  else

    log="$out/backend/dotnet.log"
    echo ">>> Backend: dotnet test (trx)"
    echo "log: $log"
    start_ts="$(date +%s)"
    pipe_tee_log "$log" bash -lc \
      "cd \"$ROOT\" && dotnet test Coursely.slnx \
        --logger trx --results-directory \"$out/backend/trx\""
    code="$?"
    end_ts="$(date +%s)"
    sec="$(format_duration_s "$start_ts" "$end_ts")"
    if [[ "$code" -eq 0 ]]; then append_summary_row "PASS" "backend dotnet tests" "$sec" "$log"; else append_summary_row "FAIL" "backend dotnet tests" "$sec" "$log"; overall=1; fi
    echo

    log="$out/frontend-vitest.log"
    echo ">>> Frontend: npm run test (--reporter verbose)"
    echo "log: $log"
    start_ts="$(date +%s)"
    pipe_tee_log "$log" bash -lc \
      "cd \"$ROOT/frontend\" && npm run test -- --reporter=verbose"
    code="$?"
    end_ts="$(date +%s)"
    sec="$(format_duration_s "$start_ts" "$end_ts")"
    if [[ "$code" -eq 0 ]]; then append_summary_row "PASS" "frontend vitest" "$sec" "$log"; else append_summary_row "FAIL" "frontend vitest" "$sec" "$log"; overall=1; fi
    echo
  fi

  local report_file="$out/REPORT.txt"
  {
    echo "Coursely local test report ($mode_label)"
    echo "Run: ${stamp}"
    echo
    local row stat label secs path
    for row in "${SUMMARY_ROWS[@]}"; do
      IFS='|' read -r stat label secs path <<<"$row"
      printf "%-5s %-40s %7s %s\n" "$stat" "$label" "${secs}s" "$path"
    done
    echo
    if [[ "$overall" -eq 0 ]]; then echo "Overall: PASS"; else echo "Overall: FAIL"; fi
    echo "Artifacts: $out"
    if [[ "$FULL" -eq 1 ]]; then
      echo "Coverlet/raw output (if emitted): $out/backend/coverlet-results/"
    else
      echo "TRX (if emitted): $out/backend/trx/"
    fi
  } | tee "$report_file"

  exit "$overall"
}

main
