#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler upgrade
echo y
~/bin/butler login

~/bin/butler wipe zombusters-windows.zip
rm -fr ZombustersPCWindows
mkdir ZombustersPCWindows
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release/* ZombustersPCWindows
zip -r -v zombusters-windows.zip ./ZombustersPCWindows
~/bin/butler push zombusters-windows.zip retrowax/zombusters:windows --userversion $MAJOR.$MINOR.$PATCH

~/bin/butler wipe zombusters-mac.zip
rm -fr ZombustersMac
mkdir ZombustersMac
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Release/netcoreapp2.2/* ZombustersMac
zip -r -v zombusters-mac.zip ./ZombustersMac
~/bin/butler push zombusters-mac.zip retrowax/zombusters:mac --userversion $MAJOR.$MINOR.$PATCH
