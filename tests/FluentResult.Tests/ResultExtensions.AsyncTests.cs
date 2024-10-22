using System.Globalization;

namespace FluentResult.Tests;

public partial class ResultExtensionsTests
{

    [Fact]
    public async Task GivenToResultAsync_WhenInvoked_ThenReturnSuccess()
    {
        // Arrange
        var number = _faker.Random.Int();
        var numberTask = Task.FromResult(number);

        // Act
        var result = await numberTask.ToResult();

        // Assert
        result.Should().BeSuccessful().And.WithValue(number);
    }

    [Fact]
    public async Task GivenAsyncOnSuccessOfTInTOut_WhenResultIsSuccess_ThenExecuteNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Task.FromResult(Result<string>.Success(_faker.Random.Word()));

        // Act
        var result = await resultIn.OnSuccess(value => Task.FromResult(Result<int>.Success(expectedValue)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful().And.WithValue(expectedValue);
    }

    [Fact]
    public async Task GivenAsyncOnSuccessOfTInTOut_WhenResultIsFailure_ThenReturnFailure()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Task.FromResult(Result<string>.Failure(_testFirstError));

        // Act
        var result = await resultIn.OnSuccess(value => Task.FromResult(Result<int>.Success(expectedValue)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenAsyncOnFailureOfTIn_WhenResultIsSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = Task.FromResult(Result<string>.Success(expectedValue));

        // Act
        var result = await resultIn.OnFailure(_ => Task.FromResult(Result<string>.Failure(_testSecondError)));

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenAsyncOnFailureOfTIn_WhenResultIsFailure_ThenExecuteNext()
    {
        // Arrange
        var resultIn = Task.FromResult(Result<string>.Failure(_testFirstError));
        var expectedError = ResultError.None;

        // Act
        var result = await resultIn.OnFailure(error =>
        {
            expectedError = error;
            return Task.FromResult(Result<string>.Failure(_testSecondError));
        });

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeFailure().And.WithError(_testSecondError);
        expectedError.Should().Be(_testFirstError);
    }

    [Fact]
    public async Task GivenAsyncSwitchOfTInTOut_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = Task.FromResult(Result<string>.Success(expectedValue));

        // Act
        var result = await resultIn.Switch(_ => Task.FromResult(Result<int>.Success(_faker.Random.Int())),
            error => Task.FromResult(Result<int>.Failure(error)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenAsyncSwitchOfTInTOut_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange
        var resultIn = Task.FromResult(Result<string>.Failure(_testFirstError));

        // Act
        var result = await resultIn.Switch(_ => Task.FromResult(Result<int>.Success(_faker.Random.Int())),
            error => Task.FromResult(Result<int>.Failure(error)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenAsyncSelect_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var number = _faker.Random.Int();
        var expectedString = number.ToString(CultureInfo.InvariantCulture);

        // Act
        var methodResult = await Task.FromResult(Result<int>.Success(number))
            .Select(n => Task.FromResult(n.ToString(CultureInfo.InvariantCulture)));
        var methodManyResult = await Task.FromResult(Result<int>.Success(number))
            .SelectMany(i => Task.FromResult(Result<decimal>.Success(i)), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = await (from intResult in Task.FromResult(Result<int>.Success(number))
            from stringResult in Task.FromResult(Result<string>.Success(intResult.ToString(CultureInfo.InvariantCulture)))
            select stringResult);

        // Assert
        methodResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().And.WithValue(expectedString);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().And.WithValue(expectedString);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeSuccessful().And.WithValue(expectedString);
    }

    [Fact]
    public async Task GivenAsyncSelect_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange

        // Act
        var methodResult = await Task.FromResult(Result<int>.Failure(_testFirstError))
            .Select(n => Task.FromResult(n.ToString(CultureInfo.InvariantCulture)));
        var methodManyResult = await Task.FromResult(Result<int>.Failure(_testFirstError))
            .SelectMany(i => Task.FromResult(Result<decimal>.Success(i)), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = await (from intResult in Task.FromResult(Result<int>.Failure(_testFirstError))
            from stringResult in Task.FromResult(Result<string>.Success(intResult.ToString(CultureInfo.InvariantCulture)))
            select stringResult);

        // Assert
        methodResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(_testFirstError);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(_testFirstError);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(_testFirstError);
    }
}
