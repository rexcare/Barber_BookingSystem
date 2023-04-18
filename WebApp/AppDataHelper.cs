using System.Security.Claims;
using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebApp;

public static class AppDataHelper
{
    public static void SetupAppData(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

        using var context = serviceScope
            .ServiceProvider.GetService<AppDbContext>();

        if (context == null)
        {
            throw new ApplicationException("Problem in services. No db context.");
        }

        // TODO - Check database state
        // can't connect - wrong address
        // can't connect - wrong user/pass
        // can connect - but no database
        // can connect - there is database

        if (configuration.GetValue<bool>("DataInitialization:DropDatabase"))
        {
            context.Database.EnsureDeleted();
        }

        if (configuration.GetValue<bool>("DataInitialization:MigrateDatabase"))
        {
            context.Database.Migrate();
        }

        if (configuration.GetValue<bool>("DataInitialization:SeedIdentity"))
        {
            using var userManager = serviceScope.ServiceProvider.GetService<UserManager<AppUser>>();
            using var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<AppRole>>();

            var startTime = TimeOnly.Parse("08:00");
            var stopTime = TimeOnly.Parse("16:00");
            var workTT = new WorkTimeTemplate();
            
            workTT.StartTime = startTime;
            workTT.StopTime = stopTime;
            
            var companyInf = new CompanyInfo()
            {
                Id = 1,
                Address = "Luise 3-3",
                Email = "hsgf@gmail.com",
                Phone = "+3725632589",
                WorkTimeTemplateInfo = 14,
                GenerationDate =  DateTime.Today.Date
            };
            if (!context.CompanyInfos.Any())
            {
                context.CompanyInfos.Add(companyInf);
                context.SaveChanges();
            }





            if (userManager == null || roleManager == null)
            {
                throw new NullReferenceException("userManager or roleManager cannot be null!");
            }

            // if (context.WorkTimeTemplates == null)
            // {
            //     context.WorkTimeTemplates.Add(workTT);
            // }

            var roles = new (string name, string displayName)[]
            {
                ("admin", "System administrator"),
                ("user", "Normal system user"),
                ("customer", "Normal customer"),
            };

            foreach (var roleInfo in roles)
            {
                var role = roleManager.FindByNameAsync(roleInfo.name).Result;
                if (role == null)
                {
                    var identityResult = roleManager.CreateAsync(new AppRole()
                    {
                        Name = roleInfo.name,
                        DisplayName = roleInfo.displayName
                    }).Result;
                    if (!identityResult.Succeeded)
                    {
                        throw new ApplicationException("Role creation failed");
                    }
                }
            }

            var users = new (string username,string firstName,string lastName, string password, string roles)[]
            {
                ("admin@itcollege.ee","Admin","College", "Kala.maja1", "admin"),
                ("akaver@itcollege.ee","Andres","Käver", "Kala.maja1", "user"),
            };

            foreach (var userInfo in users)
            {
                var user = userManager.FindByEmailAsync(userInfo.username).Result;
                if (user == null)
                {
                    user = new AppUser()
                    {
                        Email = userInfo.username,
                        FirstName = userInfo.firstName,
                        LastName = userInfo.lastName,
                        UserName = userInfo.username,
                        Role = userInfo.roles,
                        PhoneNumber = "84754164",
                        WorkTimeTemplate = workTT,
                        EmailConfirmed = true,
                        
                        
                    };
                    
                    var identityResult = userManager.CreateAsync(user, userInfo.password).Result;
                    identityResult =  userManager.AddClaimAsync(user, new Claim("aspnet.firstname",user.FirstName)).Result;
                    identityResult =  userManager.AddClaimAsync(user, new Claim("aspnet.lastname",user.LastName)).Result;
                    identityResult = userManager.AddClaimAsync(user, new Claim("aspnet.phonenumber", user.PhoneNumber)).Result;

                    if (!identityResult.Succeeded)
                    {
                        throw new ApplicationException("Cannot create user!");
                    }
                }

                if (!string.IsNullOrWhiteSpace(userInfo.roles))
                {
                    var identityResultRole = userManager.AddToRolesAsync(user,
                        userInfo.roles.Split(",").Select(r => r.Trim())
                    ).Result;
                    user.Role = userInfo.roles;
                }
            }
        }
        //
        // if (configuration.GetValue<bool>("DataInitialization:SeedData"))
        // {
        //     var f = new FooBar
        //     {
        //         Name =
        //         {
        //             ["en"] = "english",
        //             ["et"] = "estonian",
        //             ["ru"] = "russian",
        //         }
        //     };
        //
        //     context.FooBars.Add(f);
        //     context.SaveChanges();
        // }
    }
}