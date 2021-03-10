
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services;
using SprintPlannerZM.Services.Abstractions;
using SprintPlannerZM.Ui.Mvc.Settings;

namespace SprintPlannerZM.Ui.Mvc
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
            //registreren van de Settings.AppSettings
            var appSettings = new AppSettings();
            Configuration.Bind("AppSettings", appSettings);
            services.AddSingleton(appSettings);


            services.AddControllersWithViews();

            var connectionString = Configuration.GetConnectionString("TIHFDbContext");
            services.AddDbContext<TihfDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);


            services.AddScoped<IDeadlineService, DeadlineService>();
            services.AddScoped<IBeheerderService, BeheerderService>();
            services.AddScoped<ILeerlingService, LeerlingService>();
            services.AddScoped<IKlasService, KlasService>();
            services.AddScoped<ILeerkrachtService, LeerkrachtService>();
            services.AddScoped<IVakService, VakService>();
            services.AddScoped<IExamenroosterService, ExamenroosterService>();
            services.AddScoped<ILokaalService, LokaalService>();
            services.AddScoped<IHulpleerlingService, HulpleerlingService>();
            services.AddScoped<ISprintvakkeuzeService, SprintvakkeuzeService>();
            services.AddScoped<ISprintlokaalreservatieService, SprintlokaalreservatieService>();
            services.AddScoped<ILeerlingverdelingService, LeerlingverdelingService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {


                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Beheerder}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
