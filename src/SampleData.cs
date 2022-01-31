using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class SampleData{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var serviceScope = serviceProvider;
        MijnContext context = (MijnContext)serviceScope.GetService(typeof(MijnContext));
        string[] roles = new string[] { "Ouder", "Client", "Pedagoog", "Moderator"};

        foreach (string role in roles)
        {
            var roleStore = new RoleStore<IdentityRole>(context);

            if (!context.Roles.Any(r => r.Name == role))
            {
                roleStore.CreateAsync(new IdentityRole(role));
            }
        }
        context.SaveChangesAsync();
    }
}