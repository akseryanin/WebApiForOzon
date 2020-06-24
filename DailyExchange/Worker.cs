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
    public class Worker : BackgroundService
    {
        private Timer _timer;
        private DateTime _lastDateUpdate = new DateTime();
        private readonly ILogger<Worker> _logger;
        private readonly IExchnageRateRepository _repository;

        public Worker(ILogger<Worker> logger, IExchnageRateRepository repository, IConfiguration configuration)
        {
            _logger = logger;
            _repository = repository;
            // Задаем время в секундах
            if(!int.TryParse(configuration["Period"] ?? "86400", out int period))
                period = 86400;
            _timer = new Timer(this.UpdateExchangeRates, null, 0, period * 1000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Passed 10 seconds");
            await Task.Delay(10000);
        }

        private void UpdateExchangeRates(object stateInfo)
        {
            // TODO should we wait for it?
            GetUpdatesAsync().Wait();
        }

        private async Task GetUpdatesAsync()
        {
            // Защита от скачивания повторных данных
            if (_lastDateUpdate == DateTime.Today)
                return;
            var url = 
                $"https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/daily.txt?date={DateTime.Today.ToString("dd.MM.yyyy")}";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            //To add result into DB
            await _repository.AddRangeAsync(ParseFromResponse(await response.Content.ReadAsStreamAsync()));
            _lastDateUpdate = DateTime.Today;
        }

        
        // Чуть более усложненный вариант
        private IEnumerable<ExchangeRate> ParseFromResponse(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var lines = ArrayPool<string>.Shared.Rent(50);
            int index = 0;
            var span = ReadOnlySpan<char>.Empty; // to store lines 
            string line;
            
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                lines[index++] = line;
            for(var i = 2; i < index; i++)
            {
                span = lines[i].AsSpan();

                var indexOfSep = span.LastIndexOf('|');
                var rate = span.Slice(indexOfSep + 1);
                span = span.Slice(0, indexOfSep);
            
                indexOfSep = span.LastIndexOf('|');
                var code = span.Slice(indexOfSep + 1);
                span = span.Slice(0, indexOfSep);
            
                indexOfSep = span.LastIndexOf('|');
                var amount = span.Slice(indexOfSep + 1);
                yield return new ExchangeRate
                {
                    Date = DateTime.Today,
                    Code = code.ToString(),
                    Rate = decimal.Parse(rate) / int.Parse(amount)
                };
            }
            ArrayPool<string>.Shared.Return(lines);
        }
    }
}