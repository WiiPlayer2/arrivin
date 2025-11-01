URL="$1"
shift
JOBS="$*"

for job in $JOBS; do
    arrivin --server "$URL" deploy "$job" || true
done
