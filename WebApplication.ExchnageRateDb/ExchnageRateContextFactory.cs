using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApplication.ExchnageRateDb
{
    /// <summary>
    /// Для миграции
    /// </summary>
    internal class ExchnageRateContextFactory : IDesignTimeDbContextFactory<ExchangeRateContext>
    {
        public ExchangeRateContext CreateDbContext(string[] args)
        {
            var connectionString =
                "";
            if (args.Length != 0)
                connectionString = args[0];
            var optionsBuilder = new DbContextOptionsBuilder<ExchangeRateContext>();
            optionsBuilder.UseSqlServer(connectionString, builder => builder.MigrationsAssembly("WebApi"));

            return new ExchangeRateContext(optionsBuilder.Options);
        }
    }
}