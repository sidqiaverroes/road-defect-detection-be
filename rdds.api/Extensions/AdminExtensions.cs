using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using rdds.api.Models;

namespace rdds.api.Extensions
{
    public class AdminRequirement : IAuthorizationRequirement { }

    public class AdminHandler : AuthorizationHandler<AdminRequirement>
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var hasAdminRole = context.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

            if (hasAdminRole)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}