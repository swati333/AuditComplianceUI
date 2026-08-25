using System.Security.Claims;
using System.Text.Encodings.Web;
using Ehs.Observability.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finding.IntegrationTests;

/// <summary>
/// Replaces real Entra ID JWT bearer validation with a fabricated
/// <see cref="ClaimsPrincipal"/> for tests — see Audit Service's identical
/// handler for the full rationale.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public const string AnonymousHeader = "X-Test-Anonymous";

    public const string RolesHeader = "X-Test-Roles";

    /// <summary>Not a real role — use as the <see cref="RolesHeader"/> value for "authenticated, but no roles at all" (an empty header value is unreliable across HTTP client/server layers, so this sentinel is unambiguous).</summary>
    public const string NoRoles = "None";

    public const string UserIdHeader = "X-Test-UserId";

    public const string DefaultUserId = "11111111-1111-1111-1111-111111111111";
    public const string DefaultTenantId = "22222222-2222-2222-2222-222222222222";

    private static readonly string[] AllRoles =
    [
        EntraAppRoles.AuditManage,
        EntraAppRoles.AuditPerform,
        EntraAppRoles.FindingManage,
        EntraAppRoles.ActionManageOwn,
        EntraAppRoles.ActionApprove,
        EntraAppRoles.ReportView,
        EntraAppRoles.ConfigurationManage,
    ];

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey(AnonymousHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers.TryGetValue(UserIdHeader, out var userIdValues)
            ? userIdValues.ToString()
            : DefaultUserId;

        var roles = Request.Headers.TryGetValue(RolesHeader, out var rolesValues)
            ? rolesValues.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : AllRoles;

        var claims = new List<Claim>
        {
            new("oid", userId),
            new(ClaimTypes.NameIdentifier, userId),
            new("tid", DefaultTenantId),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
