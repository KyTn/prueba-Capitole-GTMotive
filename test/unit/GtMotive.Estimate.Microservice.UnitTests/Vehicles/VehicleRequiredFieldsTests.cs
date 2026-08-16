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
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Vehicles;

public sealed class VehicleRequiredFieldsTests
{
    public static TheoryData<Guid, string, string, string> InvalidFields =>
        new()
        {
            { Guid.Empty, "1234ABC", "Toyota", "Corolla" },
            { Guid.NewGuid(), " ", "Toyota", "Corolla" },
            { Guid.NewGuid(), "1234ABC", " ", "Corolla" },
            { Guid.NewGuid(), "1234ABC", "Toyota", " " },
        };

    [Theory]
    [MemberData(nameof(InvalidFields))]
    public void Create_RejectsMissingFields(Guid id, string registration, string brand, string model)
    {
        var exception = Assert.Throws<VehicleValidationException>(() => Vehicle.Create(
            id, registration, brand, model, new DateOnly(2025, 1, 1), new DateOnly(2026, 1, 1)));

        Assert.Equal(VehicleErrorCodes.InvalidVehicle, exception.Code);
    }
}
