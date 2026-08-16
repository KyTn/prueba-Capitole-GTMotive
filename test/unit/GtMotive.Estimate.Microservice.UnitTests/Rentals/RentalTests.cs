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
using GtMotive.Estimate.Microservice.Domain.Rentals;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Rentals;

public sealed class RentalTests
{
    [Fact]
    public void Create_StartsActiveRental()
    {
        var id = Guid.NewGuid();
        var personId = new PersonId(Guid.NewGuid());
        var vehicleId = Guid.NewGuid();
        var startedAt = new DateTimeOffset(2026, 7, 27, 10, 0, 0, TimeSpan.Zero);

        var rental = Rental.Create(id, personId, vehicleId, startedAt);

        Assert.Equal(id, rental.Id);
        Assert.Equal(personId, rental.PersonId);
        Assert.Equal(vehicleId, rental.VehicleId);
        Assert.Equal(startedAt, rental.StartedAt);
        Assert.Equal(RentalStatus.Active, rental.Status);
        Assert.Null(rental.EndedAt);
    }

    [Fact]
    public void Create_RejectsEmptyVehicleId()
    {
        var exception = Assert.Throws<RentalValidationException>(
            () => Rental.Create(Guid.NewGuid(), new PersonId(Guid.NewGuid()), Guid.Empty, DateTimeOffset.UtcNow));

        Assert.Equal("invalid_vehicle_id", exception.Code);
    }
}
