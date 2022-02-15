using CovidDataApi.Domain;
using CovidDataApi.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CovidDataApi.Services
{
    public class CovidDataService : ICovidDataService
    {
        private readonly ICovidDataRepo _repo;

        public CovidDataService(ICovidDataRepo repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<IDictionary<string, string>>> GetCovidDataAsync(Filter filter)
        {
            var result = await _repo.GetCovidDataByLocationAsync(filter);
            var x = SanitizeResponse(result);
            return result;
        }

        public Task<IEnumerable<IDictionary<string, string>>> GetCovidDataDailyBreakDown()
        {//Show a daily breakdown of the
         //number of cases per day for the given location and date range,
         //both total cases AND new cases each day.

            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDictionary<string, string>>> GetCovidRateOfNewCases()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, string>> SanitizeResponse(IEnumerable<IDictionary<string, string>> data)
        {
            List<string> k = new List<string> { "uid", "iso2", "iso3", "code3", "fips", "country_region", "combined_key" };

            foreach (var item in data)
            {
                foreach (var keyValue in item)
                {
                    if (k.Contains(keyValue.Key.ToLower()))
                    {
                        item.Remove(keyValue.Key);
                    }
                }
            }
            return data;
        }
    }
}
