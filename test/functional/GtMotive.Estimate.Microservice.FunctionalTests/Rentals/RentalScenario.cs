/*
 * Aviso de propiedad intelectual
 *
 * Este repositorio se ha creado exclusivamente como prueba tÃ©cnica para Capitole.
 * Salvo los componentes de terceros y los derechos que pudieran haberse cedido
 * expresamente por contrato, el cÃ³digo y la documentaciÃ³n originales contenidos en
 * Ã©l son propiedad de su autor. No se autoriza su copia, reproducciÃ³n, modificaciÃ³n,
 * distribuciÃ³n, publicaciÃ³n ni explotaciÃ³n, total o parcial, sin consentimiento
 * previo y por escrito del titular de los derechos. El titular se reserva el
 * ejercicio de las acciones legales que correspondan frente a cualquier uso no
 * autorizado.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

internal sealed class RentalScenario
{
    public RentalScenario()
    {
        Clock = new FixedClock(new DateOnly(2026, 7, 27));
        UseCase = new RentVehicleUseCase(
            People,
            Vehicles,
            Rentals,
            Clock,
            new NullAppLogger<RentVehicleUseCase>());
        ReturnUseCase = new ReturnVehicleUseCase(
            People,
            Vehicles,
            Rentals,
            Clock,
            new NullAppLogger<ReturnVehicleUseCase>());
    }

    public InMemoryPersonRegistry People { get; } = new();

    public InMemoryVehicleRepository Vehicles { get; } = new();

    public InMemoryRentalRepository Rentals { get; } = new();

    public RentVehicleUseCase UseCase { get; }

    public ReturnVehicleUseCase ReturnUseCase { get; }

    public FixedClock Clock { get; }

    public PersonId AddPerson()
    {
        var id = new PersonId(Guid.NewGuid());
        People.Add(id);
        return id;
    }

    public async Task<Vehicle> AddVehicleAsync()
    {
        var vehicle = Vehicle.Rehydrate(
            Guid.NewGuid(),
            $"{Random.Shared.Next(1000, 9999)}ABC",
            "Toyota",
            "Corolla",
            new DateOnly(2024, 1, 1));
        await Vehicles.AddAsync(vehicle, CancellationToken.None);
        return vehicle;
    }
}
