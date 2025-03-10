namespace FluentResult.Tests;

[UnitTest]
public class ResultTests
{
    private readonly Faker _faker = new();
    private readonly ResultError _testFirstError = new("TestFirstError", "Test Error");
    private readonly ResultError _testSecondError = new("TestSecondError", "Test Error");

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
        var error = CreateError();
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
        var expected = Nothing.Value;

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
        var expected = error.ToResult<Nothing>();

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
        Result<string> resultIn = _faker.Random.Word();

        // Act
        var result = resultIn.OnSuccess(_ => (Result<int>)expectedValue);

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful().And.WithValue(expectedValue);
    }

    [Fact]
    public void GivenOnSuccessOfTOut_WhenResultIsFailure_ThenReturnFailure()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = _testFirstError.ToResult<string>();

        // Act
        var result = resultIn.OnSuccess(_ => expectedValue.ToResult());

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenOnSuccessOfTOutAsync_WhenResultIsSuccess_ThenExecuteNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = _faker.Random.Word().ToResult();

        // Act
        var result = await resultIn.OnSuccess(_ => Task.FromResult(expectedValue.ToResult()));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful().And.WithValue(expectedValue);
    }

    [Fact]
    public async Task GivenOnSuccessOfTOutAsync_WhenResultIsFailure_ThenReturnFailure()
    {
        // Arrange
        var expectedValue = _faker.Random.Int();
        var resultIn = _testFirstError.ToResult<string>();

        // Act
        var result = await resultIn.OnSuccess(_ => Task.FromResult(expectedValue.ToResult()));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public void GivenOnFailure_WhenResultIsSuccess_ThenReturnSuccess()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = expectedValue.ToResult();

        // Act
        var result = resultIn.OnFailure(_ => _testSecondError.ToResult<string>());

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public void GivenOnFailure_WhenResultIsFailure_ThenExecuteNext()
    {
        // Arrange
        var resultIn = _testFirstError.ToResult<string>();
        var expectedError = ResultError.None;

        // Act
        var result = resultIn.OnFailure(error =>
        {
            expectedError = error;
            return _testSecondError.ToResult<string>();
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
        var resultIn = expectedValue.ToResult();

        // Act
        var result = await resultIn.OnFailure(_ => Task.FromResult(_testSecondError.ToResult<string>()));

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenOnFailureAsync_WhenResultIsFailure_ThenExecuteNext()
    {
        // Arrange
        var resultIn = _testFirstError.ToResult<string>();
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
    public void GivenSwitchOfTOut_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = expectedValue.ToResult();

        // Act
        var result = resultIn.Switch(_ => _faker.Random.Int(), e => e.ToResult<int>());

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public void GivenSwitchOfTOut_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange
        var resultIn = _testFirstError.ToResult<int>();

        // Act
        var result = resultIn.Switch(_ => _faker.Random.Int(), e => e.ToResult<int>());

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public async Task GivenSwitchOfTOutAsync_WhenResultIsSuccess_ThenExecuteSuccessNext()
    {
        // Arrange
        var expectedValue = _faker.Random.Word();
        var resultIn = expectedValue.ToResult();

        // Act
        var result = await resultIn.Switch(
            _ => Task.FromResult(_faker.Random.Int().ToResult()),
            e => Task.FromResult(e.ToResult<int>()));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenSwitchOfTOutAsync_WhenResultIsFailure_ThenExecuteFailureNext()
    {
        // Arrange
        var resultIn = _testFirstError.ToResult<string>();

        // Act
        var result = await resultIn.Switch(
            _ => Task.FromResult(_faker.Random.Int().ToResult()),
            e => Task.FromResult(e.ToResult<int>()));

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    [Fact]
    public void GivenEquals_WhenSameResult_ThenReturnTrue()
    {
        // Arrange
        var result = _faker.Random.Hash().ToResult();

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
        var hash = _faker.Random.Hash();
        var result = hash.ToResult();

        // Act
        var isEqual = result.Equals((object)hash.ToResult());

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenDifferentObject_ThenReturnFalse()
    {
        // Arrange
        var result = _faker.Random.Hash().ToResult();

        // Act
        var isEqual = result.Equals((object)_faker.Random.Hash().ToResult());

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenEqualOperator_WhenSameValues_ThenReturnTrue()
    {
        // Arrange
        var hash = _faker.Random.Hash();
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
        var result = _faker.Random.Hash().ToResult();

        // Act
        var isEqual = result == _faker.Random.Hash();

        // Assert
        isEqual.Should().BeFalse();
    }

    [Fact]
    public void GivenNotEqualOperator_WhenDifferentValues_ThenReturnTrue()
    {
        // Arrange
        var result = _faker.Random.Hash().ToResult();

        // Act
        var isEqual = result != _faker.Random.Hash();

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenNotEqualOperator_WhenSameValues_ThenReturnFalse()
    {
        // Arrange
        var hash = _faker.Random.Hash();
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
        var hash = _faker.Random.Hash();
        var result = hash.ToResult();

        // Act
        var isEqual = result.GetHashCode() == hash.ToResult().GetHashCode();

        // Assert
        isEqual.Should().BeTrue();
    }

    private ResultError CreateError() => new(_faker.Random.Hash(), _faker.Lorem.Sentence());
}
