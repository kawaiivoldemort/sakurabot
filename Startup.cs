using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Sakura.Uwu.Models;
using Sakura.Uwu.Services;

namespace Sakura.Uwu {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
            Console.WriteLine("MOO");
            services.AddScoped<IUpdateService, UpdateService>();
            services.AddSingleton<IBotService, BotService>();
            
            services.Configure<BotSettings>(
                options => {
                    options.DatabaseConnectionString = Configuration.GetSection("Database:ConnectionString").Value;
                    options.DatabaseName = Configuration.GetSection("Database:Name").Value;
                    options.BotToken = Configuration.GetSection("Bot:Token").Value;
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            app.UseMvc();
        }
    }
}