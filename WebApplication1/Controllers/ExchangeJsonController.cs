using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebApplication.ExchnageRateDb;
using Microsoft.Extensions.Configuration;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeJsonController : ControllerBase
    {
        private readonly ILogger<ExchangeJsonController> _logger;
        private readonly IExchnageRateRepository _repo;
        private readonly string[] _currencies;

        public ExchangeJsonController(IExchnageRateRepository repo, ILogger<ExchangeJsonController> logger, IConfiguration config)
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
                var ans = await _repo.GetAsync(rate => rate.Date >= start && rate.Date < finish); //фильтруем по границе дат
                if (ans.Count() == 0)
                    return new NotFoundResult();
                var reports = _currencies.Select(x => JsonSerializer.Serialize(new MonthReport(x, ans).Get())).ToArray();
                return new ObjectResult(reports);
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }
        }
    }
}
