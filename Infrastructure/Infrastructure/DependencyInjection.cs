using Domain;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            Console.WriteLine("Configurando los Repositorios...");
            var c = configuration.GetConnectionString("DefaultConnection");

            //aca van los repositorios
            services.AddScoped<IFarmRepository, FarmRepository>();
            services.AddScoped<ISalaRepository, SalaRepository>();
            Console.WriteLine("Repositorios configurados.");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(c);
                Console.WriteLine("Cadena de conexi�n utilizada: " + c);
            });
            return services;
        }
    }
}
