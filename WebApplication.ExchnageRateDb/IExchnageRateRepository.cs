using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WebApplication.ExchnageRateDb
{
    public interface IExchnageRateRepository
    {
        /// <summary>
        /// Добавление списка курсов валют
        /// </summary>
        /// <param name="rates"></param>
        /// <returns></returns>
        Task<bool> AddRangeAsync(IEnumerable<ExchangeRate> rates);

        /// <summary>
        /// Добавление одного курса 
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        Task<bool> AddAsync(ExchangeRate rate);

        /// <summary>
        /// Получаем список. Чуть облегчим себе жизнь в виде предиката
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<ExchangeRate>> GetAsync(Expression<Func<ExchangeRate, bool>> predicate);
    }
}