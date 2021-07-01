#!/bin/bash
#stop old version
pid=$(cat 'last.pid')
kill $pid
#build
cd ../../../../../..
dotnet restore
dotnet publish --configuration Release --framework net5.0 -p:PublishSingleFile=false -p:PublishTrimmed=false --runtime linux-x64 --self-contained true
#run new version
cd Kudos/bin/Release/net5.0/linux-x64/publish
nohup ./Kudos &>> runningLog.txt &
echo "$!" > last.pid