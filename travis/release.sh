#!/bin/bash
echo "Beta Deploy"
zip -r zombusters-windows.zip /home/travis/build/retrowax/Zombusters/ZombustersWindows/bin/Windows/Release/
~/bin/butler -V
~/bin/butler login
~/bin/butler push zombusters-windows.zip retrowax/zombusters:windows --userversion $MAJOR.$MINOR.$PATCH
