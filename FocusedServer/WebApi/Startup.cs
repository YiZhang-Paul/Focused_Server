using Core.Configurations;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Repositories;
using Service.Services;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WebApi.AppStart;

namespace WebApi
{
    [ExcludeFromCodeCoverage]
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
            services.AddCors(options =>
            {
                options.AddPolicy("focused-cors", _ => _.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod());
            });

            services.AddControllers();
            services.AddScoped<IWorkItemRepository, WorkItemRepository>();
            services.AddScoped<ITimeSeriesRepository, TimeSeriesRepository>();
            services.AddScoped<IFocusSessionRepository, FocusSessionRepository>();
            services.AddScoped<IBreakSessionRepository, BreakSessionRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IWorkItemService, WorkItemService>();
            services.AddScoped<IFocusSessionService, FocusSessionService>();
            services.AddScoped<IBreakSessionService, BreakSessionService>();
            services.AddScoped<IPerformanceService, PerformanceService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.Configure<DatabaseConfiguration>(Configuration.GetSection(DatabaseConfiguration.Key));
            CustomBsonSerializers.Register();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(_ => _.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                var payload = new { Error = $"{exception.Message} {exception.StackTrace}" };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }));

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("focused-cors");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
