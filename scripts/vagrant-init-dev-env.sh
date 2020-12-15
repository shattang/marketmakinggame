echo "Copying files"
cd /home/vagrant
pwd
cp -f /vagrant/makefile .
rm -rf src
cp -r /vagrant/src .
mkdir -p deploy
cp -r /vagrant/deploy/Dockerfile deploy
echo "Ready to run make"
