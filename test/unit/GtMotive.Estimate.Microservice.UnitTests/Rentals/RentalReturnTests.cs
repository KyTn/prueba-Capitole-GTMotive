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

public sealed class RentalReturnTests
{
    private static readonly DateTimeOffset StartedAt =
        new(2026, 7, 27, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Return_ClosesActiveRentalAndRecordsEnd()
    {
        var rental = CreateRental();
        var endedAt = StartedAt.AddHours(2);

        rental.Return(endedAt);

        Assert.Equal(RentalStatus.Closed, rental.Status);
        Assert.Equal(endedAt, rental.EndedAt);
    }

    [Fact]
    public void Return_RejectsSecondReturnAndPreservesFirstEnd()
    {
        var rental = CreateRental();
        var firstEnd = StartedAt.AddHours(1);
        rental.Return(firstEnd);

        var exception = Assert.Throws<RentalValidationException>(
            () => rental.Return(StartedAt.AddHours(2)));

        Assert.Equal("rental_not_active", exception.Code);
        Assert.Equal(firstEnd, rental.EndedAt);
    }

    [Fact]
    public void Return_RejectsEndBeforeStart()
    {
        var rental = CreateRental();

        var exception = Assert.Throws<RentalValidationException>(
            () => rental.Return(StartedAt.AddTicks(-1)));

        Assert.Equal("invalid_rental_end", exception.Code);
        Assert.Equal(RentalStatus.Active, rental.Status);
        Assert.Null(rental.EndedAt);
    }

    [Fact]
    public void Rehydrate_RestoresClosedRental()
    {
        var endedAt = StartedAt.AddHours(1);

        var rental = Rental.Rehydrate(
            Guid.NewGuid(),
            new PersonId(Guid.NewGuid()),
            Guid.NewGuid(),
            StartedAt,
            RentalStatus.Closed,
            endedAt);

        Assert.Equal(RentalStatus.Closed, rental.Status);
        Assert.Equal(endedAt, rental.EndedAt);
    }

    private static Rental CreateRental() =>
        Rental.Create(Guid.NewGuid(), new PersonId(Guid.NewGuid()), Guid.NewGuid(), StartedAt);
}
