using System;
using WebApplication.ExchnageRateDb;

namespace ExchangeArchive
{
    class Program
    {
        static void Main(string[] args)
        {
            string conn = ""; //костыль для одноразового запуска
            IExchnageRateRepository _repo = new ExchangeRateRepository(conn);
            Parser p = new Parser();
            var a = _repo.AddRangeAsync(p.RequestArchive()).Result;
        }
    }
}
