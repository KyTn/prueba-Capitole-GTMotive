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

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;

public enum ReturnVehicleResultType
{
    Returned,
    InvalidInput,
    PersonNotFound,
    VehicleNotFound,
    VehicleNotRented,
    RentalOwnershipConflict,
    RentalAlreadyReturned,
}

public sealed record ReturnVehicleResult(
    ReturnVehicleResultType Type,
    RentalDto Rental,
    string Code,
    string Detail) : IUseCaseOutput
{
    public static ReturnVehicleResult Returned(RentalDto rental) =>
        new(ReturnVehicleResultType.Returned, rental, null, null);

    public static ReturnVehicleResult Failure(ReturnVehicleResultType type, string code, string detail) =>
        new(type, null, code, detail);
}
