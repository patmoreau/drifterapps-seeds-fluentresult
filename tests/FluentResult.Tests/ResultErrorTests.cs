namespace FluentResult.Tests;

[UnitTest]
public class ResultErrorTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void GivenCreate_WhenErrorIsNone_ThenErrorIsNone()
    {
        // Arrange

        // Act
        var error = new ResultError(string.Empty, string.Empty);

        // Assert
        error.Should().Be(ResultError.None);
    }

    [Fact]
    public void GivenCreate_WhenCodeIsEmptyButDescriptionIsNot_ThenThrowArgumentException()
    {
        // Arrange
        var code = string.Empty;
        var description = _faker.Random.String();

        // Act
        var action = () => new ResultError(code, description);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenCreate_WhenDescriptionIsEmptyButCodeIsNot_ThenThrowArgumentException()
    {
        // Arrange
        var code = _faker.Random.String();
        var description = string.Empty;

        // Act
        var action = () => new ResultError(code, description);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenCreate_WhenValid_ThenObjectCreated()
    {
        // Arrange
        var code = _faker.Random.String();
        var description = _faker.Random.String();

        // Act
        var error = new ResultError(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
    }

    [Fact]
    public void GivenDeconstruct_WhenValid_ThenValuesDeconstructed()
    {
        // Arrange
        var code = _faker.Random.String();
        var description = _faker.Random.String();
        var error = new ResultError(code, description);

        // Act
        error.Deconstruct(out var codeValue, out var descriptionValue);

        // Assert
        codeValue.Should().Be(code);
        descriptionValue.Should().Be(description);
    }

    [Fact]
    public void GivenResultAssertions_WhenSame_ThenDontAssert()
    {
        // Arrange
        var code = _faker.Random.String();
        var description = _faker.Random.String();
        var error1 = new ResultError(code, description);
        var error2 = new ResultError(code, description);

        // Act
        Result<Nothing> result = error1;

        // Assert
        result.Should().BeFailure().And.WithError(error2);
    }
}
