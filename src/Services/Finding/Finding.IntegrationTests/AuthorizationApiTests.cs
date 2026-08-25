using System.Net;
using System.Net.Http.Json;
using Ehs.Observability.Security;
using Finding.Contracts.Dtos;
using Finding.Contracts.Requests;
using FluentAssertions;

namespace Finding.IntegrationTests;

/// <summary>
/// CLAUDE.md §10: see Audit Service's identical
/// <c>AuthorizationApiTests</c> for the full rationale — every other test
/// class here relies on <see cref="IntegrationTestBase"/>'s default,
/// fully-privileged principal, so 401/403 paths are only exercised here.
/// </summary>
public sealed class AuthorizationApiTests : IntegrationTestBase
{
    public AuthorizationApiTests(FindingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_without_any_credentials_returns_401()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.AnonymousHeader, "true");

        var response = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(Guid.NewGuid(), "Title", "desc", "High"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_authenticated_without_CanManageFindings_returns_403()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.AuditPerform);

        var response = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(Guid.NewGuid(), "Title", "desc", "High"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_authenticated_with_CanManageFindings_succeeds()
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, EntraAppRoles.FindingManage);

        var response = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(Guid.NewGuid(), "Title", "desc", "High"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddComment_only_requires_authentication_not_CanManageFindings()
    {
        var createResponse = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(Guid.NewGuid(), "Title", "desc", "Low"));
        var finding = (await createResponse.Content.ReadFromJsonAsync<FindingDetailDto>())!;

        Client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, TestAuthHandler.NoRoles);
        var response = await Client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/comments", new AddCommentRequest("user-1", "Test User", "a comment"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
