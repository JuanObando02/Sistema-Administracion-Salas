
using AutoMapper;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Services;
using Services.Automapper;

namespace MvcSample
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
            var _configuration = builder.Configuration;
            // Add services to the container.
            builder.Services.AddServices();
            builder.Services.AddRepositories(_configuration);
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            //configura user id como GUID
            builder.Services.AddIdentity<AppUser,IdentityRole>(options =>
            {
                // Configuraci�n de opciones (opcional)
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
            })

           
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/Login"; ;
                options.SlidingExpiration = true;
            });

            // Add automapper
            var mappingConfiguration = new MapperConfiguration (m => m.AddProfile(new MappingProfile()));
           
            IMapper mapper = mappingConfiguration.CreateMapper();
            
            builder.Services.AddSingleton(mapper);

            //builder.Services.AddCors(p => p.AddPolicy("CORS_Policy", builder =>
            //{
            //    CorsPolicyBuilder corsPolicyBuilder = builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); //builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
            //}));

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
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
            app.UseAuthentication();
            app.UseAuthorization();
           // app.UseCors("CORS_Policy");
           
           // app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {

                var services = scope.ServiceProvider;
                SeedRolesAndAdminUser(services).Wait();

            }


            app.Run();
        }


        static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // 1. Crear roles si no existen
            string[] roles = { "Admin", "User", "Master" };

            foreach (var roleName in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Crear usuario admin si no existe
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@example.com";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Administrador",
                    LastName = "Del Sistema",
                    DocumentNumber = "1000593423",
                    EmailConfirmed = true
                };

                var createUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createUser.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, roles);
                }
            }
            else
            {
                // Asegurar que tenga los roles
                var userRoles = await userManager.GetRolesAsync(adminUser);
                foreach (var role in roles.Where(role => !userRoles.Contains(role)))
                {
                    await userManager.AddToRoleAsync(adminUser, role);
                }
            }
        }


    }
}
