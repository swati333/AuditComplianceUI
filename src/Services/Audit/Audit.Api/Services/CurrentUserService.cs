using Audit.Application.Common;

namespace Audit.Api.Services;

/// <summary>
/// Reads the caller's identity from validated Entra ID JWT claims only
/// (CLAUDE.md §10) — never from a client-supplied header or request body.
/// <c>oid</c> (object id) identifies the user; <c>tid</c> identifies the
/// Entra tenant. Both are stamped onto the token by Entra itself during
/// sign-in, so by the time JWT bearer middleware has validated the token's
/// signature/issuer/audience/lifetime, these claims cannot have been forged
/// by the caller.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private const string NoAuthPlaceholder = "anonymous@no-auth-configured";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("oid")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? NoAuthPlaceholder;

    public string TenantId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("tid")?.Value
        ?? NoAuthPlaceholder;
}
