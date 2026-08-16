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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class RecordingTelemetry : ITelemetry
{
    private readonly ConcurrentQueue<(string Name, IReadOnlyDictionary<string, string> Properties)> _events = new();
    private readonly ConcurrentQueue<(string Name, double Value, IReadOnlyDictionary<string, string> Properties)> _metrics = new();

    public IReadOnlyCollection<(string Name, IReadOnlyDictionary<string, string> Properties)> Events =>
        _events.ToArray();

    public IReadOnlyCollection<(string Name, double Value, IReadOnlyDictionary<string, string> Properties)> Metrics =>
        _metrics.ToArray();

    public void TrackEvent(
        string eventName,
        IDictionary<string, string> properties = null,
        IDictionary<string, double> metrics = null) =>
        _events.Enqueue((eventName, new Dictionary<string, string>(properties ?? new Dictionary<string, string>())));

    public void TrackMetric(
        string name,
        double value,
        IDictionary<string, string> properties = null) =>
        _metrics.Enqueue((name, value, new Dictionary<string, string>(properties ?? new Dictionary<string, string>())));
}
