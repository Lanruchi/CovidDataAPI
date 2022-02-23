using CovidDataApi.Domain;
using CovidDataApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CovidDataApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CovidController : ControllerBase
    {
        private readonly ICovidDataService _service;
        public CovidController(ICovidDataService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get list of Covid 19 data
        /// </summary>
        /// <param name="filter"> The location and date range needed for filtered data</param>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<IDictionary<string,string>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetCovidData(Filter filter)
        {
            var data = _service.GetCovidDataAsync(filter).Result;
            if(data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        /// <summary>
        /// Get list of both total cases AND new cases each day
        /// </summary>
        /// <param name="filter"> The location and date range needed for filtered data</param>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<IDictionary<string, string>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetDailyBreakDown(Filter filter)
        {
            var data = _service.GetCovidDataDailyBreakDown(filter).Result;
            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        /// <summary>
        /// Get growth rate of new cases
        /// </summary>
        /// <param name="filter"> The location and date range needed for filtered data</param>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<IDictionary<string, string>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetGrowthRate(Filter filter)
        {
            var data = _service.GetCovidRateOfNewCases(filter).Result;
            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }
    }
}
