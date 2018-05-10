using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore;

using Sakura.Uwu.Models;
using Sakura.Uwu.Services;

using Npgsql.EntityFrameworkCore;

namespace Sakura.Uwu {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<BotDbContext>
            (
                options => options
                .UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
                .EnableSensitiveDataLogging()
            )
            .AddEntityFrameworkNpgsql();
            
            services.AddMvc();
            
            services.AddSingleton<IBotService, BotService>();

            services.AddScoped<IUpdateService, UpdateService>();
            
            services.Configure<BotSettings>(
                options => {
                    options.BotToken = Configuration.GetSection("Bot:Token").Value;
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<BotDbContext>();
                context.Database.EnsureCreated();
            }
            app.UseMvc();
        }
    }
}   