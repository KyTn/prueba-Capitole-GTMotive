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
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GtMotive.Estimate.Microservice.Api.Filters
{
    public sealed class BusinessExceptionFilter(IAppLogger<BusinessExceptionFilter> appLogger) : IExceptionFilter
    {
        private readonly IAppLogger<BusinessExceptionFilter> _appLogger = appLogger;

        public void OnException(ExceptionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _appLogger.LogError(context.Exception, "Exception captured in BusinessExceptionFilter.");

            if (context.Exception is DomainException)
            {
                var problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = context.Exception.Message,
                    Instance = context.HttpContext.Request.Path,
                };

                _appLogger.LogWarning("Domain Exception: {status} - {detail}", problemDetails.Status, problemDetails.Detail);

                context.Result = new BadRequestObjectResult(problemDetails);
                context.Exception = null;
            }
            else
            {
                var problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = context.Exception.Message,
                    Instance = context.HttpContext.Request.Path,
                };

                _appLogger.LogError(context.Exception, "Unhandled Exception");

                context.Result = new InternalServerErrorObjectResult(problemDetails);
                context.Exception = null;
            }
        }
    }
}
