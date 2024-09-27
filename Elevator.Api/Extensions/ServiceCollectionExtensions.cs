using Elevator.Api.Services;

namespace Elevator.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSingleton<IElevatorService, ElevatorService>();

            return services;
        }
    }
}
