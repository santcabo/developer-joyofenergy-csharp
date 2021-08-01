using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Generator;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JOIEnergy
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container (configure the app's services).
        public void ConfigureServices(IServiceCollection services)
        {
            var pricePlans = new Dictionary<Supplier, PricePlan>()
                                            {
                                                { Supplier.DrEvilsDarkEnergy, new PricePlan(supplier: Supplier.DrEvilsDarkEnergy, unitRate:10m)},
                                                { Supplier.TheGreenEco, new PricePlan(supplier: Supplier.TheGreenEco, unitRate:2m)},
                                                { Supplier.PowerForEveryone, new PricePlan(supplier: Supplier.PowerForEveryone, unitRate:1m)},
                                            };

            services.AddMvc();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IMeterReadingService, MeterReadingService>();
            services.AddTransient<IPricePlanService, PricePlanService>();
            services.AddSingleton((IServiceProvider arg) => this.GenerateMeterElectricityReadings());
            services.AddSingleton((IServiceProvider arg) => pricePlans);
            services.AddSingleton((IServiceProvider arg) => this.SmartMeterToPricePlanAccounts);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        public Dictionary<string, Supplier> SmartMeterToPricePlanAccounts => new Dictionary<string, Supplier>
                                            {
                                                { "smart-meter-0", Supplier.DrEvilsDarkEnergy },
                                                { "smart-meter-1", Supplier.TheGreenEco },
                                                { "smart-meter-2", Supplier.DrEvilsDarkEnergy },
                                                { "smart-meter-3", Supplier.PowerForEveryone },
                                                { "smart-meter-4", Supplier.TheGreenEco }
                                            };

        /// <summary>
        /// Generated ramdon readings for each "Smart Meter Id" already included in <id>SmartMeterToPricePlanAccounts</id>
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings() 
        {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            this.SmartMeterToPricePlanAccounts.Keys.ToList().ForEach(smartMeterId => readings.Add(smartMeterId, ElectricityRamdonReadingGenerator.Generate(20)));

            return readings;
        }
    }
}
