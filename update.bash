#pull
git pull
#build
dotnet restore
dotnet publish --configuration Release --framework net5.0 -p:PublishSingleFile=false -p:PublishTrimmed=false --runtime linux-x64 --self-contained true
#stop old version
cd Kudos/bin/Release/net5.0/linux-x64/publish
pid=$(cat 'last.pid')
kill $pid
#run new version
nohup ./Kudos &>> runningLog.txt &
echo "$!" > last.pid