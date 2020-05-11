#!/bin/bash
echo "Demo Deploy"
~/bin/butler -V
~/bin/butler upgrade --assume-yes
~/bin/butler login

nuget restore ZombustersWindows.sln
msbuild /t:ZombustersWindows /p:Configuration=Demo ZombustersWindows.sln
nuget restore ZombustersMac.sln
msbuild /p:Configuration=Demo ZombustersMac.sln

~/bin/butler wipe zombusters-windows-demo.zip
rm -fr ZombustersPCWindowsDEMO
mkdir ZombustersPCWindowsDEMO
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Windows/Demo/* ZombustersPCWindowsDEMO
zip -r -v zombusters-windows-demo.zip ./ZombustersPCWindowsDEMO
~/bin/butler push zombusters-windows-demo.zip retrowax/zombusters:windows-demo --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW

~/bin/butler wipe zombusters-mac-demo.zip
rm -fr ZombustersMacDEMO
mkdir ZombustersMacDEMO
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Demo/netcoreapp2.2/* ZombustersMacDEMO
zip -r -v zombusters-mac-demo.zip ./ZombustersMacDEMO
~/bin/butler push zombusters-mac-demo.zip retrowax/zombusters:mac-demo --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
