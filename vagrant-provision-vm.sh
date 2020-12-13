echo "Installing commands ..."

set USR=vagrant
set GRP=vagrant

sudo mkdir -p /var/log/marketmakinggame
sudo chown $USR /var/log/marketmakinggame
sudo chgrp $GRP  /var/log/marketmakinggame
sudo mkdir -p /var/data/marketmakinggame
sudo chown $USR /var/data/marketmakinggame
sudo chgrp $GRP  /var/data/marketmakinggame

if [ ! -f /usr/local/bin/dotnet ]; then
  . /vagrant/dotnet-install.sh -i /usr/local/dotnet-sdk
  ln -s /usr/local/dotnet-sdk/dotnet /usr/local/bin/dotnet
fi

apt -y update
apt -y install make
apt -y install nginx

apt -y install docker.io
usermod -aG docker $USR
