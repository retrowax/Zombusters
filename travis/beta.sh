#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler login
~/bin/butler wipe zombusters-windows-beta.zip
mkdir ZombustersWindows
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release ./ZombustersWindows
zip -r -v zombusters-windows-beta.zip ./ZombustersWindows
~/bin/butler push zombusters-windows-beta.zip retrowax/zombusters:windows-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
