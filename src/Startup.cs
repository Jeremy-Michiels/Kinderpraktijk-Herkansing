using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using src.Areas.Identity.Data;

namespace src
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
            services.AddControllersWithViews();

            //Hier Halen wij de naam van de database uit de Usersecrets
            services.AddDbContext<MijnContext>(o=>
                        o.UseSqlServer("Server=tcp:kinderpraktijkhij2.database.windows.net,1433;Initial Catalog=kinderpraktijkhij2;Persist Security Info=False;User ID=KinderpraktijkHij;Password=J0eBiden!123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));
                        //o.UseSqlite("Data Source=database.db"));
            
            //dit is nodig voor de identity
            services.AddIdentity<srcUser, IdentityRole>()
                                .AddEntityFrameworkStores<MijnContext>()
                                .AddDefaultUI()
                                .AddDefaultTokenProviders();
                                

            //Dit is nodig voor het gebruik van signal R
            services.AddSignalR();

                services.Configure<ReCAPTCHASettings>(o=>{
                     o.ReCAPTCHA_Site_Key = "6LcZdx8eAAAAAJRJY92iPPyF0Zn548LKM5us_LTT";
                     o.ReCAPTCHA_Sectret_Key = "6LcZdx8eAAAAAOIa93IF8r7aAZfh5HVjZWx8fmxP";
                });
            services.AddTransient<GooglereCAPTCHAService>();

            // requires
            // using Microsoft.AspNetCore.Identity.UI.Services;
            // using WebPWrecover.Services;
            //services.AddTransient<IEmailSender, EmailSender>();
            //services.Configure<AuthMessageSenderOptions>(Configuration);

            /* 
                Dit moet worden gedaan doordat het framework een preventie
                heeft voor een Cross site attack.

                Hiermee stel je vast dat je van deze website wel berichten
                zou willen ontvangen
            */
            services.AddCors(options=>
                options.AddDefaultPolicy(builder =>
                {
                    //dit moet later ook terug veranderd worden naar de website die gebruikt gaat worden
                    builder.WithOrigins("https://kinderpraktijkhij2.azurewebsites.net")
                    //builder.WithOrigins("https://localhost:5001/")
                            .AllowCredentials();
                })
            );
            services.AddHealthChecks();
            services.AddRazorPages();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Dit is nodig voor de werking van signalR
            app.UseCors(); 

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //Deze maphub moet hier geinitialiseerd zijn
                //Deze zorgt voor de verbinding
                endpoints.MapHub<ChatHub>("/chatHub");
                //endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
            //Hiermee zou de database geseed moeten worden.
            //SampleData.Initialize(app.ApplicationServices);
        }
    }
}
