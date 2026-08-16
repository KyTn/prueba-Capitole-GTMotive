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

namespace GtMotive.Estimate.Microservice.Domain.Vehicles;

public static class VehicleErrorCodes
{
    public const string InvalidVehicle = "invalid_vehicle";
    public const string FutureManufactureDate = "future_manufacture_date";
    public const string VehicleTooOld = "vehicle_too_old";
}

public sealed class VehicleValidationException : DomainException
{
    public VehicleValidationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
