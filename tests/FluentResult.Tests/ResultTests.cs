using ArgumentException = System.ArgumentException;

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
    public void GivenOnSuccessOfTInTOut_WhenResultIsSuccess_ThenExecuteNext()
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
    public void GivenOnSuccessOfTInTOut_WhenResultIsFailure_ThenReturnFailure()
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
        result.Should().BeFailure().And.WithError(_testSecondError);
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
        result.Should().BeFailure().And.WithError(_testFirstError);
    }

    private ResultError CreateError() => new(_faker.Random.Hash(), _faker.Lorem.Sentence());
}
