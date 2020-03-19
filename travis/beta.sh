#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler upgrade
echo y
~/bin/butler login

~/bin/butler wipe zombusters-windows-beta.zip
rm -fr ZombustersPCWindowsBETA
mkdir ZombustersPCWindowsBETA
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release/* ZombustersPCWindowsBETA
zip -r -v zombusters-windows-beta.zip ./ZombustersPCWindowsBETA
~/bin/butler push zombusters-windows-beta.zip retrowax/zombusters:windows-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW

~/bin/butler wipe zombusters-mac-beta.zip
rm -fr ZombustersMacBETA
mkdir ZombustersMacBETA
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Release/netcoreapp2.2/* ZombustersMacBETA
zip -r -v zombusters-mac-beta.zip ./ZombustersMacBETA
~/bin/butler push zombusters-mac-beta.zip retrowax/zombusters:mac-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
