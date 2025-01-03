namespace FluentResult.Tests;

[UnitTest]
public class ResultTests
{
    private readonly Faker _faker = new();
    private readonly ResultError _testFirstError = new("TestFirstError", "Test Error");
    private readonly ResultError _testSecondError = new("TestSecondError", "Test Error");

    [Fact]
    public void GivenSuccess_WhenCreated_ThenIsSuccessIsTrue()
    {
        // Arrange

        // Act
        var result = Result<Nothing>.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(ResultError.None);
    }

    [Fact]
    public void GivenSuccess_WhenCreatedWithValue_ThenIsSuccessIsTrue()
    {
        // Arrange
        var value = _faker.Random.Hash();

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(ResultError.None);
    }

    [Fact]
    public void GivenSuccess_WhenCreatedWithNullValue_ThenThrowException()
    {
        // Arrange

        // Act
        var act = () => Result<string>.Success(null!);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("value")
            .WithMessage("Value cannot be null. (Parameter 'value')");
    }

    [Fact]
    public void GivenFailure_WhenErrorIsNotNone_ThenIsFailureIsTrue()
    {
        // Arrange
        var error = CreateError();

        // Act
        var result = Result<Nothing>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GivenFailure_WhenErrorIsNone_ThenThrowException()
    {
        // Arrange

        // Act
        var action = () => Result<Nothing>.Failure(ResultError.None);

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("Invalid error (Parameter 'error')");
    }

    [Fact]
    public void GivenValue_WhenSuccess_ThenGetGoodValue()
    {
        // Arrange

        // Act
        var result = Result<Nothing>.Success();

        // Assert
        result.Value.Should().Be(Nothing.Value);
    }

    [Fact]
    public void GivenValue_WhenFailure_ThenThrowException()
    {
        // Arrange
        var error = CreateError();
        var result = Result<string>.Failure(error);

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Cannot access the value of a failed result.");
    }

    [Fact]
    public void GivenImplicitOperator_WhenValue_ThenReturnSuccess()
    {
        // Arrange
        var value = _faker.Random.Hash();

        // Act
        Result<string> result = value;

        // Assert
        result.Should().BeSuccessful().And.WithValue(value);
    }

    [Fact]
    public void GivenImplicitOperator_WhenError_ThenReturnFailure()
    {
        // Arrange
        var error = new ResultError(_faker.Random.Hash(), _faker.Lorem.Sentence());

        // Act
        Result<Nothing> result = error;

        // Assert
        result.Should().BeFailure().And.WithError(error);
    }

    [Fact]
    public void GivenToResult_WhenSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expected = Result<Nothing>.Success();

        // Act
        var result = expected.ToResult();

        // Assert
        result.Should().BeSuccessful().And.WithValue(Nothing.Value);
    }

    [Fact]
    public void GivenToResult_WhenFailure_ThenReturnFailure()
    {
        // Arrange
        var error = new ResultError(_faker.Random.Hash(), _faker.Lorem.Sentence());
        var expected = Result<Nothing>.Failure(error);

        // Act
        var result = expected.ToResult();

        // Assert
        result.Should().BeFailure().And.WithError(error);
    }

    [Fact]
    public void GivenOnSuccessOfTOut_WhenResultIsSuccess_ThenExecuteNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Result<string>.Success(_faker.Random.Word());

        // Act
        var result = resultIn.OnSuccess(_ => Result<int>.Success(expectedValue));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful().And.WithValue(expectedValue);
    }

    [Fact]
    public void GivenOnSuccessOfTOut_WhenResultIsFailure_ThenReturnFailure()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Result<string>.Failure(_testFirstError);

        // Act
        var result = resultIn.OnSuccess(_ => Result<int>.Success(expectedValue));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenOnSuccessOfTOutAsync_WhenResultIsSuccess_ThenExecuteNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Result<string>.Success(_faker.Random.Word());

        // Act
        var result = await resultIn.OnSuccess(_ => Task.FromResult(Result<int>.Success(expectedValue)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful().And.WithValue(expectedValue);
    }

    [Fact]
    public async Task GivenOnSuccessOfTOutAsync_WhenResultIsFailure_ThenReturnFailure()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = Result<string>.Failure(_testFirstError);

        // Act
        var result = await resultIn.OnSuccess(_ => Task.FromResult(Result<int>.Success(expectedValue)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public void GivenOnFailure_WhenResultIsSuccess_ThenReturnSuccess()
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
    public void GivenOnFailure_WhenResultIsFailure_ThenExecuteNext()
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
        result.Should().BeFailure().And.WithError(_testSecondError);
        expectedError.Should().Be(_testFirstError);
    }

    [Fact]
    public async Task GivenOnFailureAsync_WhenResultIsSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = Result<string>.Success(expectedValue);

        // Act
        var result = await resultIn.OnFailure(_ => Task.FromResult(Result<string>.Failure(_testSecondError)));

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenOnFailureAsync_WhenResultIsFailure_ThenExecuteNext()
    {
        // Arrange
        var resultIn = Result<string>.Failure(_testFirstError);
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
    public void GivenSwitchOfTOut_WhenResultIsSuccess_ThenExecuteSuccessNext()
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
    public void GivenSwitchOfTOut_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange
        var resultIn = Result<string>.Failure(_testFirstError);

        // Act
        var result = resultIn.Switch(_ => Result<int>.Success(_faker.Random.Int()), Result<int>.Failure);

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenSwitchOfTOutAsync_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = Result<string>.Success(expectedValue);

        // Act
        var result = await resultIn.Switch(
            _ => Task.FromResult(Result<int>.Success(_faker.Random.Int())),
            e => Task.FromResult(Result<int>.Failure(e)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenSwitchOfTOutAsync_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange
        var resultIn = Result<string>.Failure(_testFirstError);

        // Act
        var result = await resultIn.Switch(
            _ => Task.FromResult(Result<int>.Success(_faker.Random.Int())),
            e => Task.FromResult(Result<int>.Failure(e)));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public void GivenEquals_WhenSameResult_ThenReturnTrue()
    {
        // Arrange
        var result = Result<string>.Success(_faker.Random.Hash());

        // Act
        var isEqual = result.Equals(result);

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenSameValues_ThenReturnTrue()
    {
        // Arrange
        var hash = _faker.Random.Hash();
        var result = Result<string>.Success(hash);

        // Act
        var isEqual = result.Equals(Result<string>.Success(hash));

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenSameObject_ThenReturnTrue()
    {
        // Arrange
        var hash = _faker.Random.Hash();
        var result = Result<string>.Success(hash);

        // Act
        var isEqual = result.Equals((object)Result<string>.Success(hash));

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenDifferentObject_ThenReturnFalse()
    {
        // Arrange
        var result = Result<string>.Success(_faker.Random.Hash());

        // Act
        var isEqual = result.Equals((object)Result<string>.Success(_faker.Random.Hash()));

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenEqualOperator_WhenSameValues_ThenReturnTrue()
    {
        // Arrange
        var hash = _faker.Random.Hash();
        var result = Result<string>.Success(hash);

        // Act
        var isEqual = result == Result<string>.Success(hash);

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEqualOperator_WhenDifferentObject_ThenReturnFalse()
    {
        // Arrange
        var result = Result<string>.Success(_faker.Random.Hash());

        // Act
        var isEqual = result == Result<string>.Success(_faker.Random.Hash());

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenNotEqualOperator_WhenDifferentValues_ThenReturnTrue()
    {
        // Arrange
        var result = Result<string>.Success(_faker.Random.Hash());

        // Act
        var isEqual = result != Result<string>.Success(_faker.Random.Hash());

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenNotEqualOperator_WhenSameValues_ThenReturnFalse()
    {
        // Arrange
        var hash = _faker.Random.Hash();
        var result = Result<string>.Success(hash);

        // Act
        var isEqual = result != Result<string>.Success(hash);

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenGetHashCode_WhenSameValues_ThenSameHash()
    {
        // Arrange
        var hash = _faker.Random.Hash();
        var result = Result<string>.Success(hash);

        // Act
        var isEqual = result.GetHashCode() == Result<string>.Success(hash).GetHashCode();

        // Assert
        isEqual.Should().BeTrue();
    }

    private ResultError CreateError() => new(_faker.Random.Hash(), _faker.Lorem.Sentence());
}
