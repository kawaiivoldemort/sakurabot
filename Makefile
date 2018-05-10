build:
	sudo docker-compose build

run:
	sudo docker-compose up

cert:
	openssl req -newkey rsa:2048 -nodes -keyout ./Nginx/Cert/cert.key -x509 -days 365 -out ./Nginx/Cert/cert.pem

clean:
	rm -rf ./bin ./obj ./Cert/*.pem ./Cert/*.key