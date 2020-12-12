echo "Copying files"
cd /home/vagrant
cp /vagrant/makefile .
rm -rf src
cp -r /vagrant/src .
rm -rf data
cp -r /vagrant/data .
mkdir deploy
cp -r /vagrant/deploy/Dockerfile deploy
echo "Ready to run make"