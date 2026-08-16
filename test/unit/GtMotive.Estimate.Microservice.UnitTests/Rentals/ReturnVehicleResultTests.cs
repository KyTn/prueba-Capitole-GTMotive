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
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Rentals;

public sealed class ReturnVehicleResultTests
{
    [Fact]
    public void Returned_PreservesRental()
    {
        var dto = new RentalDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(-1),
            "closed",
            DateTimeOffset.UtcNow);

        var result = ReturnVehicleResult.Returned(dto);

        Assert.Equal(ReturnVehicleResultType.Returned, result.Type);
        Assert.Same(dto, result.Rental);
    }

    [Fact]
    public void Failure_PreservesStableCode()
    {
        var result = ReturnVehicleResult.Failure(
            ReturnVehicleResultType.VehicleNotRented,
            "vehicle_not_rented",
            "detail");

        Assert.Equal("vehicle_not_rented", result.Code);
        Assert.Null(result.Rental);
    }
}
