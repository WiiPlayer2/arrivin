URL="$1"
REPO_PATH="$2"
REMOTE="$3"
shift
shift
shift
JOBS="$@"

if [ ! -d "$REPO_PATH" ]; then
    git clone "$REMOTE" "$REPO_PATH"
fi

for job in $JOBS; do
    arrivin --server "$URL" publish "$REPO_PATH"#"$job" || true
done
