using Awesome.Core.ApiResult;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;

namespace Example.Api
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
            string[] origins = Configuration.GetValue<string>("AllowedHosts").Split(",");
            //
            services.AddCors(options => options.AddPolicy("AllowedHosts", builder => builder.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader()));
            //db

            //same format of api results
            services.AddControllers(options =>
            {
                options.Filters.Add<ActionExecuteFilter>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // 统一认证
            //var IdentityServerUri = Configuration["IDENTITYSERVER_URI"];
            //if (string.IsNullOrWhiteSpace(IdentityServerUri)) throw new("no IDENTITYSERVER_URI setting in env");
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            //{
            //    options.Authority = IdentityServerUri;
            //    options.RequireHttpsMetadata = false;
            //    options.TokenValidationParameters.ValidateAudience = false;
            //});
            //IdentityModelEventSource.ShowPII = true;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "Example.Api",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.OperationFilter<SwaggerOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Example.Api v1"));
            }
            app.UseSerilogRequestLogging();
            app.UseGlobalException();
            app.UseResponseTime();

            app.UseCors("AllowedHosts");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
