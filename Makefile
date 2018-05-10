build:
	sudo docker-compose build

run:
	sudo docker-compose up

cert:
	openssl req -newkey rsa:2048 -sha256 -nodes -keyout ./Nginx/Cert/cert.key -x509 -days 365 -out ./Nginx/Cert/cert.pem -subj "/C=US/ST=New York/L=Brooklyn/O=Example Brooklyn Company/CN=YOURDOMAIN.EXAMPLE"

clean:
	rm -rf ./bin ./obj ./Cert/*.pem ./Cert/*.key