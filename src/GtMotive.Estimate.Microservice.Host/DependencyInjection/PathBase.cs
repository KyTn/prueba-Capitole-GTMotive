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

namespace GtMotive.Estimate.Microservice.Host.DependencyInjection
{
    internal sealed class PathBase
    {
        public const string DefaultPathBase = "/";

        public PathBase(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || DefaultPathBase.Equals(value, System.StringComparison.Ordinal))
            {
                IsDefault = true;
                CurrentWithoutTrailingSlash = DefaultPathBase;
            }
            else
            {
                IsDefault = false;
                CurrentWithoutTrailingSlash = value.TrimEnd('*').TrimEnd('/');
            }
        }

        public bool IsDefault { get; }

        public string CurrentWithoutTrailingSlash { get; }
    }
}
