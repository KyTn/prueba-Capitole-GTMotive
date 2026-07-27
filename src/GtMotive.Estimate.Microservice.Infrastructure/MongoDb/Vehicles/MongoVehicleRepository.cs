using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Vehicles;

public sealed class MongoVehicleRepository : IVehicleRepository
{
    private readonly IMongoCollection<VehicleDocument> _vehicles;

    public MongoVehicleRepository(MongoService mongoService, IOptions<MongoDbSettings> options)
    {
        var database = mongoService.MongoClient.GetDatabase(options.Value.MongoDbDatabaseName);
        _vehicles = database.GetCollection<VehicleDocument>(options.Value.VehiclesCollectionName);
        var index = new CreateIndexModel<VehicleDocument>(
            Builders<VehicleDocument>.IndexKeys.Ascending(vehicle => vehicle.RegistrationNumber),
            new CreateIndexOptions { Unique = true, Name = "ux_vehicles_registration_number" });
        _vehicles.Indexes.CreateOne(index);
    }

    public async Task<bool> ExistsByRegistrationNumberAsync(
        RegistrationNumber registrationNumber,
        CancellationToken cancellationToken)
    {
        return await _vehicles
            .Find(vehicle => vehicle.RegistrationNumber == registrationNumber.Value)
            .AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        try
        {
            await _vehicles.InsertOneAsync(
                VehicleMapper.ToDocument(vehicle),
                cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception)
            when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new VehicleAlreadyExistsException();
        }
    }

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken)
    {
        var documents = await _vehicles
            .Find(Builders<VehicleDocument>.Filter.Empty)
            .ToListAsync(cancellationToken);

        return documents.Select(VehicleMapper.ToDomain).ToArray();
    }
}
