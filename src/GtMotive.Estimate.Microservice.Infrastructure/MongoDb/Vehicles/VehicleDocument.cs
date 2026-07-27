using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Vehicles;

public sealed class VehicleDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string RegistrationNumber { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public DateTime ManufactureDate { get; set; }
}
