using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PhoneExtractVerify.Api.Models;
using PhoneExtractVerify.Api.Services;
using PhoneExtractVerify.Api.Services.Interface;

namespace PhoneExtractVerify.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TwilioCredentials>(Configuration.GetSection("TwilioAccount"));
            services.Configure<AzureComputerVisionCredentials>(Configuration.GetSection("AzureComputerVisionCredentials"));

            services.AddTransient<IWordProcessingService, WordProcessingService>();
            services.AddTransient<ITwilioHelperService, TwilioHelperService>();
            services.AddTransient<IAzureComputerVisionHelperService, AzureComputerVisionHelperService>();
            services.AddTransient<IMediatorService, MediatorService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
