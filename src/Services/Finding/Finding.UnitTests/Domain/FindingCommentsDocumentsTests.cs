using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using Finding.Domain.Enums;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.UnitTests.Domain;

public class FindingCommentsDocumentsTests
{
    private const string Actor = "user@example.com";

    private static FindingEntity CreateFinding() =>
        FindingEntity.Create(Guid.NewGuid(), "Finding", null, FindingSeverity.Medium, Actor);

    [Fact]
    public void AddComment_appends_to_Comments()
    {
        var finding = CreateFinding();

        var comment = finding.AddComment("user-1", "Alice", "Looks resolved to me.", Actor);

        finding.Comments.Should().ContainSingle().Which.Should().BeSameAs(comment);
    }

    [Fact]
    public void AddComment_rejects_empty_text()
    {
        var finding = CreateFinding();

        var act = () => finding.AddComment("user-1", "Alice", "  ", Actor);

        act.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void AddDocument_appends_to_Documents_with_metadata_only()
    {
        var finding = CreateFinding();

        var document = finding.AddDocument("photo.jpg", "blob://findings/photo.jpg", "image/jpeg", 204800, Actor);

        finding.Documents.Should().ContainSingle().Which.Should().BeSameAs(document);
        document.SizeBytes.Should().Be(204800);
    }

    [Fact]
    public void AddDocument_rejects_an_empty_blob_reference()
    {
        var finding = CreateFinding();

        var act = () => finding.AddDocument("photo.jpg", "", "image/jpeg", 100, Actor);

        act.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void RecordRootCauseAnalysis_sets_text_author_and_timestamp()
    {
        var finding = CreateFinding();

        finding.RecordRootCauseAnalysis("Vendor contract lapsed.", Actor);

        finding.RootCauseAnalysis.Should().Be("Vendor contract lapsed.");
        finding.RootCauseAnalysisBy.Should().Be(Actor);
        finding.RootCauseAnalysisAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void RecordRootCauseAnalysis_rejects_empty_text()
    {
        var finding = CreateFinding();

        var act = () => finding.RecordRootCauseAnalysis("", Actor);

        act.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void Comments_and_documents_cannot_be_added_once_Closed()
    {
        var finding = CreateFinding();
        finding.StartReview(Actor);
        finding.RequireAction(Actor);
        finding.Resolve(true, Actor);
        finding.Verify(Actor);
        finding.Close(Actor);

        var act = () => finding.AddComment("user-1", "Alice", "Too late.", Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "FINDING_NOT_EDITABLE");
    }
}
