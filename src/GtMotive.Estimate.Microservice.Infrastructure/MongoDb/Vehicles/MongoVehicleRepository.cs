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

using System.Collections.Generic;
using System;
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

    public async Task<Vehicle> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _vehicles.Find(vehicle => vehicle.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : VehicleMapper.ToDomain(document);
    }
}
