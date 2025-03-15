using System.Globalization;

namespace FluentResult.Tests;

public partial class ResultExtensionsTests
{
    public static TheoryData<Result<int>, bool, bool> MatchData =>
        new()
        {
            {Faker.Random.Int().ToResult(), true, false},
            {TestFirstError.ToResult<int>(), false, true}
        };

    public static TheoryData<Result<int>, bool> OnSuccessData =>
        new()
        {
            {Faker.Random.Int().ToResult(), true},
            {TestFirstError.ToResult<int>(), false}
        };

    public static TheoryData<Result<int>, bool> OnFailureData =>
        new()
        {
            {Faker.Random.Int().ToResult(), false},
            {TestFirstError.ToResult<int>(), true}
        };

    [Fact]
    public async Task GivenToResultAsync_WhenInvoked_ThenReturnSuccess()
    {
        // Arrange
        var number = Faker.Random.Int();
        var numberTask = Task.FromResult(number);

        // Act
        var result = await numberTask.ToResult();

        // Assert
        result.Should().BeSuccessful().And.WithValue(number);
    }

    [Fact]
    public void GivenToResultAsync_WhenInvokedForResultError_ThenThrowInvalidOperationException()
    {
        // Arrange
        var error = TestFirstError;
        var numberTask = Task.FromResult(error);

        // Act
        var action = () => numberTask.ToResult();

        // Assert
        action.Should()
            .ThrowAsync<InvalidOperationException>().WithMessage("ResultError is not allowed.");
    }

    [Fact]
    public async Task GivenToResultAsync_WhenInvokedWithResultError_ThenReturnFailure()
    {
        // Arrange
        var numberTask = Task.FromResult(TestFirstError);

        // Act
        var result = await numberTask.ToResult<string>();

        // Assert
        result.Should().BeFailure().And.WithError(TestFirstError);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public async Task GivenAsyncMatchOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = await Task.FromResult(resultIn).Match(_ => { calledSuccess = true; },
            _ => { calledFailure = true; });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public async Task GivenAsyncMatchOfTInTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = await Task.FromResult(resultIn).Match(
            _ =>
            {
                calledSuccess = true;
                return Nothing.Value.ToResult();
            },
            error =>
            {
                calledFailure = true;
                return error.ToResult<Nothing>();
            });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public async Task GivenAsyncMatchOfTInWithNoTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = await Task.FromResult(resultIn).Match(
            _ =>
            {
                calledSuccess = true;
                return Task.CompletedTask;
            },
            _ =>
            {
                calledFailure = true;
                return Task.CompletedTask;
            });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public async Task GivenAsyncMatchOfTInWithTaskTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = await Task.FromResult(resultIn).Match(
            _ =>
            {
                calledSuccess = true;
                return Nothing.Task.ToResult();
            },
            error =>
            {
                calledFailure = true;
                return error.ToResult<Nothing>().ToTask();
            });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public async Task GivenAsyncOnSuccessOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess)
    {
        // Arrange
        var calledSuccess = false;

        // Act
        var result = await Task.FromResult(resultIn).OnSuccess(
            _ => { calledSuccess = true; });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public async Task GivenAsyncOnSuccessOfTInTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess)
    {
        // Arrange
        var calledSuccess = false;

        // Act
        var result = await Task.FromResult(resultIn).OnSuccess(
            _ =>
            {
                calledSuccess = true;
                return Nothing.Value.ToResult();
            });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public async Task GivenAsyncOnSuccessOfTInWithNoTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess)
    {
        // Arrange
        var calledSuccess = false;

        // Act
        var result = await Task.FromResult(resultIn).OnSuccess(
            _ =>
            {
                calledSuccess = true;
                return Task.CompletedTask;
            });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public async Task GivenAsyncOnSuccessOfTInTaskTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess)
    {
        // Arrange
        var calledSuccess = false;

        // Act
        var result = await Task.FromResult(resultIn).OnSuccess(
            _ =>
            {
                calledSuccess = true;
                return Nothing.Task.ToResult();
            });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnFailureData))]
    public async Task GivenAsyncOnFailureVoid_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedfailure)
    {
        // Arrange
        var calledFailure = false;

        // Act
        var result = await Task.FromResult(resultIn).OnFailure(
            _ => { calledFailure = true; });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsFailure.Should().Be(expectedfailure);
        calledFailure.Should().Be(expectedfailure);
    }

    [Theory]
    [MemberData(nameof(OnFailureData))]
    public async Task GivenAsyncOnFailureOfTask_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedfailure)
    {
        // Arrange
        var calledFailure = false;

        // Act
        var result = await Task.FromResult(resultIn).OnFailure(
            _ =>
            {
                calledFailure = true;
                return Task.CompletedTask;
            });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsFailure.Should().Be(expectedfailure);
        calledFailure.Should().Be(expectedfailure);
    }

    [Fact]
    public async Task GivenAsyncSelect_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var number = Faker.Random.Int();
        var expectedString = number.ToString(CultureInfo.InvariantCulture);

        // Act
        var methodResult = await Task.FromResult(number.ToResult())
            .Select(n => Task.FromResult(n.ToString(CultureInfo.InvariantCulture)));
        var methodManyResult = await Task.FromResult(number.ToResult())
            .SelectMany(i => Task.FromResult(((decimal) i).ToResult()),
                (_, d) => d.ToString(CultureInfo.InvariantCulture));
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
        var methodResult = await Task.FromResult(TestFirstError.ToResult<int>())
            .Select(n => Task.FromResult(n.ToString(CultureInfo.InvariantCulture)));
        var methodManyResult = await Task.FromResult(TestFirstError.ToResult<int>())
            .SelectMany(i => Task.FromResult(((decimal) i).ToResult()),
                (_, d) => d.ToString(CultureInfo.InvariantCulture));
        var queryResult = await (from intResult in Task.FromResult(TestFirstError.ToResult<int>())
            from stringResult in Task.FromResult(intResult.ToString(CultureInfo.InvariantCulture).ToResult())
            select stringResult);

        // Assert
        methodResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(TestFirstError);
        methodManyResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(TestFirstError);
        queryResult.Should().BeOfType<Result<string>>()
            .And.BeFailure().And.WithError(TestFirstError);
    }
}
