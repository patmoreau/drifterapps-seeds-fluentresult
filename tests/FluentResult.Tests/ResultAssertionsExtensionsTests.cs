namespace FluentResult.Tests;

[UnitTest]
public class ResultAssertionsExtensionsTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void GivenWithValue_WhenSuccessCarriesNull_ThenFailAssertionWithoutDereferencingValue()
    {
        // Arrange
        var expectedValue = Faker.Random.Word();
        var result = Faker.Random.Int().ToResult().Select(_ => (string?) null);

        // Act
        var action = () => result.Should().BeSuccessful().And.WithValue(expectedValue);

        // Assert
        action.Should()
            .Throw<Exception>()
            .WithMessage("*to have value*")
            .Which.Should().NotBeOfType<NullReferenceException>();
    }

    [Fact]
    public void GivenWithValue_WhenExpectedValueIsNullAndSuccessCarriesNull_ThenSucceed()
    {
        // Arrange
        var result = Faker.Random.Int().ToResult().Select(_ => (string?) null);

        // Act
        var action = () => result.Should().BeSuccessful().And.WithValue(null);

        // Assert
        action.Should().NotThrow();
    }
}
