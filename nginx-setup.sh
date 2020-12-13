DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

sudo cp $DIR/nginx-sites-available-default /etc/nginx/sites-available/default
mkdir -p ~/ssl/marketmakerpoker
cd ~/ssl/marketmakerpoker
openssl genrsa -des3 -out server.key 2048
openssl rsa -in server.key -out server.key
openssl req -new -key server.key -out server.csr
openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt
sudo mkdir -p /etc/nginx/ssl/marketmakerpoker
sudo cp ~/ssl/marketmakerpoker/* /etc/nginx/ssl/marketmakerpoker