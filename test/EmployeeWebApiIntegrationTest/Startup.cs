using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EmployeeWebApi.Models;
using EmployeeWebApi.Services;
using Microsoft.AspNet.Mvc.Formatters;
using Autofac;
using Autofac.Extensions.DependencyInjection;


namespace EmployeeWebApiIntegrationTest
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
              //  builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
           // Configuration["Data:DefaultConnection:ConnectionString"] = $@"Data Source={appEnv.ApplicationBasePath}/EmployeeWebApiIntegrationTest.db";
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {

            
            //Add framework services.
           //services.AddApplicationInsightsTelemetry(Configuration);

            services.AddEntityFramework()
                .AddSqlite()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
                var jsonOutputFormatter = new JsonOutputFormatter();
                jsonOutputFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                options.OutputFormatters.Clear();
                options.OutputFormatters.Add(jsonOutputFormatter);
            });

            //Add Entity Framework
            services.AddEntityFramework()
                       .AddSqlite()
                       .AddDbContext<EmployeeDbContext>(options =>
              options.UseSqlite
              (Configuration["Data:DefaultConnection:ConnectionString"]));

         
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            

            var builder = new ContainerBuilder();

            builder.RegisterModule(new AutofacModule());
            builder.Populate(services);

            var container = builder.Build();
           // DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            return container.Resolve<IServiceProvider>();
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

         //   app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                        // serviceScope.ServiceProvider.GetService<EmployeeDbContext>()
                        //      .Database.Migrate();
                    }
                }
                catch { }
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

          //  app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
