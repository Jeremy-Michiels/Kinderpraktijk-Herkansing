using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using src.Areas.Identity.Data;

[assembly: HostingStartup(typeof(src.Areas.Identity.IdentityHostingStartup))]
namespace src.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            /*
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<srcContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("srcContextConnection")));

                services.AddDefaultIdentity<srcUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<srcContext>();
            });
            */
        }
    }
}
