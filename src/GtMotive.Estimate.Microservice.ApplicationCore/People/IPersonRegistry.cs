using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.People;

public interface IPersonRegistry
{
    Task<bool> ExistsAsync(PersonId personId, CancellationToken cancellationToken);
}
