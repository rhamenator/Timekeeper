#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 3 ]]; then
  echo "Usage: $0 <publish_dir> <version> <output_dir>" >&2
  exit 1
fi

publish_dir="$1"
version="$2"
output_dir="$3"

build_root="$(mktemp -d)"
payload_root="$build_root/timekeeper-$version"
install_root="$payload_root/opt/timekeeper"
bin_root="$payload_root/usr/local/bin"

mkdir -p "$install_root" "$bin_root" "$output_dir"
cp -R "$publish_dir"/. "$install_root"/

cat > "$bin_root/timekeeper" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
exec /opt/timekeeper/Timekeeper.Web "$@"
EOF
chmod 755 "$bin_root/timekeeper"

if ! command -v fpm >/dev/null 2>&1; then
  echo "fpm is required to build the RPM package." >&2
  exit 1
fi

fpm -s dir -t rpm \
  -n timekeeper \
  -v "$version" \
  --rpm-os linux \
  --description "Timekeeper payroll and time management platform" \
  -C "$payload_root" \
  -p "$output_dir/timekeeper-${version}.x86_64.rpm" \
  .

rm -rf "$build_root"
