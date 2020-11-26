#!/bin/bash
echo "Beta Deploy to Itch.io"
~/bin/butler -V
~/bin/butler upgrade --assume-yes
~/bin/butler login

~/bin/butler wipe zombusters_windows_beta.zip
rm -fr ZombustersPCWindowsBETA
mkdir ZombustersPCWindowsBETA
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release/* ZombustersPCWindowsBETA
zip -r -v zombusters_windows_beta.zip ./ZombustersPCWindowsBETA
~/bin/butler push zombusters_windows_beta.zip retrowax/zombusters:windows-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW

~/bin/butler wipe zombusters-mac-beta.zip
rm -fr ZombustersMacBETA
mkdir ZombustersMacBETA
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Release/netcoreapp2.2/* ZombustersMacBETA
zip -r -v zombusters-mac-beta.zip ./ZombustersMacBETA
~/bin/butler push zombusters-mac-beta.zip retrowax/zombusters:mac-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
