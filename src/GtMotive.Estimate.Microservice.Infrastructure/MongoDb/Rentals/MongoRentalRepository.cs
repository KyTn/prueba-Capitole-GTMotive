using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Rentals;

public sealed class MongoRentalRepository : IRentalRepository
{
    private const string PersonIndexName = "ux_rentals_active_person";
    private const string VehicleIndexName = "ux_rentals_active_vehicle";
    private readonly IMongoCollection<RentalDocument> _rentals;

    public MongoRentalRepository(MongoService mongoService, IOptions<MongoDbSettings> options)
    {
        var database = mongoService.MongoClient.GetDatabase(options.Value.MongoDbDatabaseName);
        _rentals = database.GetCollection<RentalDocument>(options.Value.RentalsCollectionName);
        var activeFilter = new BsonDocument("Status", RentalStatus.Active.ToString());
        var indexes = new[]
        {
            new CreateIndexModel<RentalDocument>(
                Builders<RentalDocument>.IndexKeys.Ascending(rental => rental.PersonId),
                new CreateIndexOptions<RentalDocument>
                {
                    Unique = true,
                    Name = PersonIndexName,
                    PartialFilterExpression = activeFilter,
                }),
            new CreateIndexModel<RentalDocument>(
                Builders<RentalDocument>.IndexKeys.Ascending(rental => rental.VehicleId),
                new CreateIndexOptions<RentalDocument>
                {
                    Unique = true,
                    Name = VehicleIndexName,
                    PartialFilterExpression = activeFilter,
                }),
        };
        _rentals.Indexes.CreateMany(indexes);
    }

    public async Task<AddActiveRentalResult> TryAddActiveAsync(
        Rental rental,
        CancellationToken cancellationToken)
    {
        try
        {
            await _rentals.InsertOneAsync(RentalMapper.ToDocument(rental), cancellationToken: cancellationToken);
            return AddActiveRentalResult.Created;
        }
        catch (MongoWriteException exception)
            when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return exception.Message.Contains(PersonIndexName, StringComparison.Ordinal)
                ? AddActiveRentalResult.PersonConflict
                : AddActiveRentalResult.VehicleConflict;
        }
    }
}
