using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication.ExchnageRateDb;

namespace DailyExchange
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    services.AddDbContext<ExchangeRateContext>(builder =>
                    {
                        builder.UseSqlServer(configuration.GetConnectionString("Default"));
                    }, ServiceLifetime.Singleton);
                    services.AddSingleton<IExchnageRateRepository, ExchangeRateRepository>();
                    services.AddHostedService<Worker>();
                });
    }
}