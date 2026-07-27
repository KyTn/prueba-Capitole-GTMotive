using System;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;

public sealed record CreateVehicleCommand(
    string RegistrationNumber,
    string Brand,
    string Model,
    DateOnly ManufactureDate) : IUseCaseInput, IRequest<CreateVehicleResult>;
