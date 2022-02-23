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
            try
            {
                var result = await _repo.GetCovidDataByLocationAsync(filter);
                return SanitizeResponse(result);
            }
            catch (Exception e)
            {
                //LOG ERROR in persistence place
                Console.WriteLine(e.Message);
                return null;
            }

        }

        public async Task<IEnumerable<IDictionary<string, string>>> GetCovidDataDailyBreakDown(Filter filter)
        {//Show a daily breakdown of the
         //number of cases per day for the given location and date range,
         //both total cases AND new cases each day.
            try
            {
                var result = await GetCovidDataAsync(filter);
                if (result == null)
                {
                    throw new Exception("No Covid data");
                }
                var totalDataList = new List<int>();

                foreach (var item in result)
                {
                    totalDataList.Clear();
                    foreach (var keyValue in item)
                    {
                        if (Regex.IsMatch(keyValue.Key, @"^\d"))//filter for dates
                        {
                            totalDataList.Add(Convert.ToInt32(keyValue.Value));
                        }
                    }


                    long total = 0;
                    foreach (var val in totalDataList)
                    {
                        total += val;
                    }
                    item.Add("total", total.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                //LOG ERROR in persistence place
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<IEnumerable<IDictionary<string, string>>> GetCovidRateOfNewCases(Filter filter)
        {
            try
            {
                var result = await GetCovidDataAsync(filter);
                if (result == null)
                {
                    throw new Exception("No Covid data");
                }

                int monthCounter = 0;
                int prevDateCases = 0;
                foreach (var item in result)
                {
                    for (int i = 0; i < item.Count; i++)
                    {
                        var data = item.ElementAt(i);
                        var itemKey = data.Key;
                        var itemValue = data.Value;

                        if (Regex.IsMatch(itemKey, @"^\d"))//filter for dates
                        {
                            int currentDateCases = int.Parse(itemValue);
                            if (monthCounter == 0 || (currentDateCases <= prevDateCases))
                            {
                                prevDateCases = int.Parse(item.ElementAt(i).Value);
                                item[itemKey] = $"{itemValue}, growthRate: 0";
                            }
                            else
                            {
                                var diff = currentDateCases - prevDateCases;
                                prevDateCases = int.Parse(item.ElementAt(i).Value);
                                item[itemKey] = $"{itemValue}, growthRate: {diff}";//Todo percentage
                            }

                            monthCounter++;
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                //LOG ERROR in persistence place
                Console.WriteLine(e.Message);
                return null;
            }
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
