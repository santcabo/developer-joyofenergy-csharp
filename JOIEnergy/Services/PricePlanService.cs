using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        public interface Debug { void Log(string s); };

        private readonly List<PricePlan> _pricePlans;
        private IMeterReadingService _meterReadingService;

        public PricePlanService(List<PricePlan> pricePlan, IMeterReadingService meterReadingService)
        {
            this._pricePlans = pricePlan;
            this._meterReadingService = meterReadingService;
        }

        private decimal calculateAverageReading(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            var newSummedReadings = electricityReadings.Select(readings =>
            {
                var multiplier = 1m;
                if (pricePlan.PeakTimeMultiplier.TryGetValue(readings.Time.DayOfWeek, out decimal ret))
                {
                    multiplier = ret;
                }
                return readings.Reading * multiplier;
            }).Sum();
           
            return newSummedReadings / electricityReadings.Count();
        }

        //private decimal calculatedTotalReading(List<ElectricityReading> electricityReadings)
        //{
        //    electricityReadings.
        //}

        /// <summary>
        /// Return number of hours between first and last reading.
        /// </summary>
        /// <param name="electricityReadings"></param>
        /// <returns>Number of hours</returns>
        private decimal calculateTimeElapsed(List<ElectricityReading> electricityReadings)
        {
            var first = electricityReadings.Min(reading => reading.Time);
            var last = electricityReadings.Max(reading => reading.Time);

            return (decimal)(last - first).TotalHours;
        }

        /// <summary>
        /// Calculate cost of a electricityReading list by a given Priceplan
        /// </summary>
        /// <param name="electricityReadings"></param>
        /// <param name="pricePlan"></param>
        /// <returns></returns>
        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            var average = calculateAverageReading(electricityReadings, pricePlan);
            var timeElapsed = calculateTimeElapsed(electricityReadings);
            var averagedCost = average/timeElapsed;
            return averagedCost * pricePlan.UnitRate;
        }

        public Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(string smartMeterId)
        {
            var electricityReadings = this._meterReadingService.GetReadings(smartMeterId);

            if (!electricityReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }

            return this._pricePlans.ToDictionary(plan => plan.EnergySupplier.ToString(), plan => calculateCost(electricityReadings, plan));
        }
    }
}
