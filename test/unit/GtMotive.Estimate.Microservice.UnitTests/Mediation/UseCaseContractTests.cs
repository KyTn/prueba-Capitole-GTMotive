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
