#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 4 ]]; then
  echo "Usage: $0 <publish_dir> <version> <runtime> <output_dir>" >&2
  exit 1
fi

publish_dir="$1"
version="$2"
runtime="$3"
output_dir="$4"

package_root="$(mktemp -d)"
app_root="$package_root/Applications/Timekeeper"
bin_root="$package_root/usr/local/bin"

mkdir -p "$app_root" "$bin_root" "$output_dir"
cp -R "$publish_dir"/. "$app_root"/

cat > "$bin_root/timekeeper" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
exec "/Applications/Timekeeper/Timekeeper.Web" "$@"
EOF
chmod 755 "$bin_root/timekeeper"

pkgbuild \
  --root "$package_root" \
  --identifier "org.timekeeper.app.$runtime" \
  --version "$version" \
  --install-location "/" \
  "$output_dir/Timekeeper-${version}-${runtime}.pkg"

rm -rf "$package_root"
