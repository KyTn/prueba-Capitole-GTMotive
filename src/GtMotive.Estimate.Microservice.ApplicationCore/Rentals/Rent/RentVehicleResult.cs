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

using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;

public enum RentVehicleResultType
{
    Created,
    InvalidInput,
    PersonNotFound,
    VehicleNotFound,
    PersonAlreadyHasActiveRental,
    VehicleNotAvailable,
}

public sealed record RentVehicleResult(
    RentVehicleResultType Type,
    RentalDto Rental,
    string Code,
    string Detail) : IUseCaseOutput
{
    public static RentVehicleResult Created(RentalDto rental) =>
        new(RentVehicleResultType.Created, rental, null, null);

    public static RentVehicleResult Failure(RentVehicleResultType type, string code, string detail) =>
        new(type, null, code, detail);
}
