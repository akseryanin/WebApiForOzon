using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApplication.ExchnageRateDb
{
    public class ExchangeRateContextFactory : IDesignTimeDbContextFactory<ExchangeRateContext>
    {
        public ExchangeRateContext CreateDbContext(string[] args)
        {
            var connection = args[0];
            var optionsBuilder = new DbContextOptionsBuilder<ExchangeRateContext>();
            optionsBuilder.UseSqlServer(connection);

            return new ExchangeRateContext(optionsBuilder.Options);
        }
    }
}