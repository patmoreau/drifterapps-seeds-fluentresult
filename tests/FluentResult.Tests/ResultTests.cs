namespace FluentResult.Tests;

[UnitTest]
public class ResultTests
{
    private static readonly Faker Faker = new();
    private static readonly ResultError TestFirstError = new("TestFirstError", "Test Error");
    private static readonly ResultError TestSecondError = new("TestSecondError", "Test Error");

    [Fact]
    public void GivenValue_WhenSuccess_ThenGetGoodValue()
    {
        // Arrange

        // Act
        var result = Nothing.Value.ToResult();

        // Assert
        result.Value.Should().Be(Nothing.Value);
    }

    [Fact]
    public void GivenValue_WhenFailure_ThenThrowException()
    {
        // Arrange
        var error = ResultTests.CreateError();
        var result = error.ToResult<string>();

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Cannot access the value of a failed result.");
    }

    [Fact]
    public void GivenImplicitOperator_WhenValue_ThenReturnSuccess()
    {
        // Arrange
        var value = Faker.Random.Hash();

        // Act
        Result<string> result = value;

        // Assert
        result.Should().BeSuccessful().And.WithValue(value);
    }

    [Fact]
    public void GivenImplicitOperator_WhenError_ThenReturnFailure()
    {
        // Arrange
        var error = new ResultError(Faker.Random.Hash(), Faker.Lorem.Sentence());

        // Act
        Result<Nothing> result = error;

        // Assert
        result.Should().BeFailure().And.WithError(error);
    }

    [Fact]
    public void GivenImplicitOperator_WhenResultValue_ThenReturnSuccess()
    {
        // Arrange
        var result = Faker.Random.Hash().ToResult();

        // Act
        string value = result;

        // Assert
        value.Should().BeOfType<string>().And.Be(result.Value);
    }

    [Fact]
    public void GivenImplicitOperator_WhenResultError_ThenReturnFailure()
    {
        // Arrange
        var result = new ResultError(Faker.Random.Hash(), Faker.Lorem.Sentence()).ToResult<string>();

        // Act
        ResultError error = result;

        // Assert
        error.Should().BeOfType<ResultError>().And.BeEquivalentTo(result.Error);
    }

    [Fact]
    public void GivenToResult_WhenSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expected = Nothing.Value;

        // Act
        var result = expected.ToResult();

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public void GivenToResult_WhenFailure_ThenReturnFailure()
    {
        // Arrange
        var error = new ResultError(Faker.Random.Hash(), Faker.Lorem.Sentence());
        var expected = error.ToResult<Nothing>();

        // Act
        var result = expected.ToResult();

        // Assert
        result.Should().BeFailure().And.WithError(error);
    }

    [Fact]
    public async Task GivenToTask_WhenSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expected = Nothing.Value.ToResult();

        // Act
        var result = await expected.ToTask();

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenToTask_WhenFailure_ThenReturnFailure()
    {
        // Arrange
        var error = new ResultError(Faker.Random.Hash(), Faker.Lorem.Sentence()).ToResult<Nothing>();

        // Act
        var result = await error.ToTask();

        // Assert
        result.Should().BeFailure();
    }

    [Fact]
    public void GivenFromResult_WhenSuccess_ThenReturnValue()
    {
        // Arrange
        var expected = Nothing.Value.ToResult();

        // Act
        var result = expected.FromResult();

        // Assert
        result.Should().Be(Nothing.Value);
    }

    public static TheoryData<Result<int>, bool, bool> MatchOfVoidData =>
        new()
        {
            {Faker.Random.Int().ToResult(), true, false},
            {TestFirstError.ToResult<int>(), false, true}
        };

    [Theory]
    [MemberData(nameof(MatchOfVoidData))]
    public void GivenMatchOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn, bool expectedSuccess,
        bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        resultIn.Match(_ => { calledSuccess = true; }, _ => { calledFailure = true; });

        // Assert
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    public static TheoryData<Result<int>, bool, bool> MatchOfTOutData =>
        new()
        {
            {Faker.Random.Int().ToResult(), true, false},
            {TestFirstError.ToResult<int>(), false, true}
        };

    [Theory]
    [MemberData(nameof(MatchOfTOutData))]
    public void GivenMatchOfTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange

        // Act
        var result = resultIn.Match(_ => Faker.Random.Word().ToResult(), _ => TestSecondError.ToResult<string>());

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
    }

    public static TheoryData<Result<int>, bool, bool> MatchOfTaskData =>
        new()
        {
            {Faker.Random.Int().ToResult(), true, false},
            {TestFirstError.ToResult<int>(), false, true}
        };

    [Theory]
    [MemberData(nameof(MatchOfTaskData))]
    public async Task GivenMatchOfTask_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        await resultIn.Match(_ =>
        {
            calledSuccess = true;
            return Task.CompletedTask;
        }, _ =>
        {
            calledFailure = true;
            return TestSecondError.ToResult<string>();
        });

        // Assert
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    public static TheoryData<Result<int>, bool, bool> MatchOfTaskOfTOutData =>
        new()
        {
            {Faker.Random.Int().ToResult(), true, false},
            {TestFirstError.ToResult<int>(), false, true}
        };

    [Theory]
    [MemberData(nameof(MatchOfTaskOfTOutData))]
    public async Task GivenMatchOfTaskOfTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange

        // Act
        var result = await resultIn.Match(_ => Task.FromResult(Faker.Random.Word().ToResult()), _ => Task.FromResult(TestSecondError.ToResult<string>()));

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
    }

    public static TheoryData<Result<int>, bool> OnSuccessData =>
        new()
        {
            {Faker.Random.Int().ToResult(), true},
            {TestFirstError.ToResult<int>(), false}
        };

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public void GivenOnSuccessOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn, bool expectedSuccess)
    {
        // Arrange
        var calledSuccess = false;

        // Act
        resultIn.OnSuccess(_ => { calledSuccess = true; });

        // Assert
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public void GivenOnSuccessOfTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn, bool expectedSuccess)
    {
        // Arrange

        // Act
        var result = resultIn.OnSuccess(_ => Faker.Random.Word().ToResult());

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.IsSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public async Task GivenOnSuccessOfTask_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn, bool expectedSuccess)
    {
        // Arrange
        var calledSuccess = false;

        // Act
        await resultIn.OnSuccess(_ =>
        {
            calledSuccess = true;
            return Task.CompletedTask;
        });

        // Assert
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public async Task GivenOnSuccessOfTaskOfTOut_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn, bool expectedSuccess)
    {
        // Arrange
        var calledSuccess = false;

        // Act
        var result = await resultIn.OnSuccess(_ =>
        {
            calledSuccess = true;
            return Task.FromResult(Faker.Random.Word().ToResult());
        });

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        calledSuccess.Should().Be(expectedSuccess);
    }

    public static TheoryData<Result<int>, bool> OnFailureData =>
        new()
        {
            {Faker.Random.Int().ToResult(), false},
            {TestFirstError.ToResult<int>(), true}
        };

    [Theory]
    [MemberData(nameof(OnFailureData))]
    public void GivenOnFailureOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn, bool expectedFailure)
    {
        // Arrange
        var calledFailure = false;

        // Act
        var result = resultIn.OnFailure(_ => { calledFailure = true; });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsFailure.Should().Be(expectedFailure);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(OnFailureData))]
    public async Task GivenOnFailureOfTask_WhenInvoked_ThenAppropriateMethodIsCalled(Result<int> resultIn, bool expectedFailure)
    {
        // Arrange
        var calledFailure = false;

        // Act
        var result = await resultIn.OnFailure(_ =>
        {
            calledFailure = true;
            return Task.CompletedTask;
        });

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsFailure.Should().Be(expectedFailure);
        calledFailure.Should().Be(expectedFailure);
    }

    [Fact]
    public void GivenEquals_WhenSameResult_ThenReturnTrue()
    {
        // Arrange
        var result = Faker.Random.Hash().ToResult();

        // Act
        var isEqual = result.Equals(result);

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenSameValues_ThenReturnTrue()
    {
        // Arrange
        var hash = Faker.Random.Hash();
        var result = hash.ToResult();

        // Act
        var isEqual = result.Equals(hash.ToResult());

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenSameObject_ThenReturnTrue()
    {
        // Arrange
        var hash = Faker.Random.Hash();
        var result = hash.ToResult();

        // Act
        var isEqual = result.Equals((object) hash.ToResult());

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenDifferentObject_ThenReturnFalse()
    {
        // Arrange
        var result = Faker.Random.Hash().ToResult();

        // Act
        var isEqual = result.Equals((object) Faker.Random.Hash().ToResult());

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenEqualOperator_WhenSameValues_ThenReturnTrue()
    {
        // Arrange
        var hash = Faker.Random.Hash();
        var result = hash.ToResult();

        // Act
        var isEqual = result == hash;

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEqualOperator_WhenDifferentObject_ThenReturnFalse()
    {
        // Arrange
        var result = Faker.Random.Hash().ToResult();

        // Act
        var isEqual = result == Faker.Random.Hash();

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenNotEqualOperator_WhenDifferentValues_ThenReturnTrue()
    {
        // Arrange
        var result = Faker.Random.Hash().ToResult();

        // Act
        var isEqual = result != Faker.Random.Hash();

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenNotEqualOperator_WhenSameValues_ThenReturnFalse()
    {
        // Arrange
        var hash = Faker.Random.Hash();
        var result = hash.ToResult();

        // Act
        var isEqual = result != hash;

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenGetHashCode_WhenSameValues_ThenSameHash()
    {
        // Arrange
        var hash = Faker.Random.Hash();
        var result = hash.ToResult();

        // Act
        var isEqual = result.GetHashCode() == hash.ToResult().GetHashCode();

        // Assert
        isEqual.Should().BeTrue();
    }

    private static ResultError CreateError() => new(Faker.Random.Hash(), Faker.Lorem.Sentence());
}
