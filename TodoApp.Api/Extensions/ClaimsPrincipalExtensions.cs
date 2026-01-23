using System;
using System.Security.Claims;

namespace TodoApp.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal user)
    {
        var idString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idString))
        {
            throw new UnauthorizedAccessException("ID do usuário não encontrado no token.");
        }

        return Guid.Parse(idString);
    }
}