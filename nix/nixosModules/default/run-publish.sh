URL="$1"
REPO_PATH="$2"
REMOTE="$3"
IGNORE_PUSH_ERRORS_ARG="$4"
shift
shift
shift
shift
JOBS="$*"

if [ ! -d "$REPO_PATH" ]; then
    git clone "$REMOTE" "$REPO_PATH"
else
    git -C "$REPO_PATH" pull
fi

for job in $JOBS; do
    arrivin --server "$URL" publish "$REPO_PATH#arrivin.$job" "$IGNORE_PUSH_ERRORS_ARG" -- --max-jobs 1 || true
done
