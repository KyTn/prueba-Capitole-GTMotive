using System;
using System.Collections.Generic;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;

public sealed class ListVehiclesResult
{
    public ListVehiclesResult(IReadOnlyList<VehicleDto> vehicles)
    {
        Vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
    }

    public IReadOnlyList<VehicleDto> Vehicles { get; }
}
