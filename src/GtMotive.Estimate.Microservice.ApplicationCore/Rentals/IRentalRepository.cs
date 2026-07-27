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

public interface IRentalRepository
{
    Task<AddActiveRentalResult> TryAddActiveAsync(Rental rental, CancellationToken cancellationToken);
}
