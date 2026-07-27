using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string TestScheme = "Test";
    public const string PermissionsHeader = "X-Test-Permissions";
    public const string AnonymousHeader = "X-Test-Anonymous";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey(AnonymousHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var permissions = Request.Headers.TryGetValue(PermissionsHeader, out var values)
            ? values
                .SelectMany(value => value.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
                .Select(value => value.Trim())
            : AuthorizationCatalog.Policies.All;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "integration-test-user")
        };
        claims.AddRange(permissions.Select(
            permission => new Claim(AuthorizationCatalog.PermissionClaimType, permission)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, TestScheme));
        var ticket = new AuthenticationTicket(principal, TestScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
