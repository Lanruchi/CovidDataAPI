
using CovidDataApi.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace CovidDataApi.Repository
{
    public interface ICovidDataRepo
    {
        Task<IEnumerable<IDictionary<string, string>>> GetCovidDataByLocationAsync(Filter filter);
    }
}
