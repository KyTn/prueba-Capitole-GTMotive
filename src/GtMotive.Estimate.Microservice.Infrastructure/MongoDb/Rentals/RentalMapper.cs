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

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Rentals;

internal static class RentalMapper
{
    public static RentalDocument ToDocument(Rental rental) =>
        new()
        {
            Id = rental.Id,
            PersonId = rental.PersonId.Value,
            VehicleId = rental.VehicleId,
            StartedAt = rental.StartedAt.UtcDateTime,
            Status = rental.Status.ToString(),
            EndedAt = rental.EndedAt?.UtcDateTime,
        };

    public static Rental ToDomain(RentalDocument document) =>
        Rental.Rehydrate(
            document.Id,
            new PersonId(document.PersonId),
            document.VehicleId,
            new DateTimeOffset(DateTime.SpecifyKind(document.StartedAt, DateTimeKind.Utc)),
            Enum.Parse<RentalStatus>(document.Status),
            document.EndedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(document.EndedAt.Value, DateTimeKind.Utc))
                : null);
}
