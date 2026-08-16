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

namespace GtMotive.Estimate.Microservice.Domain.Vehicles;

public sealed class Vehicle
{
    private Vehicle(
        Guid id,
        RegistrationNumber registrationNumber,
        string brand,
        string model,
        DateOnly manufactureDate)
    {
        Id = id;
        RegistrationNumber = registrationNumber;
        Brand = brand;
        Model = model;
        ManufactureDate = manufactureDate;
    }

    public Guid Id { get; }

    public RegistrationNumber RegistrationNumber { get; }

    public string Brand { get; }

    public string Model { get; }

    public DateOnly ManufactureDate { get; }

    public static Vehicle Create(
        Guid id,
        string registrationNumber,
        string brand,
        string model,
        DateOnly manufactureDate,
        DateOnly registrationDate)
    {
        if (id == Guid.Empty)
        {
            throw new VehicleValidationException(VehicleErrorCodes.InvalidVehicle, "Vehicle id is required.");
        }

        var normalizedBrand = RequiredText(brand, "Brand");
        var normalizedModel = RequiredText(model, "Model");

        if (manufactureDate > registrationDate)
        {
            throw new VehicleValidationException(
                VehicleErrorCodes.FutureManufactureDate,
                "Manufacture date cannot be in the future.");
        }

        if (manufactureDate < registrationDate.AddYears(-5))
        {
            throw new VehicleValidationException(
                VehicleErrorCodes.VehicleTooOld,
                "Vehicle is more than five years old.");
        }

        return new Vehicle(
            id,
            new RegistrationNumber(registrationNumber),
            normalizedBrand,
            normalizedModel,
            manufactureDate);
    }

    public static Vehicle Rehydrate(
        Guid id,
        string registrationNumber,
        string brand,
        string model,
        DateOnly manufactureDate)
    {
        if (id == Guid.Empty)
        {
            throw new VehicleValidationException(VehicleErrorCodes.InvalidVehicle, "Vehicle id is required.");
        }

        return new Vehicle(
            id,
            new RegistrationNumber(registrationNumber),
            RequiredText(brand, "Brand"),
            RequiredText(model, "Model"),
            manufactureDate);
    }

    private static string RequiredText(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new VehicleValidationException(VehicleErrorCodes.InvalidVehicle, $"{field} is required.");
        }

        return value.Trim();
    }
}
