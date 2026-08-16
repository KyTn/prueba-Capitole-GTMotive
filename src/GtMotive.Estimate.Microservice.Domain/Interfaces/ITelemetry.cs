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

namespace GtMotive.Estimate.Microservice.Domain.Interfaces
{
    /// <summary>
    /// Send events, metrics and other telemetry.
    /// </summary>
    public interface ITelemetry
    {
        /// <summary>
        /// User actions and other events. Used to track user behavior or to monitor performance.
        /// Insert TrackEvent calls in your code to count various events. How often users choose a particular feature,
        /// how often they achieve particular goals, or maybe how often they make particular types of mistakes.
        /// </summary>
        /// <param name="eventName">A name for the event. Max length 150.</param>
        /// <param name="properties">Named string values you can use to search and filter events.</param>
        /// <param name="metrics">Numeric measurements associated with this event.</param>
        void TrackEvent(
            string eventName,
            IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null);

        /// <summary>
        /// Performance measurements such as queue lengths not related to specific events.
        /// For get chart metrics that are not attached to particular events. For example, you could monitor a queue length
        /// at regular intervals. With metrics, the individual measurements are of less interest than the variations and trends,
        /// and so statistical charts are useful.
        /// </summary>
        /// <param name="name">The name of the metric. Max length 150.</param>
        /// <param name="value">he value of the metric. Sum if based on more than one sample count.</param>
        /// <param name="properties">Named string values you can use to search and classify trace messages.</param>
        void TrackMetric(
            string name,
            double value,
            IDictionary<string, string> properties = null);
    }
}
