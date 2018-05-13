using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;

namespace Cert
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory().ToString()).ToString()).ToString(), "SakuraBot"))
                .AddJsonFile("appsettings.json")
                .Build();
            var botToken = configuration.GetSection("Bot:Token").Value;
            if (args.Length > 0)
            {
                var uri = args[0];
                var client = new TelegramBotClient(botToken);
                var cert = new Telegram.Bot.Types.InputFiles.InputFileStream(System.IO.File.Open("./cert.pem", FileMode.Open));
                Task t = client.SetWebhookAsync(uri, cert);
                t.Wait();
                var v = client.GetWebhookInfoAsync();
                v.Wait();
                var r = v.Result;
            }
            else {
                Console.WriteLine("No URL Sent");
            }
        }
    }
}