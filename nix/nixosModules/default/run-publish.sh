URL="$1"
REPO_PATH="$2"
REMOTE="$3"
IGNORE_PUSH_ERRORS_ARG="$4"
shift
shift
shift
shift
JOBS="$*"

_old_commit=""
if [ ! -d "$REPO_PATH" ]; then
    git clone "$REMOTE" "$REPO_PATH"
else
    _old_commit=$(git -C "$REPO_PATH" rev-parse HEAD)
    git -C "$REPO_PATH" pull
fi
_new_commit=$(git -C "$REPO_PATH" rev-parse HEAD)

if [[ "$_old_commit" != "$_new_commit" ]]; then
    for job in $JOBS; do
        arrivin --server "$URL" publish "$REPO_PATH#arrivin.$job" "$IGNORE_PUSH_ERRORS_ARG" -- --max-jobs 1 || true
    done
fi
