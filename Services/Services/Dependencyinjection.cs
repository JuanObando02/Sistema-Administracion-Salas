using Microsoft.Extensions.DependencyInjection;

namespace Services
{
    public static class Dependencyinjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
          
            services.AddTransient<IFarmService, FarmService>();
            services.AddTransient<ISalaService, SalaService>();


            return services;
        }
    }
}
