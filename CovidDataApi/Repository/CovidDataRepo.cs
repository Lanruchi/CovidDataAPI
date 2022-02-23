using System;
using CsvHelper;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Net;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Configuration;
using System.Dynamic;
using System.Data;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using CovidDataApi.Domain;
using Microsoft.AspNetCore.Hosting;

namespace CovidDataApi.Repository
{
    public class CovidDataRepo : ICovidDataRepo
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CovidDataRepo(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<IDictionary<string, string>>> GetCovidDataByLocationAsync(Filter filter)
        {

            var dataList = new List<Dictionary<string, string>>();
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            filter.Location = filter.Location.Trim();

            if (string.IsNullOrEmpty(filter.Location))
            {
                throw new ArgumentException(nameof(filter.Location));
            }
            else
            {
                if (!string.IsNullOrEmpty(filter.FromDate) && !DateTime.TryParse(filter.FromDate, out startDate))
                {
                    throw new ArgumentException(nameof(filter.FromDate));
                }
                if (!string.IsNullOrEmpty(filter.ToDate) && !DateTime.TryParse(filter.ToDate, out endDate))
                {
                    throw new ArgumentException(nameof(filter.ToDate));
                }
            }

            var data = await GetCovidData();

            string jsonString = SerializeToJson(data);
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, string>>>(jsonString);


            if (startDate != default(DateTime) && endDate != default(DateTime))
            {
                string sDate = startDate.ToString("MM/dd/yy");
                string eDate = endDate.ToString("MM/dd/yy");

                foreach (var item in deserialized)
                {
                    foreach (var keyValue in item)
                    {
                        if (Regex.IsMatch(keyValue.Key, @"^\d"))
                        {
                            var dateKey = DateTime.Parse(keyValue.Key).ToString("M/d/yy");
                            if (DateTime.Parse(dateKey) < DateTime.Parse(sDate) ||
                                DateTime.Parse(dateKey) > DateTime.Parse(eDate))
                            {
                                item.Remove(dateKey);
                            }
                        }

                        if (keyValue.Value == filter.Location)
                        {
                            dataList.Add((Dictionary<string, string>)item);
                        }
                    }

                }
            }
            else if (startDate == default(DateTime) && endDate == default(DateTime))
            {
                foreach (var item in deserialized)
                {
                    foreach (var keyValue in item)
                    {
                        if (keyValue.Value == filter.Location)
                        {
                            dataList.Add((Dictionary<string, string>)item);
                        }
                    }

                }
            }

            else if (startDate != default(DateTime) && endDate == default(DateTime))
            {
                string sDate = startDate.ToString("MM/dd/yy");
                foreach (var item in deserialized)
                {
                    foreach (var keyValue in item)
                    {
                        if (Regex.IsMatch(keyValue.Key, @"^\d"))
                        {
                            var dateKey = DateTime.Parse(keyValue.Key).ToString("M/d/yy");
                            if (DateTime.Parse(dateKey) < DateTime.Parse(sDate))
                            {
                                item.Remove(dateKey);
                            }
                        }

                        if (keyValue.Value == filter.Location)
                        {
                            dataList.Add((Dictionary<string, string>)item);
                        }
                    }

                }
            }

            else if (startDate == default(DateTime) && endDate != default(DateTime))
            {
                string eDate = endDate.ToString("M/d/yy");
                foreach (var item in deserialized)
                {
                    foreach (var keyValue in item)
                    {
                        if (Regex.IsMatch(keyValue.Key, @"^\d"))
                        {
                            var dateKey = DateTime.Parse(keyValue.Key).ToString("M/d/yy");
                            if (DateTime.Parse(dateKey) > DateTime.Parse(eDate))
                            {
                                item.Remove(dateKey);
                            }
                        }

                        if (keyValue.Value == filter.Location)
                        {
                            dataList.Add((Dictionary<string, string>)item);
                        }
                    }

                }
            }
            return dataList;
        }

        /// <summary>
        ///  download a fresh copy locally if not exist in folder path OR
        ///  if file exist but date of original file is pver 24 hrs
        /// </summary>
        /// <returns></returns>
        public async Task<List<dynamic>> GetCovidData()
        {
            /*
             * download a fresh copy locally if not exist in folder path 
             * OR  
             * if file exist but date of original file is pver 24 hrs
             */
            var CsvRecords = new List<dynamic>();
            string folderName = "\\TempFile\\";
            var fileName = "covidData.csv";
            var path = _webHostEnvironment.WebRootPath + folderName + fileName;

            DateTime fileTime = File.GetLastWriteTime(path);
            TimeSpan elapsed = DateTime.Parse(DateTime.Now.ToString()).Subtract(DateTime.Parse(fileTime.ToString()));

            if (!File.Exists(path) || elapsed.TotalHours > 24)
            {
                using (var client = new WebClient())
                {
                    var csvDataUrl = "https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_US.csv";
                    var uri = new Uri(csvDataUrl);
                    using (var stream = await client.OpenReadTaskAsync(uri))
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                            {
                                CsvRecords = csvReader.GetRecords<dynamic>().ToList();
                            }
                        }
                    }
                }

                if (!Directory.Exists(_webHostEnvironment.WebRootPath + folderName))
                {
                    Directory.CreateDirectory(_webHostEnvironment.WebRootPath + folderName);
                }

                if (!File.Exists(path))
                {
                    //File.Create(path);
                    using (TextWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
                    {
                        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                        csv.WriteRecords(CsvRecords); // where values implements IEnumerable
                    }
                }
                else
                {
                    File.Delete(path);
                    using (TextWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
                    {
                        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                        csv.WriteRecords(CsvRecords); // where values implements IEnumerable
                    }
                }

            }
            else
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    CsvRecords = csv.GetRecords<dynamic>().ToList();
                }
            }

            return CsvRecords;
        }

        private static string SerializeToJson(List<dynamic> records)
        {
            return JsonSerializer.Serialize(records, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }
    }
}
