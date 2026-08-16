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
