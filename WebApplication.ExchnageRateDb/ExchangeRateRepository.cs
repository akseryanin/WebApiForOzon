using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.ExchnageRateDb
{
    public class ExchangeRateRepository : IExchnageRateRepository
    {
        private ExchangeRateContext _context { get; set; }

        public ExchangeRateRepository(ExchangeRateContext context) => _context = context;

        public ExchangeRateRepository(string connectionString) =>
            _context = new ExchnageRateContextFactory().CreateDbContext(new[] {connectionString});
        public async Task<bool> AddRangeAsync(IEnumerable<ExchangeRate> rates)
        {
            try
            {
                await _context.Rates.AddRangeAsync(rates);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // место для логгера
                return false;
            }
        }

        public async Task<bool> AddAsync(ExchangeRate rate)
        {
            try
            {
                await _context.Rates.AddAsync(rate);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // место для логгера
                return false;
            }
        }

        public async ValueTask<IEnumerable<ExchangeRate>> GetAsync(Expression<Func<ExchangeRate, bool>> predicate) =>
            await _context.Rates.Where(predicate).ToListAsync();
    }
}