using System;
using System.Diagnostics.CodeAnalysis;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Infrastructure.Interfaces;
using GtMotive.Estimate.Microservice.Infrastructure.Logging;
using GtMotive.Estimate.Microservice.Infrastructure.Telemetry;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.Time;
using GtMotive.Estimate.Microservice.ApplicationCore.People;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Rentals;
using GtMotive.Estimate.Microservice.Infrastructure.People;
using Microsoft.Extensions.DependencyInjection;

[assembly: CLSCompliant(false)]

namespace GtMotive.Estimate.Microservice.Infrastructure
{
    public static class InfrastructureConfiguration
    {
        [ExcludeFromCodeCoverage]
        public static IInfrastructureBuilder AddBaseInfrastructure(
            this IServiceCollection services,
            bool isDevelopment)
        {
            services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<MongoService>();
            services.AddScoped<IVehicleRepository, MongoVehicleRepository>();
            services.AddScoped<IRentalRepository, MongoRentalRepository>();
            services.AddSingleton<IPersonRegistry, HttpPersonRegistry>();

            if (!isDevelopment)
            {
                services.AddScoped<ITelemetry, AppTelemetry>();
            }
            else
            {
                services.AddScoped<ITelemetry, NoOpTelemetry>();
            }

            return new InfrastructureBuilder(services);
        }

        private sealed class InfrastructureBuilder(IServiceCollection services) : IInfrastructureBuilder
        {
            public IServiceCollection Services { get; } = services;
        }
    }
}
