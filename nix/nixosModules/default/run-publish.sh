URL="$1"
REPO_PATH="$2"
REMOTE="$3"
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
    arrivin --server "$URL" publish "$REPO_PATH"#"arrivin.$job" || true
done
