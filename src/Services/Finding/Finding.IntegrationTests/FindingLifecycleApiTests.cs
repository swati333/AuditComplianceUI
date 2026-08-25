using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Finding.Contracts.Dtos;
using Finding.Contracts.Requests;
using FluentAssertions;

namespace Finding.IntegrationTests;

/// <summary>
/// End-to-end coverage of CLAUDE.md §2's finding lifecycle over real HTTP +
/// a real SQL Server. The corrective-action gate on Resolve() can't be
/// exercised as "blocked" here — ActionPlanGateway is a permissive
/// placeholder until Action Plan Service exists (see
/// Finding.Infrastructure's ActionPlanGateway) — but the rule itself is
/// fully unit-tested on the aggregate with both true and false.
/// </summary>
public sealed class FindingLifecycleApiTests : IntegrationTestBase
{
    public FindingLifecycleApiTests(FindingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Full_lifecycle_from_Open_to_Closed_succeeds_for_a_Critical_finding()
    {
        var finding = await CreateFindingAsync(severity: "Critical");

        var underReview = await PostAndReadAsync($"/api/v1/findings/{finding.Id}/start-review");
        underReview.Status.Should().Be("UnderReview");

        var actionRequired = await PostAndReadAsync($"/api/v1/findings/{finding.Id}/require-action");
        actionRequired.Status.Should().Be("ActionRequired");

        var resolved = await PostAndReadAsync($"/api/v1/findings/{finding.Id}/resolve");
        resolved.Status.Should().Be("Resolved");

        var verified = await PostAndReadAsync($"/api/v1/findings/{finding.Id}/verify");
        verified.Status.Should().Be("Verified");

        var closed = await PostAndReadAsync($"/api/v1/findings/{finding.Id}/close");
        closed.Status.Should().Be("Closed");

        var historyResponse = await Client.GetAsync($"/api/v1/findings/{finding.Id}/status-history");
        var history = await historyResponse.Content.ReadFromJsonAsync<List<FindingStatusHistoryDto>>();
        history!.Select(h => h.ToStatus).Should().ContainInOrder("Open", "UnderReview", "ActionRequired", "Resolved", "Verified", "Closed");
    }

    [Fact]
    public async Task Resolving_an_Open_finding_without_going_through_the_chain_is_rejected_with_409()
    {
        var finding = await CreateFindingAsync();

        var response = await Client.PostAsync($"/api/v1/findings/{finding.Id}/resolve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task Closing_before_Verified_is_rejected_with_409()
    {
        var finding = await CreateFindingAsync();
        await Client.PostAsync($"/api/v1/findings/{finding.Id}/start-review", null);
        await Client.PostAsync($"/api/v1/findings/{finding.Id}/require-action", null);
        await Client.PostAsync($"/api/v1/findings/{finding.Id}/resolve", null);

        var response = await Client.PostAsync($"/api/v1/findings/{finding.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("INVALID_STATUS_TRANSITION");
    }

    private async Task<FindingDetailDto> CreateFindingAsync(string severity = "Low")
    {
        var response = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(Guid.NewGuid(), $"Finding {Guid.NewGuid()}", "desc", severity));
        return (await response.Content.ReadFromJsonAsync<FindingDetailDto>())!;
    }

    private async Task<FindingDetailDto> PostAndReadAsync(string url)
    {
        var response = await Client.PostAsync(url, null);
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: $"POST {url} should succeed");
        return (await response.Content.ReadFromJsonAsync<FindingDetailDto>())!;
    }
}
