using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using HintKeep.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

                case NotFoundException notFoundException:
                    context.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Content = string.IsNullOrWhiteSpace(notFoundException.Message) ? null : notFoundException.Message
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

                case ValidationException validationException:
                    if (validationException.ValidationResult.MemberNames.Any())
                        foreach (var validationResultMemberName in validationException.ValidationResult.MemberNames)
                            context.ModelState.AddModelError(validationResultMemberName, validationException.ValidationResult.ErrorMessage);
                    else
                        context.ModelState.AddModelError("*", validationException.ValidationResult.ErrorMessage);
                    context.Result = new UnprocessableEntityObjectResult(context.ModelState);
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