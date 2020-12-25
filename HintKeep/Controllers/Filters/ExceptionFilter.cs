using System.Net;
using HintKeep.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HintKeep.Controllers.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case PreconditionFailedException preconditionFailedException when !string.IsNullOrWhiteSpace(preconditionFailedException.Message):
                    context.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.PreconditionFailed,
                        Content = preconditionFailedException.Message
                    };
                    break;
                    
                case PreconditionFailedException preconditionFailedException:
                    context.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.PreconditionFailed
                    };
                    break;

                default:
                    context.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                    break;
            }
        }
    }
}