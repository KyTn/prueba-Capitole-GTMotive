using System;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;

public sealed record RentVehicleCommand(Guid PersonId, Guid VehicleId)
    : IUseCaseInput, IRequest<RentVehicleResult>;
