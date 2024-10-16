namespace FluentResult.Tests;

public partial class ResultExtensionsTests
{
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
        result.Should().BeSuccessful().WithValue(expectedValue);
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
        result.Should().BeFailure().WithError(_testFirstError);
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
        result.Should().BeFailure().WithError(_testSecondError);
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
        result.Should().BeFailure().WithError(_testFirstError);
    }
}
