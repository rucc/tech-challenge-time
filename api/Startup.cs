using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using pentoTrack.Services;
using static pentoTrack.Services.ITrackerRepository;

namespace pentoTrack
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        Config m_trackerCfg;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            m_trackerCfg = new Config();
            var clock = new SysClock();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "pentoTrack", 
                    Version = "v1",
                    Description = "A simple time tracking API",
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSingleton<Config>(m_trackerCfg);
            services.AddSingleton<IClock>(clock);
            var trackerRepo = new JsonFileTrackerRepo(m_trackerCfg, clock);
            services.AddSingleton<ITrackerRepository>(trackerRepo);
            var userRepo = new SingleUserRepository();
            services.AddSingleton<IUserRepository>(userRepo);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();
            app.UseMiddleware<RequestLoggingMiddleware>();
            m_trackerCfg.ContentRoot = env.ContentRootPath;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "pentoTrack v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate m_next;
        private readonly ILogger m_logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            m_next = next;
            m_logger = loggerFactory.CreateLogger("RequestLogger");
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await m_next(context);
            }
            finally
            {
                m_logger.LogInformation(
                    "Request {method} {url} => {statusCode}",
                    context.Request?.Method,
                    context.Request?.Path.Value,
                    context.Response?.StatusCode);
            }
        }
    }
}
