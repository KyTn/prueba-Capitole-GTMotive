using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

public sealed record ApiAuthorizationRequirement(
    string ResourceName,
    IReadOnlyList<string> PolicyNames) : IAuthorizationRequirement;

