#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler login
~/bin/butler wipe zombusters-windows-beta.zip
rm -fr ZombustersPCWindows
mkdir ZombustersPCWindows
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release ZombustersPCWindows
zip -r -v zombusters-windows-beta.zip ./ZombustersPCWindows
~/bin/butler push zombusters-windows-beta.zip retrowax/zombusters:windows-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
