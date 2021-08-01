using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Enums;

namespace JOIEnergy.Domain
{
    public class PricePlan
    {
        public Supplier EnergySupplier { get; set; }
        public decimal UnitRate { get; set; }
        public IDictionary<DayOfWeek, decimal> PeakTimeMultiplier { get; set; }

        public PricePlan(Supplier supplier, decimal unitRate, IDictionary<DayOfWeek, decimal> peakTimeMultiplier = null)
        {
            this.EnergySupplier = supplier;
            this.UnitRate = unitRate;
            this.PeakTimeMultiplier = peakTimeMultiplier ?? NoMultipliers();
        }

        public decimal GetPrice(DateTime datetime) => this.PeakTimeMultiplier.TryGetValue(datetime.DayOfWeek, out decimal multiplier)
                                                      ? multiplier * this.UnitRate
                                                      : this.UnitRate;

        public static IDictionary<DayOfWeek, decimal> NoMultipliers() => new Dictionary<DayOfWeek, decimal>();
    }
}
