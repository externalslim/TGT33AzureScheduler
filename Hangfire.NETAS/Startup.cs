using System;
using Hangfire.MemoryStorage;
using Hangfire.NETAS.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hangfire.NETAS
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseDefaultTypeSerializer()
            .UseMemoryStorage());

            services.AddHangfireServer();

            services.AddSingleton<IBlobStorage, BlobStorage>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env, 
            IBackgroundJobClient backgroundJobClient, 
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider
            )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseHangfireDashboard();
            backgroundJobClient.Enqueue(() => Console.WriteLine("Hello Hangfire Job"));


            /*cron hesaplamasý için bu site kullanýlabilir
            * 
            * https://crontab.guru/#0_00_*_*_*
            * 
            */

            recurringJobManager.AddOrUpdate(
                "Azure remove image from temp file",
                () => serviceProvider.GetService<IBlobStorage>().RemoveImageFromTempFile(),
                "0 00 * * *"
                );
        }
    }
}
