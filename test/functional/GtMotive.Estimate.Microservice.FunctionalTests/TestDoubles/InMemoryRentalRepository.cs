using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class InMemoryRentalRepository : IRentalRepository
{
    private readonly object _sync = new();
    private readonly List<Rental> _rentals = new();

    public IReadOnlyList<Rental> Rentals
    {
        get
        {
            lock (_sync)
            {
                return _rentals.ToArray();
            }
        }
    }

    public Task<AddActiveRentalResult> TryAddActiveAsync(
        Rental rental,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (_rentals.Any(item =>
                    item.Status == RentalStatus.Active &&
                    item.PersonId == rental.PersonId))
            {
                return Task.FromResult(AddActiveRentalResult.PersonConflict);
            }

            if (_rentals.Any(item =>
                    item.Status == RentalStatus.Active &&
                    item.VehicleId == rental.VehicleId))
            {
                return Task.FromResult(AddActiveRentalResult.VehicleConflict);
            }

            _rentals.Add(rental);
            return Task.FromResult(AddActiveRentalResult.Created);
        }
    }

    public Task<Rental> GetActiveByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var rental = _rentals.SingleOrDefault(
                item => item.Status == RentalStatus.Active && item.VehicleId == vehicleId);
            return Task.FromResult(Clone(rental));
        }
    }

    public Task<CloseActiveRentalResult> TryCloseActiveAsync(
        Rental rental,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var index = _rentals.FindIndex(item =>
                item.Id == rental.Id &&
                item.PersonId == rental.PersonId &&
                item.VehicleId == rental.VehicleId &&
                item.Status == RentalStatus.Active);
            if (index < 0)
            {
                return Task.FromResult(CloseActiveRentalResult.Conflict);
            }

            _rentals[index] = Clone(rental);
            return Task.FromResult(CloseActiveRentalResult.Closed);
        }
    }

    private static Rental Clone(Rental rental) =>
        rental is null
            ? null
            : Rental.Rehydrate(
                rental.Id,
                rental.PersonId,
                rental.VehicleId,
                rental.StartedAt,
                rental.Status,
                rental.EndedAt);
}
