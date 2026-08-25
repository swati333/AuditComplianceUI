using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ehs.SharedKernel.Pagination;
using Finding.Contracts.Dtos;
using Finding.Contracts.Requests;
using FluentAssertions;

namespace Finding.IntegrationTests;

public sealed class FindingCrudApiTests : IntegrationTestBase
{
    public FindingCrudApiTests(FindingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_returns_201_with_Open_status()
    {
        var request = new CreateFindingRequest(Guid.NewGuid(), "Blocked exit", "desc", "High");

        var response = await Client.PostAsJsonAsync("/api/v1/findings", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var body = await response.Content.ReadFromJsonAsync<FindingDetailDto>();
        body!.Status.Should().Be("Open");
        body.Severity.Should().Be("High");
    }

    [Fact]
    public async Task Create_with_empty_title_returns_400_ProblemDetails_with_field_errors()
    {
        var request = new CreateFindingRequest(Guid.NewGuid(), "", null, "Low");

        var response = await Client.PostAsJsonAsync("/api/v1/findings", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("VALIDATION_ERROR");
        problem.GetProperty("errors").GetProperty("Title").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_with_an_unknown_severity_returns_400()
    {
        var request = new CreateFindingRequest(Guid.NewGuid(), "Title", null, "Severe");

        var response = await Client.PostAsJsonAsync("/api/v1/findings", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_returns_the_created_finding()
    {
        var created = await CreateFindingAsync();

        var response = await Client.GetAsync($"/api/v1/findings/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FindingDetailDto>();
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetById_for_an_unknown_id_returns_404_with_error_code()
    {
        var response = await Client.GetAsync($"/api/v1/findings/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("RESOURCE_NOT_FOUND");
    }

    [Fact]
    public async Task List_filters_by_severity()
    {
        await CreateFindingAsync(severity: "Critical");

        var response = await Client.GetAsync("/api/v1/findings?severity=Critical&pageNumber=1&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<FindingSummaryDto>>();
        page!.Items.Should().OnlyContain(f => f.Severity == "Critical");
    }

    [Fact]
    public async Task List_filters_by_AuditId()
    {
        var auditId = Guid.NewGuid();
        var created = await CreateFindingAsync(auditId: auditId);

        var response = await Client.GetAsync($"/api/v1/findings?auditId={auditId}&pageNumber=1&pageSize=50");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<FindingSummaryDto>>();
        page!.Items.Should().ContainSingle(f => f.Id == created.Id);
    }

    [Fact]
    public async Task Update_with_a_stale_RowVersion_returns_409_concurrency_conflict()
    {
        var created = await CreateFindingAsync();

        var firstUpdate = new UpdateFindingRequest("Updated title", "desc", "Medium", created.RowVersion);
        (await Client.PutAsJsonAsync($"/api/v1/findings/{created.Id}", firstUpdate)).StatusCode.Should().Be(HttpStatusCode.OK);

        var staleUpdate = new UpdateFindingRequest("Conflicting title", "desc", "Medium", created.RowVersion);
        var response = await Client.PutAsJsonAsync($"/api/v1/findings/{created.Id}", staleUpdate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("CONCURRENCY_CONFLICT");
    }

    [Fact]
    public async Task Delete_soft_deletes_so_a_subsequent_GetById_returns_404()
    {
        var created = await CreateFindingAsync();

        var deleteResponse = await Client.DeleteAsync($"/api/v1/findings/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/v1/findings/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordRootCauseAnalysis_persists_text()
    {
        var created = await CreateFindingAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/findings/{created.Id}/root-cause-analysis", new RecordRootCauseAnalysisRequest("Vendor contract lapsed."));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FindingDetailDto>();
        body!.RootCauseAnalysis.Should().Be("Vendor contract lapsed.");
    }

    [Fact]
    public async Task AddComment_and_AddDocument_persist_metadata()
    {
        var created = await CreateFindingAsync();

        var commentResponse = await Client.PostAsJsonAsync($"/api/v1/findings/{created.Id}/comments", new AddCommentRequest("user-1", "Alice", "Investigating now."));
        commentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var documentResponse = await Client.PostAsJsonAsync(
            $"/api/v1/findings/{created.Id}/documents",
            new AddDocumentRequest("photo.jpg", "blob://findings/photo.jpg", "image/jpeg", 2048));
        documentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await (await Client.GetAsync($"/api/v1/findings/{created.Id}")).Content.ReadFromJsonAsync<FindingDetailDto>();
        detail!.Comments.Should().ContainSingle(c => c.Text == "Investigating now.");
        detail.Documents.Should().ContainSingle(d => d.FileName == "photo.jpg");
    }

    private async Task<FindingDetailDto> CreateFindingAsync(Guid? auditId = null, string severity = "Low")
    {
        var response = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(auditId ?? Guid.NewGuid(), $"Finding {Guid.NewGuid()}", "desc", severity));
        return (await response.Content.ReadFromJsonAsync<FindingDetailDto>())!;
    }
}
