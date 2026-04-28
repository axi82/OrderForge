using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderForge.Api.Controllers;
using OrderForge.Application.Profile;

namespace OrderForge.Api.UnitTests.Controllers;

public sealed class ProfileControllerTests
{
    private readonly Mock<ISender> _mediator = new();

    [Fact]
    public async Task UpdateProfile_sends_UpdateMyProfileCommand_and_returns_NoContent()
    {
        var sut = new ProfileController(_mediator.Object);
        var body = new UpdateMyProfileBody("Jane", "Doe");

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdateMyProfileCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await sut.UpdateProfile(body, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _mediator.Verify(
            m => m.Send(
                    It.Is<UpdateMyProfileCommand>(c => c.FirstName == "Jane" && c.LastName == "Doe"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_coalesces_null_names_to_empty_strings()
    {
        var sut = new ProfileController(_mediator.Object);
        var body = new UpdateMyProfileBody(null, null);

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdateMyProfileCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sut.UpdateProfile(body, CancellationToken.None);

        _mediator.Verify(
            m => m.Send(
                    It.Is<UpdateMyProfileCommand>(c => c.FirstName == "" && c.LastName == ""),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task ChangePassword_sends_ChangeMyPasswordCommand_and_returns_NoContent()
    {
        var sut = new ProfileController(_mediator.Object);
        var body = new ChangeMyPasswordBody("old-secret", "new-secret", "new-secret");

        _mediator
            .Setup(m => m.Send(It.IsAny<ChangeMyPasswordCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await sut.ChangePassword(body, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _mediator.Verify(
            m => m.Send(
                    It.Is<ChangeMyPasswordCommand>(c =>
                        c.CurrentPassword == "old-secret"
                        && c.NewPassword == "new-secret"
                        && c.ConfirmPassword == "new-secret"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task ChangePassword_coalesces_null_fields_to_empty_strings()
    {
        var sut = new ProfileController(_mediator.Object);
        var body = new ChangeMyPasswordBody(null, null, null);

        _mediator
            .Setup(m => m.Send(It.IsAny<ChangeMyPasswordCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sut.ChangePassword(body, CancellationToken.None);

        _mediator.Verify(
            m => m.Send(
                    It.Is<ChangeMyPasswordCommand>(c =>
                        c.CurrentPassword == ""
                        && c.NewPassword == ""
                        && c.ConfirmPassword == ""),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_propagates_mediator_exceptions()
    {
        var sut = new ProfileController(_mediator.Object);
        var body = new UpdateMyProfileBody("A", "B");

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdateMyProfileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("validation failed"));

        var act = async () => await sut.UpdateProfile(body, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("validation failed");
    }
}
