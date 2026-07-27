using System;

namespace GtMotive.Estimate.Microservice.Domain.Rentals;

public readonly record struct PersonId
{
    public PersonId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new RentalValidationException("invalid_person_id", "Person id is required.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
