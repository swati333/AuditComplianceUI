using ActionPlan.Application.Common;

namespace ActionPlan.Api.Services;

/// <summary>
/// Reads the caller's identity from validated Entra ID JWT claims only — see
/// Finding.Api's identical implementation for the full rationale.
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
