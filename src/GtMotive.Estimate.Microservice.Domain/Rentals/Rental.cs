using System;

namespace GtMotive.Estimate.Microservice.Domain.Rentals;

public sealed class Rental
{
    private Rental(
        Guid id,
        PersonId personId,
        Guid vehicleId,
        DateTimeOffset startedAt,
        RentalStatus status,
        DateTimeOffset? endedAt)
    {
        Id = id;
        PersonId = personId;
        VehicleId = vehicleId;
        StartedAt = startedAt;
        Status = status;
        EndedAt = endedAt;
    }

    public Guid Id { get; }

    public PersonId PersonId { get; }

    public Guid VehicleId { get; }

    public DateTimeOffset StartedAt { get; }

    public RentalStatus Status { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public static Rental Create(Guid id, PersonId personId, Guid vehicleId, DateTimeOffset startedAt)
    {
        if (id == Guid.Empty)
        {
            throw new RentalValidationException("invalid_rental_id", "Rental id is required.");
        }

        if (vehicleId == Guid.Empty)
        {
            throw new RentalValidationException("invalid_vehicle_id", "Vehicle id is required.");
        }

        return new Rental(id, personId, vehicleId, startedAt, RentalStatus.Active, null);
    }

    public void Return(DateTimeOffset endedAt)
    {
        if (Status != RentalStatus.Active)
        {
            throw new RentalValidationException("rental_not_active", "Only an active rental can be returned.");
        }

        if (endedAt < StartedAt)
        {
            throw new RentalValidationException(
                "invalid_rental_end",
                "The rental end cannot be earlier than its start.");
        }

        Status = RentalStatus.Closed;
        EndedAt = endedAt;
    }

    public static Rental Rehydrate(
        Guid id,
        PersonId personId,
        Guid vehicleId,
        DateTimeOffset startedAt,
        RentalStatus status,
        DateTimeOffset? endedAt = null)
    {
        if (status == RentalStatus.Active && endedAt.HasValue)
        {
            throw new RentalValidationException("invalid_active_rental_end", "An active rental cannot have an end.");
        }

        if (status == RentalStatus.Closed && !endedAt.HasValue)
        {
            throw new RentalValidationException("missing_rental_end", "A closed rental requires an end.");
        }

        if (endedAt < startedAt)
        {
            throw new RentalValidationException(
                "invalid_rental_end",
                "The rental end cannot be earlier than its start.");
        }

        return new Rental(id, personId, vehicleId, startedAt, status, endedAt);
    }
}
