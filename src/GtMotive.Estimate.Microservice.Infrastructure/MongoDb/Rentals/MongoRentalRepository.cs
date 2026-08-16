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

    public async Task<Rental> GetActiveByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        var filter = Builders<RentalDocument>.Filter.And(
            Builders<RentalDocument>.Filter.Eq(rental => rental.VehicleId, vehicleId),
            Builders<RentalDocument>.Filter.Eq(rental => rental.Status, RentalStatus.Active.ToString()));
        var document = await _rentals.Find(filter).SingleOrDefaultAsync(cancellationToken);
        return document is null ? null : RentalMapper.ToDomain(document);
    }

    public async Task<CloseActiveRentalResult> TryCloseActiveAsync(
        Rental rental,
        CancellationToken cancellationToken)
    {
        var filter = Builders<RentalDocument>.Filter.And(
            Builders<RentalDocument>.Filter.Eq(item => item.Id, rental.Id),
            Builders<RentalDocument>.Filter.Eq(item => item.PersonId, rental.PersonId.Value),
            Builders<RentalDocument>.Filter.Eq(item => item.VehicleId, rental.VehicleId),
            Builders<RentalDocument>.Filter.Eq(item => item.Status, RentalStatus.Active.ToString()));
        var update = Builders<RentalDocument>.Update
            .Set(item => item.Status, RentalStatus.Closed.ToString())
            .Set(item => item.EndedAt, rental.EndedAt.Value.UtcDateTime);
        var result = await _rentals.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount == 1
            ? CloseActiveRentalResult.Closed
            : CloseActiveRentalResult.Conflict;
    }
}
