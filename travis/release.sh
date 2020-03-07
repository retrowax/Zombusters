#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler login
~/bin/butler wipe zombusters-windows.zip
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release/ ./ZombustersWindows
zip -r zombusters-windows.zip /ZombustersWindows/
~/bin/butler push zombusters-windows.zip retrowax/zombusters:windows --userversion $MAJOR.$MINOR.$PATCH
