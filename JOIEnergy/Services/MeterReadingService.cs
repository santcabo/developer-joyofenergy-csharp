using System;
using System.Collections.Generic;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        public Dictionary<string, List<ElectricityReading>> MeterAssociatedReadings { get; set; }

        public MeterReadingService(Dictionary<string, List<ElectricityReading>> meterAssociatedReadings)
        {
            this.MeterAssociatedReadings = meterAssociatedReadings;
        }

        public List<ElectricityReading> GetReadings(string smartMeterId) 
        {
            if (this.MeterAssociatedReadings.TryGetValue(smartMeterId, out List<ElectricityReading> ret)) return ret;

            return new List<ElectricityReading>();
        }

        public void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings) 
        {
            if (!this.MeterAssociatedReadings.ContainsKey(smartMeterId)) 
            {
                this.MeterAssociatedReadings.Add(smartMeterId, electricityReadings);
            }
            else
            {
                this.MeterAssociatedReadings[smartMeterId].AddRange(electricityReadings);
            }
        }
    }
}
