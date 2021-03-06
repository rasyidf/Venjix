using AutoMapper;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

using Serilog;

using System;
using System.Linq;

using Venjix.Infrastructure.AI;
using Venjix.Infrastructure.DAL;
using Venjix.Infrastructure.DataTables;
using Venjix.Infrastructure.DTO;
using Venjix.Infrastructure.Services;
 

namespace Venjix
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
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson();
            services.AddAuthentication(x =>
            {
                x.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                x.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/login/index";
                options.LogoutPath = "/login/logout";
                options.AccessDeniedPath = "/error/unauthorizedpage";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });
            services.AddAuthorization(config =>
            {
                config.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });
            services.AddAntiforgery(options =>
            {
                options.FormFieldName = "VAntiForgery";
                options.HeaderName = "X-CSRF-TOKEN";
                options.SuppressXFrameOptionsHeader = false;
            });

            services.AddDbContext<VenjixContext>(options => options.UseSqlite(Configuration.GetConnectionString("VenjixContext")));
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddHealthChecks()
                .AddDbContextCheck<VenjixContext>();
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(Startup));

            // register services
            services.AddTransient<IDataTables, DataTables>();
            services.AddTransient<IForecastingService, ForecastingService>();
            services.AddScoped<ITriggerRunnerService, TriggerRunnerService>();
            services.AddSingleton<ITelegramService, TelegramService>();
            services.AddSingleton<IVenjixOptionsService, VenjixOptionsService>(options =>
            {
                var instance = new VenjixOptionsService();
                instance.Reload().GetAwaiter().GetResult();
                return instance;
            });
             

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/index");
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(x => x
              .AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var response = new HealthCheckDto
                    {
                        Status = report.Status.ToString(),
                        HealthChecks = report.Entries.Select(x => new IndividualHealthCheckDto
                        {
                            Component = x.Key,
                            Status = x.Value.Status.ToString(),
                            Description = x.Value.Description
                        }),
                        HealthCheckDuration = report.TotalDuration
                    };

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "APIs",
                    pattern: "api/{controller=ApiData}/{action=SaveDataByQuery}/{id?}");
            });
        }
    }
}