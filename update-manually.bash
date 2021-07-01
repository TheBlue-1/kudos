echo "give permissions (step 1/4)"
sudo chmod 777 ./pull.bash
echo "pull changes (step 2/4)"
./pull.bash
echo "update bot (step 3/4)"
./update.bash
echo "success (step 4/4)"