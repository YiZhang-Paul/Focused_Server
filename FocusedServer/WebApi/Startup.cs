using Core.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Repositories;
using Service.Services;
using System.Text.Json;

namespace WebApi
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
            services.AddCors(options =>
            {
                options.AddPolicy("focused-cors", _ => _.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod());
            });

            services.AddControllers();
            services.AddScoped<WorkItemRepository, WorkItemRepository>();
            services.AddScoped<TimeSeriesRepository, TimeSeriesRepository>();
            services.AddScoped<FocusSessionRepository, FocusSessionRepository>();
            services.AddScoped<BreakSessionRepository, BreakSessionRepository>();
            services.AddScoped<UserProfileRepository, UserProfileRepository>();
            services.AddScoped<WorkItemService, WorkItemService>();
            services.AddScoped<FocusSessionService, FocusSessionService>();
            services.AddScoped<BreakSessionService, BreakSessionService>();
            services.AddScoped<PerformanceService, PerformanceService>();
            services.Configure<DatabaseConfiguration>(Configuration.GetSection(DatabaseConfiguration.Key));
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
