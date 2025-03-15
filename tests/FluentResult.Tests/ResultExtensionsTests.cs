using System.Globalization;

namespace FluentResult.Tests;

[UnitTest]
public partial class ResultExtensionsTests
{
    private static readonly Faker Faker = new();
    private static readonly ResultError TestFirstError = new("TestFirstError", "Test Error");

    [Fact]
    public void GivenToResult_WhenInvoked_ThenReturnSuccess()
    {
        // Arrange
        var number = Faker.Random.Int();

        // Act
        var result = number.ToResult();

        // Assert
        result.Should().BeSuccessful().And.WithValue(number);
    }

    [Fact]
    public void GivenToResult_WhenInvokedFromNull_ThenThrowArgumentNullException()
    {
        // Arrange
        object number = null!;

        // Act
        var action = () => number.ToResult();

        // Assert
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void GivenToResult_WhenTypeIsResultError_ThenThrowInvalidOperationException()
    {
        // Arrange

        // Act
        var action = () => TestFirstError.ToResult();

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("ResultError is not allowed.");
    }

    [Fact]
    public void GivenToResult_WhenInvokedWithResultError_ThenReturnFailure()
    {
        // Arrange

        // Act
        var result = TestFirstError.ToResult<string>();

        // Assert
        result.Should().BeFailure().And.WithError(TestFirstError);
    }

    [Fact]
    public void GivenToResult_WhenInvokedWithResultErrorNone_ThenThrowArgumentException()
    {
        // Arrange

        // Act
        var action = () => ResultError.None.ToResult<string>();

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithParameterName("error")
            .WithMessage("Invalid error (Parameter 'error')");
    }

    [Fact]
    public void GivenSelect_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var number = Faker.Random.Int();
        var expectedString = number.ToString(CultureInfo.InvariantCulture);

        // Act
        var methodResult = number.ToResult()
            .Select(n => n.ToString(CultureInfo.InvariantCulture));
        var methodManyResult = number.ToResult()
            .SelectMany(i => ((decimal) i).ToResult(), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = from intResult in number.ToResult()
            from stringResult in intResult.ToString(CultureInfo.InvariantCulture).ToResult()
            select stringResult;

        // Assert
        methodResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().And.WithValue(expectedString);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().And.WithValue(expectedString);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().And.WithValue(expectedString);
    }

    [Fact]
    public void GivenSelect_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange

        // Act
        var methodResult = TestFirstError.ToResult<int>()
            .Select(n => n.ToString(CultureInfo.InvariantCulture));
        var methodManyResult = TestFirstError.ToResult<int>()
            .SelectMany(i => ((decimal) i).ToResult(), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = from intResult in TestFirstError.ToResult<int>()
            from stringResult in intResult.ToString(CultureInfo.InvariantCulture).ToResult()
            select stringResult;

        // Assert
        methodResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(TestFirstError);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(TestFirstError);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(TestFirstError);
    }
}
