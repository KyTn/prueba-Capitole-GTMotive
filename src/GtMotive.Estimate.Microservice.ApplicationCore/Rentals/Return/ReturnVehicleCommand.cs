using System;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;

public sealed record ReturnVehicleCommand(Guid PersonId, Guid VehicleId)
    : IUseCaseInput, IRequest<ReturnVehicleResult>;
