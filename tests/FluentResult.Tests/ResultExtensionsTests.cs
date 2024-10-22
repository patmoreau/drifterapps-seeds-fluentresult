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
    public void GivenSelect_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var number = _faker.Random.Int();
        var expectedString = number.ToString(CultureInfo.InvariantCulture);

        // Act
        var methodResult = Result<int>.Success(number)
            .Select(n => n.ToString(CultureInfo.InvariantCulture));
        var methodManyResult = Result<int>.Success(number)
            .SelectMany(i => Result<decimal>.Success(i), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = from intResult in Result<int>.Success(number)
            from stringResult in Result<string>.Success(intResult.ToString(CultureInfo.InvariantCulture))
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
        var methodResult = Result<int>.Failure(_testFirstError)
            .Select(n => n.ToString(CultureInfo.InvariantCulture));
        var methodManyResult = Result<int>.Failure(_testFirstError)
            .SelectMany(i => Result<decimal>.Success(i), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = from intResult in Result<int>.Failure(_testFirstError)
            from stringResult in Result<string>.Success(intResult.ToString(CultureInfo.InvariantCulture))
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
