using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi;
using WebApplication.ExchnageRateDb;
using System.Text.Json;
using System.IO;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeTxtController : ControllerBase
    {

        private readonly ILogger<ExchangeTxtController> _logger;
        private readonly IExchnageRateRepository _repo;
        private readonly string[] _currencies;

        public ExchangeTxtController(IExchnageRateRepository repo, ILogger<ExchangeTxtController> logger, IConfiguration config)
        {
            _logger = logger;
            _repo = repo;
            _currencies = config.GetSection("Currencies").Get<string[]>();
        }

        [HttpGet("{year:int}/{month:int}")]
        public async Task<IActionResult> Get(int year, int month)
        {
            try
            {
                DateTime start = new DateTime(year, month, 1);
                DateTime finish;
                if (start.Month == 12)
                    finish = new DateTime(year + 1, 1, 1);
                else
                    finish = new DateTime(year, month + 1, 1);
                var ans = await _repo.GetAsync(rate => rate.Date >= start && rate.Date < finish);
                if (ans.Count() == 0)
                    return new NotFoundResult();
                var reports = _currencies.Select(x => new MonthReport(x, ans).Get()).ToArray();
                FileStream fs = new FileStream("report.txt", FileMode.OpenOrCreate);
                string rep = "";
                foreach(var cur in reports) //вывод в формате txt
                {
                    rep += cur.Currency + '\n';
                    foreach (var week in cur.Reports)
                    {
                        rep += $"{week.StartDay}...{week.FinishDay} min: {week.Min}; max: {week.Max}; median: {week.Median}\n";
                    }
                }
                return new ObjectResult(rep);
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }
           
        }
    }
}
