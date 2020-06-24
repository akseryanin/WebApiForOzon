using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.ExchnageRateDb;

namespace WebApi
{
    /// <summary>
    /// Класс представление для недельного отчета
    /// </summary>
    public class WeekReport
    {
        public int StartDay { get; set; }
        public int FinishDay { get; set; }
        public decimal Max { get; set;  }
        public decimal Min { get; set; }
        public decimal Median { get; set; }
    }
    /// <summary>
    /// Класс для месечного отчета
    /// </summary>
    public class MonthReport
    {
        private IEnumerable<ExchangeRate> _data;
        private string _cur;
        private List<WeekReport> reports;

        public string Currency
        {
            get { return _cur; }
        }
        public List<WeekReport> Reports
        {
            get { return reports; }
        }
        public MonthReport(string cur, IEnumerable<ExchangeRate> data)
        {
            _data = data;
            _cur = cur;
        }

        public MonthReport Get()
        {
            reports = GetREport();
            return this;
        }
        public List<WeekReport> GetREport()
        {
            var rate = _data.Where(x => x.Code == _cur && x.Date.DayOfWeek < DayOfWeek.Saturday).OrderBy(x => x.Date).ToArray(); //отфильтруем рабочие дни
            if (rate.Count() == 0)
                return null;
            List<List<ExchangeRate>> splitWeeks = new List<List<ExchangeRate>> { };
            splitWeeks.Add(new List<ExchangeRate>());
            for (int i = 1; i < rate.Length; ++i)
            {
                splitWeeks[splitWeeks.Count - 1].Add(rate[i - 1]);
                if ((rate[i].Date - rate[i - 1].Date).TotalDays > 1) //если разница больше дня, то значит наступила следующая рабочая неделя
                    splitWeeks.Add(new List<ExchangeRate>());
            }
            splitWeeks[splitWeeks.Count - 1].Add(rate[rate.Length - 1]);

            List<WeekReport> ans = new List<WeekReport> { };
            foreach (var week in splitWeeks)
            {
                ans.Add(
                    new WeekReport
                    {
                        StartDay = week[0].Date.Day,
                        FinishDay = week[week.Count - 1].Date.Day,
                        Max = week.Max(x => x.Rate),
                        Min = week.Min(x => x.Rate),
                        Median = week.OrderBy(x => x.Rate).Select(x => x.Rate).ElementAt(week.Count / 2)
                    }
                    ) ;
            }
            return ans;

        }


    }
}
