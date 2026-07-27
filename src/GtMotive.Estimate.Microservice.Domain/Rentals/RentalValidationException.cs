using GtMotive.Estimate.Microservice.Domain;

namespace GtMotive.Estimate.Microservice.Domain.Rentals;

public sealed class RentalValidationException(string code, string message) : DomainException(message)
{
    public string Code { get; } = code;
}
