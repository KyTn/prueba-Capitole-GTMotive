using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;

public interface IVehicleRepository
{
    Task<bool> ExistsByRegistrationNumberAsync(
        RegistrationNumber registrationNumber,
        CancellationToken cancellationToken);

    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);
}

public sealed class VehicleAlreadyExistsException : DomainException
{
    public VehicleAlreadyExistsException()
        : base("A vehicle with this registration number already exists.")
    {
    }
}
