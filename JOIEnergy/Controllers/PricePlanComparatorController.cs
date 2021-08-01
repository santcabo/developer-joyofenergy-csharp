using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JOIEnergy.Controllers
{
    [Route("price-plans")]
    public class PricePlanComparatorController : Controller
    {
        private readonly IPricePlanService _pricePlanService;
        private readonly IAccountService _accountService;

        public PricePlanComparatorController(IPricePlanService pricePlanService, IAccountService accountService)
        {
            this._pricePlanService = pricePlanService;
            this._accountService = accountService;
        }

        [HttpGet("compare-all/{smartMeterId}")]
        public ObjectResult CalculatedCostForEachPricePlan(string smartMeterId)
        {
            var costPerPricePlan = this._pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);

            return
                costPerPricePlan.Any() ? 
                new ObjectResult(JObject.FromObject(costPerPricePlan)) : 
                new NotFoundObjectResult($"Smart Meter ID ({smartMeterId}) not found");
        }

        [HttpGet("recommend/{smartMeterId}")]
        public ObjectResult RecommendCheapestPricePlans(string smartMeterId, int? limit = null) 
        {
            var consumptionForPricePlans = this._pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);

            if (!consumptionForPricePlans.Any())            
                return new NotFoundObjectResult($"Smart Meter ID ({smartMeterId}) not found");
            
            var recommendations = consumptionForPricePlans.OrderBy(pricePlanComparison => pricePlanComparison.Value);

            if (limit.HasValue && limit.Value < recommendations.Count())
            {
                return new ObjectResult(recommendations.Take(limit.Value));
            }

            return new ObjectResult(recommendations);
        }
    }
}
