#!/bin/bash
echo "Beta Deploy to Itch.io"
~/bin/butler -V
~/bin/butler upgrade --assume-yes
~/bin/butler login

~/bin/butler wipe zombusters_windows_beta.zip
rm -fr ZombustersPCWindowsBETA
mkdir ZombustersPCWindowsBETA
nuget restore ZombustersWindows.sln
msbuild publish /p:Configuration=Release ZombustersWindows.sln
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Release/netcoreapp3.1/publish/* ZombustersPCWindowsBETA
zip -r -v zombusters_windows_beta.zip ./ZombustersPCWindowsBETA
~/bin/butler push zombusters_windows_beta.zip retrowax/zombusters:windows-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW

~/bin/butler wipe zombusters_mac_beta.zip
rm -fr ZombustersMacBETA
mkdir ZombustersMacBETA
nuget restore ZombustersMac.sln
msbuild publish /p:Configuration=Release ZombustersMac.sln
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Release/netcoreapp3.1/publish/* ZombustersMacBETA
zip -r -v zombusters_mac_beta.zip ./ZombustersMacBETA
~/bin/butler push zombusters_mac_beta.zip retrowax/zombusters:mac-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW

~/bin/butler wipe zombusters_linux_beta.zip
rm -fr ZombustersLinuxBETA
mkdir ZombustersLinuxBETA
nuget restore ZombustersLinux.sln
msbuild publish /p:Configuration=Release ZombustersLinux.sln
mv /home/travis/build/retrowax/Zombusters-Enhanced-Edition/ZombustersWindows/bin/Release/netcoreapp3.1/publish/* ZombustersLinuxBETA
zip -r -v zombusters_linux_beta.zip ./ZombustersLinuxBETA
~/bin/butler push zombusters_linux_beta.zip retrowax/zombusters:linux-beta --userversion $MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW
