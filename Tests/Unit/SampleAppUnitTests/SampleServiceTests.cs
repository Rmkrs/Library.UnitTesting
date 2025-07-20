namespace SampleAppUnitTests;

using FluentAssertions;
using Library.UnitTesting.Common;
using Moq;
using NUnit.Framework;
using SampleApp;
using SampleApp.Contracts;

[TestFixture]
public class SampleServiceTests : UnitTestBase<SampleService>
{
    private readonly Mock<IDateTimeProvider> dateTimeProviderMock = new();

    [Test]
    public void Greet_Valid_ReturnsCorrectResult()
    {
        // Arrange
        var name = this.Instantiator.Random<string>();
        var dateTime = this.Instantiator.Random<DateTime>();
        var expected = $"Hello, {name}. It's {dateTime:HH:mm}.";

        this.dateTimeProviderMock.Setup(x => x.Now).Returns(dateTime);

        var target = this.GetTarget();

        // Act
        var actual = target.Greet(name);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    protected override SampleService GetTarget()
    {
        return new SampleService(this.dateTimeProviderMock.Object);
    }
}