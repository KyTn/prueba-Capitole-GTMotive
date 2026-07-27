using System;

namespace GtMotive.Estimate.Microservice.Domain.Rentals;

public sealed class Rental
{
    private Rental(Guid id, PersonId personId, Guid vehicleId, DateTimeOffset startedAt, RentalStatus status)
    {
        Id = id;
        PersonId = personId;
        VehicleId = vehicleId;
        StartedAt = startedAt;
        Status = status;
    }

    public Guid Id { get; }

    public PersonId PersonId { get; }

    public Guid VehicleId { get; }

    public DateTimeOffset StartedAt { get; }

    public RentalStatus Status { get; }

    public DateTimeOffset? EndedAt { get; }

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

        return new Rental(id, personId, vehicleId, startedAt, RentalStatus.Active);
    }

    public static Rental Rehydrate(
        Guid id,
        PersonId personId,
        Guid vehicleId,
        DateTimeOffset startedAt,
        RentalStatus status) =>
        new(id, personId, vehicleId, startedAt, status);
}
