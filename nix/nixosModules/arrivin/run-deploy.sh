URL="$1"
shift
JOBS="$*"

for job in $JOBS; do
    arrivin --server "$URL" deploy "$job" -- --max-jobs 1 || true
done
