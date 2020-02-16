#!/bin/bash
echo "Beta Deploy"
~/bin/butler -V
~/bin/butler login
~/bin/butler wipe zombusters-windows.zip
zip -r zombusters-windows.zip /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Release/
~/bin/butler push zombusters-windows.zip retrowax/zombusters:windows --userversion $MAJOR.$MINOR.$PATCH
