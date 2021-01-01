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
                case UnauthorizedException unauthorizedException:
                    context.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Content = string.IsNullOrWhiteSpace(unauthorizedException.Message) ? null : unauthorizedException.Message
                    };
                    break;

                case ConflictException conflictException:
                    context.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.Conflict,
                        Content = string.IsNullOrWhiteSpace(conflictException.Message) ? null : conflictException.Message
                    };
                    break;

                case PreconditionFailedException preconditionFailedException:
                    context.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.PreconditionFailed,
                        Content = string.IsNullOrWhiteSpace(preconditionFailedException.Message) ? null : preconditionFailedException.Message
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