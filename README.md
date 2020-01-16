# SakuraBot

Sakura is a very powerful telegram bot built to do many things. It can

- Greet new users
- Ban people, warn people and mute people from groups
- Fetch Dota 2 match information of completed videogames and generate match scorecards using the OpenDota API

## Steps to Run

1. Install Dotnet Core, MongoDB
2. Start MongoD
3. Set up a bot via BotFather and get the API Token
4. Expose your port 5000 using ngrok as http. Copy the HTTPS URL from NGROK.
5. Run a POST request on https://api.telegram.org/bot{BOT-TOKEN}/setWebhook with body [url: {HTTPS-URL}/api/update] in form-data
6. In AppSettings.json
    - Set your Bot Token
    - Set your Database Connection String to "mongodb://localhost:27017"
7. Run Dotnet Run

## Steps to Run in a Docker Container

1. Install Dotnet Core, Docker (v17 and up), Docker Compose
2. Set up a bot via BotFather and get the API Token
3. Expose your port 5000 using ngrok as http. Copy the HTTPS URL from NGROK.
4. Run a POST request on https://api.telegram.org/bot{BOT-TOKEN}/setWebhook with body [url: {HTTPS-URL}/api/update] in form-data
5. In AppSettings.json
    - Set your Bot Token
    - Set your Database Connection String to "mongodb://mongodb:27017"
5. Run `docker-compose build`
6. Run `docker-compose up`
7. Try out your bot, it should echo everything you say to it
