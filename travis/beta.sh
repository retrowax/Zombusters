#!/bin/bash
echo "Beta Deploy"
zip -r zombusters-windows-$SNAPSHOT_NOW.zip /home/travis/build/retrowax/Zombusters/ZombustersWindows/bin/Windows/Release/
~/bin/butler -V
~/bin/butler login
~/bin/butler push zombusters-windows-$SNAPSHOT_NOW.zip retrowax/zombusters:windows-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
