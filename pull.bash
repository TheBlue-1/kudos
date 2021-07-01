#!/bin/bash
#pull
cd Kudos/bin/Release/net5.0/linux-x64/publish #only for manual execution, bot is already there
cd ../../../../../..
echo "im at $PWD"
git restore ./update.bash
git restore ./pull.bash
sudo git pull
sudo chmod 777 ./update.bash
sudo chmod 777 ./pull.bash