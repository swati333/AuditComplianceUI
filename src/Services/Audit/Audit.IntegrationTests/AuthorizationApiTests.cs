using System.Net;
using System.Net.Http.Json;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Ehs.Observability.Security;
using FluentAssertions;

namespace Audit.IntegrationTests;

/// <summary>
/// CLAUDE.md §10: policy-based authorization must actually reject an
/// unauthenticated caller (401) and an authenticated-but-under-privileged
/// caller (403), not just accept a fully-privileged one — every other test
/// class in this project uses <see cref="IntegrationTestBase"/>'s default,
/// fully-privileged principal, so this is the only place those two paths are
/// exercised.
/// </summary>
public sealed class AuthorizationApiTests : IntegrationTestBase
{
    public AuthorizationApiTests(AuditApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_without_any_credentials_returns_401()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.AnonymousHeader, "true");

        var response = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest("Title", "desc", "scope", "loc"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_authenticated_without_CanManageAudits_returns_403()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.AuditPerform);

        var response = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest("Title", "desc", "scope", "loc"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_authenticated_with_CanManageAudits_succeeds()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.AuditManage);

        var response = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest("Title", "desc", "scope", "loc"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Start_requires_CanPerformAudits_not_CanManageAudits()
    {
        var audit = await PlanAuditAsync();

        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.AuditManage);
        var deniedResponse = await Client.PostAsync($"/api/v1/audits/{audit.Id}/start", content: null);
        deniedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Client.DefaultRequestHeaders.Remove(TestAuthHandler.RolesHeader);
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.AuditPerform);
        var allowedResponse = await Client.PostAsync($"/api/v1/audits/{audit.Id}/start", content: null);
        allowedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_only_requires_authentication_not_a_specific_role()
    {
        var audit = await CreateAuditAsync();

        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, TestAuthHandler.NoRoles);
        var response = await Client.GetAsync($"/api/v1/audits/{audit.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<AuditDetailDto> CreateAuditAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest($"Audit {Guid.NewGuid()}", "desc", "scope", "loc"));
        return (await response.Content.ReadFromJsonAsync<AuditDetailDto>())!;
    }

    private async Task<AuditDetailDto> PlanAuditAsync()
    {
        var audit = await CreateAuditAsync();
        await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/team-members", new AssignTeamMemberRequest("auditor-1", "Auditor One", "Auditor"));

        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddDays(2);
        var response = await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/plan", new PlanAuditRequest(start, end));
        return (await response.Content.ReadFromJsonAsync<AuditDetailDto>())!;
    }
}
