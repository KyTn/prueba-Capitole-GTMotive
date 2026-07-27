using System;
using System.Collections.Generic;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;

public sealed class ListVehiclesResult : IUseCaseOutput
{
    public ListVehiclesResult(IReadOnlyList<VehicleDto> vehicles)
    {
        Vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
    }

    public IReadOnlyList<VehicleDto> Vehicles { get; }
}
