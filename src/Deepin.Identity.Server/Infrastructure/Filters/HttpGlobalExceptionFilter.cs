using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Net;
using Deepin.Identity.Domain;
using System.ComponentModel.DataAnnotations;
using Deepin.Identity.Server.Infrastructure.ActionResults;
using Microsoft.EntityFrameworkCore;

namespace Deepin.Identity.Server.Infrastructure.Filters;

public class HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger, IWebHostEnvironment env) : IExceptionFilter
{
    private readonly ILogger<HttpGlobalExceptionFilter> _logger = logger;
    public void OnException(ExceptionContext context)
    {
        logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message);
        if (context.Exception.GetType() == typeof(DomainException))
        {
            var problemDetails = new HttpValidationProblemDetails()
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status400BadRequest,
                Detail = "Please refer to the errors property for additional details."
            };
            if (context.Exception.InnerException is null)
            {
                problemDetails.Errors.Add("DomainValidations", [context.Exception.Message]);
            }
            context.Result = new BadRequestObjectResult(problemDetails);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else
        {
            if (context.Exception.GetType() == typeof(DbUpdateException) && context.Exception.InnerException is not null)
            {
                _logger.LogError(new EventId(context.Exception.InnerException.HResult), context.Exception.InnerException, context.Exception.InnerException.Message);
            }
            var json = new JObject(new JProperty("Messages", ["An error occur.Try it again."]));
            if (env.IsDevelopment())
            {
                json.Add(new JProperty("Exception", JToken.FromObject(context.Exception)));
            }
            context.Result = new InternalServerErrorObjectResult(json);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        context.ExceptionHandled = true;
    }
}
