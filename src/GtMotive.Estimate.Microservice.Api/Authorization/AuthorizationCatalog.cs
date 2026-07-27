using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

public static class AuthorizationCatalog
{
    public const string PermissionClaimType = "permission";

    public static class Resources
    {
        public const string Vehicles = nameof(Vehicles);
        public const string Rentals = nameof(Rentals);

        public static IReadOnlySet<string> All { get; } =
            new HashSet<string>(StringComparer.Ordinal) { Vehicles, Rentals };
    }

    public static class Policies
    {
        public const string VehiclesCreate = "Vehicles.Create";
        public const string VehiclesRead = "Vehicles.Read";
        public const string RentalsCreate = "Rentals.Create";
        public const string RentalsReturn = "Rentals.Return";

        public static IReadOnlySet<string> All { get; } =
            new HashSet<string>(StringComparer.Ordinal)
            {
                VehiclesCreate,
                VehiclesRead,
                RentalsCreate,
                RentalsReturn
            };
    }

    public static IReadOnlyDictionary<string, string> PolicyResources { get; } =
        new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [Policies.VehiclesCreate] = Resources.Vehicles,
                [Policies.VehiclesRead] = Resources.Vehicles,
                [Policies.RentalsCreate] = Resources.Rentals,
                [Policies.RentalsReturn] = Resources.Rentals
            });

    public static bool IsKnownResource(string resourceName) =>
        resourceName is not null && Resources.All.Contains(resourceName);

    public static bool IsKnownPolicy(string policyName) =>
        policyName is not null && Policies.All.Contains(policyName);

    public static bool IsPolicyForResource(string policyName, string resourceName) =>
        policyName is not null &&
        resourceName is not null &&
        PolicyResources.TryGetValue(policyName, out var expectedResource) &&
        string.Equals(expectedResource, resourceName, StringComparison.Ordinal);
}
