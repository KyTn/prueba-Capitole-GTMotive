using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;

public sealed record ListVehiclesQuery : IUseCaseInput, IRequest<ListVehiclesResult>;
