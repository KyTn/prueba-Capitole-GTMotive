using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals;

public enum AddActiveRentalResult
{
    Created,
    PersonConflict,
    VehicleConflict,
}

public enum CloseActiveRentalResult
{
    Closed,
    Conflict,
}

public interface IRentalRepository
{
    Task<AddActiveRentalResult> TryAddActiveAsync(Rental rental, CancellationToken cancellationToken);

    Task<Rental> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);

    Task<CloseActiveRentalResult> TryCloseActiveAsync(
        Rental rental,
        CancellationToken cancellationToken);
}
