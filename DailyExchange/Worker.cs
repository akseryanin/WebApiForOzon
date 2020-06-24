using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication.ExchnageRateDb;

namespace DailyExchange
{
    /// <summary>
    /// Воркер по шаблону WorkerService
    /// </summary>
    public class Worker : BackgroundService
    {
        private int period = 5000;
        private Timer _timer;
        private DateTime _lastDateUpdate = new DateTime();
        private readonly ILogger<Worker> _logger;
        private readonly IExchnageRateRepository _repository;

        public Worker(ILogger<Worker> logger, IExchnageRateRepository repository, IConfiguration config)
        {
            _logger = logger;
            _repository = repository;
            period = config.GetSection("Period").Get<int>();
            _timer = new Timer(this.UpdateExchangeRates, null, 0, period);
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(period);
        }

        private void UpdateExchangeRates(object stateInfo)
        {
            GetUpdatesAsync().Wait();
        }

        private async Task GetUpdatesAsync()
        {
            if (_lastDateUpdate == DateTime.Today)
                return;
            var url = 
                $"https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/daily.txt?date={DateTime.Today.ToString("dd.MM.yyyy")}";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await _repository.AddRangeAsync(ParseFromResponse(await response.Content.ReadAsStreamAsync()));
            _lastDateUpdate = DateTime.Today;
            
            var res = ParseFromResponse(await response.Content.ReadAsStreamAsync());
            foreach (var elem in res)
            {
                Console.WriteLine($"{elem.Code} {elem.Rate}");
            }
        }

        /// <summary>
        /// Парс ежедневных данных
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private IEnumerable<ExchangeRate> ParseFromResponse(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var lines = ArrayPool<string>.Shared.Rent(50);
            int index = 0;
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                lines[index++] = line;
            for(var i = 2; i < index; i++)
            {
                var elems = lines[i].Split('|').Skip(2).ToList();
                yield return new ExchangeRate
                {
                    Date = DateTime.Today,
                    Code = elems[1],
                    Rate = decimal.Parse(elems[2]) / int.Parse(elems[0])
                };
            }
            ArrayPool<string>.Shared.Return(lines);
        }
    }
}