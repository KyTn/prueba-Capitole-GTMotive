using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

[assembly: CLSCompliant(false)]

namespace GtMotive.Estimate.Microservice.ApplicationCore
{
    /// <summary>
    /// Adds Use Cases classes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ApplicationConfiguration
    {
        /// <summary>
        /// Adds Use Cases to the ServiceCollection.
        /// </summary>
        /// <param name="services">Service Collection.</param>
        /// <returns>The modified instance.</returns>
        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            services.AddScoped<CreateVehicleUseCase>();
            services.AddScoped<ListVehiclesUseCase>();
            services.AddScoped<RentVehicleUseCase>();
            services.AddScoped<ReturnVehicleUseCase>();
            services.AddScoped<IUseCase<CreateVehicleCommand>>(provider => provider.GetRequiredService<CreateVehicleUseCase>());
            services.AddScoped<IUseCase<ListVehiclesQuery>>(provider => provider.GetRequiredService<ListVehiclesUseCase>());
            services.AddScoped<IUseCase<RentVehicleCommand>>(provider => provider.GetRequiredService<RentVehicleUseCase>());
            services.AddScoped<IUseCase<ReturnVehicleCommand>>(provider => provider.GetRequiredService<ReturnVehicleUseCase>());
            return services;
        }
    }
}
