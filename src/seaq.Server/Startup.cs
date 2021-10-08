using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using seaq;
using Serilog;

namespace SEAQ.Server
{
    public class Startup
    {
        protected const string Url = "https://hmelas01.okstovall.com:9200/";
        protected const string Username = "elastic";
        protected const string Password = "elastic";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var args = new ClusterArgs("", Url, Username, Password, true);
            var cluster = Cluster.Create(args);

            services.AddSingleton(cluster);

            services.AddControllers()
                .AddNewtonsoftJson(x => 
                {
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    
                });
            //    .AddJsonOptions(x =>
            //{
            //    x.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            //    x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            //});
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SEAQ.Server", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SEAQ.Server v1"));
                app.UseCors(x => x.AllowAnyMethod().AllowAnyOrigin().AllowAnyOrigin().AllowAnyHeader());
            }



            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseSerilogRequestLogging();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
