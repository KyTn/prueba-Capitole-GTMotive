using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.People;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using Microsoft.Extensions.Options;

namespace GtMotive.Estimate.Microservice.Infrastructure.People;

public sealed class HttpPersonRegistry : IPersonRegistry, IDisposable
{
    private readonly HttpClient _client;

    public HttpPersonRegistry(IOptions<PersonRegistrySettings> options)
    {
        _client = new HttpClient { BaseAddress = new Uri(options.Value.BaseUrl, UriKind.Absolute) };
    }

    public async Task<bool> ExistsAsync(PersonId personId, CancellationToken cancellationToken)
    {
        using var response = await _client.GetAsync($"persons/{personId.Value:D}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public void Dispose() => _client.Dispose();
}
