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
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

internal static class UseCaseTelemetry
{
    internal const string CompletedEvent = "UseCaseCompleted";
    internal const string DurationMetric = "UseCaseDurationMs";

    internal static void Track(ITelemetry telemetry, string operation, string outcome, double durationMs)
    {
        try
        {
            var properties = new Dictionary<string, string>
            {
                ["operation"] = operation,
                ["outcome"] = outcome,
            };
            telemetry.TrackEvent(CompletedEvent, properties);
            telemetry.TrackMetric(
                DurationMetric,
                durationMs,
                new Dictionary<string, string> { ["operation"] = operation });
        }
        catch
        {
            // Telemetry is observational and must not change the use-case result.
        }
    }
}
