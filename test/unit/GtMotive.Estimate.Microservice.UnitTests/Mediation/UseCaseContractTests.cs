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

using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Mediation;

public sealed class UseCaseContractTests
{
    [Fact]
    public void InputsImplementProvidedContract()
    {
        Assert.IsAssignableFrom<IUseCaseInput>(
            new CreateVehicleCommand("1234ABC", "Toyota", "Corolla", new(2024, 1, 1)));
        Assert.IsAssignableFrom<IUseCaseInput>(new ListVehiclesQuery());
        Assert.True(typeof(IUseCaseInput).IsAssignableFrom(typeof(RentVehicleCommand)));
        Assert.True(typeof(IUseCaseInput).IsAssignableFrom(typeof(ReturnVehicleCommand)));
    }

    [Fact]
    public void OutputsImplementProvidedContract()
    {
        Assert.True(typeof(IUseCaseOutput).IsAssignableFrom(typeof(CreateVehicleResult)));
        Assert.True(typeof(IUseCaseOutput).IsAssignableFrom(typeof(ListVehiclesResult)));
        Assert.True(typeof(IUseCaseOutput).IsAssignableFrom(typeof(RentVehicleResult)));
        Assert.True(typeof(IUseCaseOutput).IsAssignableFrom(typeof(ReturnVehicleResult)));
    }

    [Fact]
    public void UseCasesImplementProvidedContract()
    {
        Assert.True(typeof(IUseCase<CreateVehicleCommand>).IsAssignableFrom(typeof(CreateVehicleUseCase)));
        Assert.True(typeof(IUseCase<ListVehiclesQuery>).IsAssignableFrom(typeof(ListVehiclesUseCase)));
        Assert.True(typeof(IUseCase<RentVehicleCommand>).IsAssignableFrom(typeof(RentVehicleUseCase)));
        Assert.True(typeof(IUseCase<ReturnVehicleCommand>).IsAssignableFrom(typeof(ReturnVehicleUseCase)));
    }
}
