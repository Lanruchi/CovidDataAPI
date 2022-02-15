using CovidDataApi.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CovidDataApi.Services
{
    public interface ICovidDataService
    {
        Task<IEnumerable<IDictionary<string, string>>> GetCovidDataAsync(Filter filter);
        Task<IEnumerable<IDictionary<string, string>>> GetCovidDataDailyBreakDown();
        Task<IEnumerable<IDictionary<string, string>>> GetCovidRateOfNewCases();
    }
}
