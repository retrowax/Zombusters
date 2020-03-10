#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler login
~/bin/butler wipe zombusters-windows.zip
rm -fr ZombustersPCWindows
mkdir ZombustersPCWindows
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release/* ZombustersPCWindows
zip -r -v zombusters-windows.zip ./ZombustersPCWindows
~/bin/butler push zombusters-windows.zip retrowax/zombusters:windows --userversion $MAJOR.$MINOR.$PATCH
