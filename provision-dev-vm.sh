echo "Installing commands ..."

if [ ! -f /usr/local/bin/dotnet ]; then
  . /vagrant/dotnet-install.sh -i /usr/local/dotnet-sdk
  ln -s /usr/local/dotnet-sdk/dotnet /usr/local/bin/dotnet
fi

apt -y update
sudo apt -y install docker.io
sudo apt -y install make