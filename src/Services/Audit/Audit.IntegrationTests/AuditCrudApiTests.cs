using System.Net;
using System.Net.Http.Json;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Ehs.SharedKernel.Pagination;
using FluentAssertions;

namespace Audit.IntegrationTests;

public sealed class AuditCrudApiTests : IntegrationTestBase
{
    public AuditCrudApiTests(AuditApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_returns_201_with_Location_header_and_Draft_status()
    {
        var request = new CreateAuditRequest("Fire Safety Audit", "desc", "Full site", "Plant A");

        var response = await Client.PostAsJsonAsync("/api/v1/audits", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var body = await response.Content.ReadFromJsonAsync<AuditDetailDto>();
        body!.Status.Should().Be("Draft");
        body.Title.Should().Be("Fire Safety Audit");
    }

    [Fact]
    public async Task Create_with_empty_title_returns_400_ProblemDetails_with_field_errors()
    {
        var request = new CreateAuditRequest("", "desc", "Scope", "Location");

        var response = await Client.PostAsJsonAsync("/api/v1/audits", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("VALIDATION_ERROR");
        problem.GetProperty("errors").GetProperty("Title").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetById_returns_the_created_audit()
    {
        var created = await CreateAuditAsync();

        var response = await Client.GetAsync($"/api/v1/audits/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuditDetailDto>();
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetById_for_an_unknown_id_returns_404_with_error_code()
    {
        var response = await Client.GetAsync($"/api/v1/audits/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("RESOURCE_NOT_FOUND");
        problem.TryGetProperty("traceId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task List_returns_a_paged_result_containing_a_created_audit()
    {
        var created = await CreateAuditAsync(title: $"Paged Audit {Guid.NewGuid()}");

        var response = await Client.GetAsync("/api/v1/audits?pageNumber=1&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AuditSummaryDto>>();
        page!.Items.Should().Contain(a => a.Id == created.Id);
    }

    [Fact]
    public async Task List_filters_by_status()
    {
        var response = await Client.GetAsync("/api/v1/audits?status=Draft&pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AuditSummaryDto>>();
        page!.Items.Should().OnlyContain(a => a.Status == "Draft");
    }

    [Fact]
    public async Task Update_with_a_stale_RowVersion_returns_409_concurrency_conflict()
    {
        var created = await CreateAuditAsync();

        // First update succeeds and changes the RowVersion server-side.
        var firstUpdate = new UpdateAuditRequest("Updated title", "desc", "Scope", "Location", created.RowVersion);
        (await Client.PutAsJsonAsync($"/api/v1/audits/{created.Id}", firstUpdate)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Second update reuses the now-stale RowVersion from the original GET.
        var staleUpdate = new UpdateAuditRequest("Conflicting title", "desc", "Scope", "Location", created.RowVersion);
        var response = await Client.PutAsJsonAsync($"/api/v1/audits/{created.Id}", staleUpdate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("CONCURRENCY_CONFLICT");
    }

    [Fact]
    public async Task Delete_soft_deletes_so_a_subsequent_GetById_returns_404()
    {
        var created = await CreateAuditAsync();

        var deleteResponse = await Client.DeleteAsync($"/api/v1/audits/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/v1/audits/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<AuditDetailDto> CreateAuditAsync(string title = "Audit")
    {
        var response = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest(title, "desc", "Scope", "Location"));
        return (await response.Content.ReadFromJsonAsync<AuditDetailDto>())!;
    }
}
