#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler login
~/bin/butler wipe zombusters-windows-beta.zip
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release/ ./ZombustersWindows
zip -r zombusters-windows.zip /ZombustersWindows/
~/bin/butler push zombusters-windows-beta.zip retrowax/zombusters:windows-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
