using Finding.Application.Validators;
using Finding.Contracts.Requests;
using FluentAssertions;

namespace Finding.UnitTests.Validators;

public class AddDocumentRequestValidatorTests
{
    private readonly AddDocumentRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var request = new AddDocumentRequest("photo.jpg", "blob://findings/photo.jpg", "image/jpeg", 1024);

        _validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Zero_size_fails()
    {
        var request = new AddDocumentRequest("photo.jpg", "blob://findings/photo.jpg", "image/jpeg", 0);

        _validator.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Oversized_file_fails()
    {
        var request = new AddDocumentRequest("huge.zip", "blob://findings/huge.zip", "application/zip", 200L * 1024 * 1024);

        _validator.Validate(request).IsValid.Should().BeFalse();
    }
}
