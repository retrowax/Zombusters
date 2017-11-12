#!/bin/bash
echo "Beta Deploy"
zip -r zombusters_windows.zip /home/travis/build/retrowax/Zombusters/ZombustersWindows/bin/Windows/Release/
~/bin/butler -V
~/bin/butler login
~/bin/butler push zombusters_windows.zip retrowax/zombusters:windows
