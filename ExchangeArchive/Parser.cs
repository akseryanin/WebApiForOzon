using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using WebApplication.ExchnageRateDb;

namespace ExchangeArchive
{
    /// <summary>
    /// Просто парсер архивных данных
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Сам парс
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        private IEnumerable<ExchangeRate> ParseResponse(WebResponse resp)
        {
            using (Stream stream = resp.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = reader.ReadLine();
                    string[] currency = line.Split('|');
                    Tuple<string, int>[] data = new Tuple<string, int>[currency.Length - 1];
                    for (int i = 1; i < currency.Length; ++i)
                    {
                        string[] tp = currency[i].Split(' ');
                        string curren = tp[1];
                        int par = int.Parse(tp[0]);
                        data[i - 1] = new Tuple<string, int>(curren, par);
                    }
                    List<ExchangeRate> list = new List<ExchangeRate> { };
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] entities = line.Split('|');
                        DateTime date = DateTime.ParseExact(entities[0], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        string[] values = entities.Select(v => v.Replace('.', ',')).Skip(1).ToArray();
                        for (int i = 0; i < values.Length; ++i)
                        {
                            list.Add(new ExchangeRate
                            {
                                Date = date,
                                Code = data[i].Item1,
                                Rate = decimal.Parse(values[i]) / data[i].Item2
                            }
                            );
                        }
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// Параллельные запросы к апи чешского банка
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExchangeRate> RequestArchive()
        {
            var urls = new[]
            {
                "https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/year.txt?year=2017",
                "https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/year.txt?year=2018"
            };

            var result =
                urls
                    .AsParallel()
                    .Select(url => ParseResponse(WebRequest.Create(url).GetResponse())).ToList();

            foreach (var col in result)
            {
                foreach (var elem in col)
                {
                    yield return elem;
                }
            }
        }
    }
}
