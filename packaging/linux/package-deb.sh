#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 3 ]]; then
  echo "Usage: $0 <publish_dir> <version> <output_dir>" >&2
  exit 1
fi

publish_dir="$1"
version="$2"
output_dir="$3"

package_root="$(mktemp -d)"
install_root="$package_root/opt/timekeeper"
bin_root="$package_root/usr/local/bin"
debian_root="$package_root/DEBIAN"

mkdir -p "$install_root" "$bin_root" "$debian_root" "$output_dir"
cp -R "$publish_dir"/. "$install_root"/

cat > "$bin_root/timekeeper" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
exec /opt/timekeeper/Timekeeper.Web "$@"
EOF
chmod 755 "$bin_root/timekeeper"

installed_size_kb="$(du -sk "$package_root" | awk '{print $1}')"

cat > "$debian_root/control" <<EOF
Package: timekeeper
Version: $version
Section: utils
Priority: optional
Architecture: amd64
Maintainer: Timekeeper Contributors <opensource@timekeeper.local>
Depends: libc6, libgcc-s1, libicu72 | libicu71 | libicu70 | libicu69, libssl3 | libssl1.1, zlib1g
Description: Timekeeper payroll and time management platform
 Cross-platform payroll and timekeeping application packaged as a self-contained .NET runtime.
Installed-Size: $installed_size_kb
EOF

chmod 755 "$package_root" "$install_root" "$bin_root"
dpkg-deb --build "$package_root" "$output_dir/timekeeper_${version}_amd64.deb"
rm -rf "$package_root"
