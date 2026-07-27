using System;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Rentals;

internal static class RentalMapper
{
    public static RentalDocument ToDocument(Rental rental) =>
        new()
        {
            Id = rental.Id,
            PersonId = rental.PersonId.Value,
            VehicleId = rental.VehicleId,
            StartedAt = rental.StartedAt.UtcDateTime,
            Status = rental.Status.ToString(),
            EndedAt = rental.EndedAt?.UtcDateTime,
        };

    public static Rental ToDomain(RentalDocument document) =>
        Rental.Rehydrate(
            document.Id,
            new PersonId(document.PersonId),
            document.VehicleId,
            new DateTimeOffset(DateTime.SpecifyKind(document.StartedAt, DateTimeKind.Utc)),
            Enum.Parse<RentalStatus>(document.Status),
            document.EndedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(document.EndedAt.Value, DateTimeKind.Utc))
                : null);
}
