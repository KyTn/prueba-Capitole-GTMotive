using System;
using System.Linq;
using GtMotive.Estimate.Microservice.Api.Rentals.Rent;
using GtMotive.Estimate.Microservice.Api.Rentals.Return;
using GtMotive.Estimate.Microservice.Api.Vehicles.Create;
using GtMotive.Estimate.Microservice.Api.Vehicles.List;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using MediatR;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Mediation;

public sealed class ControllerMediationTests
{
    [Theory]
    [InlineData(typeof(VehiclesController))]
    [InlineData(typeof(ListVehiclesController))]
    [InlineData(typeof(RentalsController))]
    [InlineData(typeof(RentalReturnsController))]
    public void ControllerDependsOnlyOnMediator(Type controllerType)
    {
        var constructor = Assert.Single(controllerType.GetConstructors());
        var parameter = Assert.Single(constructor.GetParameters());

        Assert.Equal(typeof(IMediator), parameter.ParameterType);
        Assert.DoesNotContain(
            constructor.GetParameters(),
            item => item.ParameterType.Name.EndsWith("UseCase", StringComparison.Ordinal));
    }

    [Fact]
    public void ApiRequestsAreTransportOnlyAndApplicationInputsAreMediatRMessages()
    {
        var requestType = typeof(IRequest<>);
        var apiRequests = new[]
        {
            typeof(CreateVehicleRequest),
            typeof(RentVehicleRequest),
            typeof(ReturnVehicleRequest),
        };
        var applicationMessages = new[]
        {
            typeof(CreateVehicleCommand),
            typeof(ListVehiclesQuery),
            typeof(RentVehicleCommand),
            typeof(ReturnVehicleCommand),
        };

        Assert.All(
            apiRequests,
            request => Assert.DoesNotContain(
                request.GetInterfaces(),
                item => item.IsGenericType && item.GetGenericTypeDefinition() == requestType));
        Assert.All(
            applicationMessages,
            message => Assert.Contains(
                message.GetInterfaces(),
                item => item.IsGenericType && item.GetGenericTypeDefinition() == requestType));
    }
}
