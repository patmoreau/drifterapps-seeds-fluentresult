using System.Globalization;

namespace FluentResult.Tests;

[UnitTest]
public partial class ResultExtensionsTests
{
    private readonly Faker _faker = new();
    private readonly ResultError _testFirstError = new("TestFirstError", "Test Error");
    private readonly ResultError _testSecondError = new("TestSecondError", "Test Error");

    [Fact]
    public void GivenOnSuccessOfTInTOut_WhenResultIsSuccess_ThenExecuteNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Result<string>.Success(_faker.Random.Word());

        // Act
        var result = resultIn.OnSuccess(_ => Result<int>.Success(expectedValue));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful().WithValue(expectedValue);
    }

    [Fact]
    public void GivenOnSuccessOfTInTOut_WhenResultIsFailure_ThenReturnFailure()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Result<string>.Failure(_testFirstError);

        // Act
        var result = resultIn.OnSuccess(_ => Result<int>.Success(expectedValue));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().WithError(_testFirstError);
    }

    [Fact]
    public void GivenOnFailureOfTIn_WhenResultIsSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = Result<string>.Success(expectedValue);

        // Act
        var result = resultIn.OnFailure(_ => Result<string>.Failure(_testSecondError));

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public void GivenOnFailureOfTIn_WhenResultIsFailure_ThenExecuteNext()
    {
        // Arrange
        var resultIn = Result<string>.Failure(_testFirstError);
        var expectedError = ResultError.None;

        // Act
        var result = resultIn.OnFailure(error =>
        {
            expectedError = error;
            return Result<string>.Failure(_testSecondError);
        });

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeFailure().WithError(_testSecondError);
        expectedError.Should().Be(_testFirstError);
    }

    [Fact]
    public void GivenSwitchOfTInTOut_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = Result<string>.Success(expectedValue);

        // Act
        var result = resultIn.Switch(_ => Result<int>.Success(_faker.Random.Int()), Result<int>.Failure);

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public void GivenSwitchOfTInTOut_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange
        var resultIn = Result<string>.Failure(_testFirstError);

        // Act
        var result = resultIn.Switch(_ => Result<int>.Success(_faker.Random.Int()), Result<int>.Failure);

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().WithError(_testFirstError);
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
            .And.BeSuccessful().WithValue(expectedString);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().WithValue(expectedString);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().WithValue(expectedString);
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
            .And.BeFailure().WithError(_testFirstError);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().WithError(_testFirstError);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().WithError(_testFirstError);
    }

    [Fact]
    public void GivenEnsure_WhenIgnoreOnFailure_ThenReturn()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();
        aggregate.AddResult(Result<Nothing>.Failure(_testFirstError));

        // Act
        var result = aggregate.Ensure(() => true, _testSecondError, EnsureOnFailure.IgnoreOnFailure);

        //Assert
        result.Results.Should().ContainSingle();
    }

    [Fact]
    public void GivenEnsure_WhenValidateOnFailure_ThenPerformValidation()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();
        aggregate.AddResult(Result<Nothing>.Failure(_testFirstError));

        // Act
        var result = aggregate.Ensure(() => true, _testSecondError, EnsureOnFailure.ValidateOnFailure);

        //Assert
        result.Results.Where(x => x.IsFailure).Should().ContainSingle();
        result.Results.Where(x => x.IsSuccess).Should().ContainSingle();
    }

    [Fact]
    public void GivenEnsure_WhenSuccessAndFailure_ThenPerformBothValidations()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();

        // Act
        var result = aggregate
            .Ensure(() => true, _testFirstError)
            .Ensure(() => false, _testSecondError);

        //Assert
        result.Results.Where(x => x.IsFailure).Should().ContainSingle();
        result.Results.Where(x => x.IsSuccess).Should().ContainSingle();
    }
}
