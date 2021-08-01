using System;
using System.Collections.Generic;
using JOIEnergy.Domain;

namespace JOIEnergy.Generator
{
    /// <summary>
    ///  
    /// </summary>
    public static class ElectricityRamdonReadingGenerator
    {
        public static List<ElectricityReading> Generate(int number)
        {
            var readings = new List<ElectricityReading>();
            var random = new Random();
            var timeNow = DateTime.Now;

            for (var i = number; i > 0; i--)
            {
                var reading = (decimal)random.NextDouble();
                var electricityReading = new ElectricityReading
                {
                    Reading = reading,
                    Time = timeNow.AddSeconds(-i * 10)
                };
                readings.Add(electricityReading);
            }

            return readings;
        }
    }
}
