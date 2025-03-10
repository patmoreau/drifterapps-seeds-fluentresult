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
    public async Task GivenToResultAsync_WhenInvokedWithResultError_ThenReturnFailure()
    {
        // Arrange
        var numberTask = Task.FromResult(_testFirstError);

        // Act
        var result = await numberTask.ToResult<string>();

        // Assert
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenAsyncOnSuccessOfTInTOut_WhenResultIsSuccess_ThenExecuteNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Task.FromResult(_faker.Random.Word().ToResult());

        // Act
        var result = await resultIn.OnSuccess(value => Task.FromResult(expectedValue.ToResult()));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful().And.WithValue(expectedValue);
    }

    [Fact]
    public async Task GivenAsyncOnSuccessOfTInTOut_WhenResultIsFailure_ThenReturnFailure()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Task.FromResult(_testFirstError.ToResult<int>());

        // Act
        var result = await resultIn.OnSuccess(value => Task.FromResult(expectedValue.ToResult()));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenAsyncOnFailureOfTIn_WhenResultIsSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = Task.FromResult(expectedValue.ToResult());

        // Act
        var result = await resultIn.OnFailure(_ => Task.FromResult(_testSecondError.ToResult<string>()));

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenAsyncOnFailureOfTIn_WhenResultIsFailure_ThenExecuteNext()
    {
        // Arrange
        var resultIn = Task.FromResult(_testFirstError.ToResult<string>());
        var expectedError = ResultError.None;

        // Act
        var result = await resultIn.OnFailure(error =>
        {
            expectedError = error;
            return Task.FromResult(_testSecondError.ToResult<string>());
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
        var resultIn = Task.FromResult(expectedValue.ToResult());

        // Act
        var result = await resultIn.Switch(_ => Task.FromResult(_faker.Random.Int().ToResult()),
            error => Task.FromResult(error.ToResult<int>()));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenAsyncSwitchOfTInTOut_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange
        var resultIn = Task.FromResult(_testFirstError.ToResult<string>());

        // Act
        var result = await resultIn.Switch(_ => Task.FromResult(_faker.Random.Int().ToResult()),
            error => Task.FromResult(error.ToResult<int>()));

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
        var methodResult = await Task.FromResult(number.ToResult())
            .Select(n => Task.FromResult(n.ToString(CultureInfo.InvariantCulture)));
        var methodManyResult = await Task.FromResult(number.ToResult())
            .SelectMany(i => Task.FromResult(((decimal)i).ToResult()), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = await (from intResult in Task.FromResult(number.ToResult())
            from stringResult in Task.FromResult(intResult.ToString(CultureInfo.InvariantCulture).ToResult())
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
        var methodResult = await Task.FromResult(_testFirstError.ToResult<int>())
            .Select(n => Task.FromResult(n.ToString(CultureInfo.InvariantCulture)));
        var methodManyResult = await Task.FromResult(_testFirstError.ToResult<int>())
            .SelectMany(i => Task.FromResult(((decimal)i).ToResult()), (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = await (from intResult in Task.FromResult(_testFirstError.ToResult<int>())
            from stringResult in Task.FromResult(intResult.ToString(CultureInfo.InvariantCulture).ToResult())
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
