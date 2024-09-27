
using Elevator.Api.Extensions;

namespace Elevator.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddServices();

            builder.Build().AddPipelines().Run();
        }
    }
}
