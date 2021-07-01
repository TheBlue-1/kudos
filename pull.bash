#!/bin/bash
#pull
cd ../../../../../..
echo "im at $PWD"
git restore ./update.bash
git restore ./pull.bash
sudo git pull
sudo chmod 777 ./update.bash
sudo chmod 777 ./pull.bash