using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Controllers.Filters
{
    public class ActiveSessionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IEntityTables _entityTables;

        public ActiveSessionAuthorizationFilter(IEntityTables entityTables)
            => _entityTables = entityTables;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirstValue(ClaimTypes.Name);
                var sessionId = user.FindFirstValue(ClaimTypes.SerialNumber);
                var entities = await Task.WhenAll(
                    _entityTables
                        .Users
                        .ExecuteAsync(TableOperation.Retrieve(userId.ToEncodedKeyProperty(), "details".ToEncodedKeyProperty(), new List<string>()))
                        .ContinueWith(task => task.Result.Result),
                    _entityTables
                        .UserSessions
                        .ExecuteAsync(TableOperation.Retrieve<UserSessionEntity>(userId.ToEncodedKeyProperty(), sessionId.ToEncodedKeyProperty(), new List<string> { nameof(UserSessionEntity.Expiration) }))
                        .ContinueWith(task => task.Result.Result)
                );

                if (entities.Any(result => result is null) || entities.OfType<UserSessionEntity>().Single().Expiration < DateTime.UtcNow)
                    context.Result = new UnauthorizedObjectResult(string.Empty);
            }
        }
    }
}