
using Microsoft.EntityFrameworkCore;

namespace WebApplication.ExchnageRateDb
{
    public class ExchangeRateContext : DbContext
    {
        private readonly string _connectionString = "";

        public DbSet<ExchangeRate> Rates { get; set; }
        
        public ExchangeRateContext(DbContextOptions opts) : base(opts)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Индекс для ускорения поиска
            modelBuilder.Entity<ExchangeRate>().HasIndex(r => new {r.Date, r.Code});
            base.OnModelCreating(modelBuilder);
        }
    }
}