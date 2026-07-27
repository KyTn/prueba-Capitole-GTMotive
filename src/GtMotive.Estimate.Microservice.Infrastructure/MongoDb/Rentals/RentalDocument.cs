using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Rentals;

public sealed class RentalDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid PersonId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid VehicleId { get; set; }

    public DateTime StartedAt { get; set; }

    public string Status { get; set; }
}
