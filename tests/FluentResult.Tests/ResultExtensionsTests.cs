using System.Globalization;

namespace FluentResult.Tests;

[UnitTest]
public partial class ResultExtensionsTests
{
    private readonly Faker _faker = new();
    private readonly ResultError _testFirstError = new("TestFirstError", "Test Error");
    private readonly ResultError _testSecondError = new("TestSecondError", "Test Error");

    [Fact]
    public void GivenToResult_WhenInvoked_ThenReturnSuccess()
    {
        // Arrange
        var number = _faker.Random.Int();

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
    public void GivenToResult_WhenInvokedWithResultError_ThenReturnFailure()
    {
        // Arrange

        // Act
        var result = _testFirstError.ToResult<string>();

        // Assert
        result.Should().BeFailure().And.WithError(_testFirstError);
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
        var number = _faker.Random.Int();
        var expectedString = number.ToString(CultureInfo.InvariantCulture);

        // Act
        var methodResult = number.ToResult()
            .Select(n => n.ToString(CultureInfo.InvariantCulture));
        var methodManyResult = number.ToResult()
            .SelectMany(i => ((decimal)i).ToResult(), (_, d) => d.ToString(CultureInfo.InvariantCulture));
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
        var methodResult = _testFirstError.ToResult<int>()
            .Select(n => n.ToString(CultureInfo.InvariantCulture));
        var methodManyResult = _testFirstError.ToResult<int>()
            .SelectMany(i => ((decimal)i).ToResult(), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = from intResult in _testFirstError.ToResult<int>()
            from stringResult in intResult.ToString(CultureInfo.InvariantCulture).ToResult()
            select stringResult;

        // Assert
        methodResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(_testFirstError);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(_testFirstError);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(_testFirstError);
    }
}
