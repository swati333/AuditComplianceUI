using System.Net;
using System.Net.Http.Json;
using Ehs.Observability.Security;
using FluentAssertions;
using Notification.Contracts.Requests;

namespace Notification.IntegrationTests;

/// <summary>
/// CLAUDE.md §10: see Audit Service's identical <c>AuthorizationApiTests</c>
/// for the 401/403 rationale. Also covers the ownership check
/// <c>NotificationPreferencesController</c> adds on top of plain
/// authentication, since <c>userId</c> there is caller-supplied routing, not
/// a claim (see that controller's doc comment).
/// </summary>
public sealed class AuthorizationApiTests : IntegrationTestBase
{
    public AuthorizationApiTests(NotificationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTemplates_without_any_credentials_returns_401()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.AnonymousHeader, "true");

        var response = await Client.GetAsync("/api/v1/notification-templates");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTemplates_authenticated_without_CanManageConfiguration_returns_403()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.AuditManage);

        var response = await Client.GetAsync("/api/v1/notification-templates");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTemplates_authenticated_with_CanManageConfiguration_succeeds()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.ConfigurationManage);

        var response = await Client.GetAsync("/api/v1/notification-templates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPreferences_for_a_different_user_without_CanManageConfiguration_returns_403()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, TestAuthHandler.NoRoles);
        Client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "caller-user");

        var response = await Client.GetAsync("/api/v1/notification-preferences/someone-else");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPreferences_for_your_own_userId_succeeds_with_no_special_role()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, TestAuthHandler.NoRoles);
        Client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "caller-user");

        var response = await Client.GetAsync("/api/v1/notification-preferences/caller-user");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Upsert_preferences_for_a_different_user_with_CanManageConfiguration_succeeds()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.ConfigurationManage);
        Client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "admin-user");

        var response = await Client.PutAsJsonAsync("/api/v1/notification-preferences/someone-else/Email", new UpsertPreferenceRequest(true));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
