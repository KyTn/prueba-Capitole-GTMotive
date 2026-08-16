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

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases
{
    /// <summary>
    /// Interface to define the Standard Output Port.
    /// </summary>
    /// <typeparam name="TUseCaseOutput">Tyoe of the use case response dto.</typeparam>
    public interface IOutputPortStandard<in TUseCaseOutput>
        where TUseCaseOutput : IUseCaseOutput
    {
        /// <summary>
        /// Writes to the Standard Output.
        /// </summary>
        /// <param name="response">The Output Port Message.</param>
        void StandardHandle(TUseCaseOutput response);
    }
}
