// ReSharper disable RedundantArgumentDefaultValue

namespace FluentResult.Tests;

[UnitTest]
public class ResultAggregateTests
{
    private static readonly Faker Faker = new();
    private readonly ResultError _testFirstError = new(Faker.Random.Word(), Faker.Lorem.Sentence());
    private readonly ResultError _testSecondError = new(Faker.Random.Word(), Faker.Lorem.Sentence());

    [Fact]
    public void Create_WithNoResults_ReturnsEmptyAggregate()
    {
        // Act
        var resultAggregate = ResultAggregate.Create();

        // Assert
        resultAggregate.Results.Should().BeEmpty();
        resultAggregate.IsSuccess.Should().BeTrue();
        resultAggregate.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void AddResult_WithSuccessResult_UpdatesAggregate()
    {
        // Arrange
        var resultAggregate = ResultAggregate.Create();
        var successResult = Result<Nothing>.Success();

        // Act
        resultAggregate.AddResult(successResult);

        // Assert
        resultAggregate.Results.Should().ContainSingle().And.OnlyContain(x => x.Equals(successResult));
        resultAggregate.IsSuccess.Should().BeTrue();
        resultAggregate.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void AddResult_WithFailureResult_UpdatesAggregate()
    {
        // Arrange
        var resultAggregate = ResultAggregate.Create();
        var failureResult = Result<Nothing>.Failure(new ResultError(Faker.Random.Word(), Faker.Lorem.Sentence()));

        // Act
        resultAggregate.AddResult(failureResult);

        // Assert
        resultAggregate.Results.Should().ContainSingle().And.OnlyContain(x => x.Equals(failureResult));
        resultAggregate.IsSuccess.Should().BeFalse();
        resultAggregate.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddResult_WithMixedResults_UpdatesAggregate()
    {
        // Arrange
        var resultAggregate = ResultAggregate.Create();
        var successResult = Result<Nothing>.Success();
        var failureResult = Result<Nothing>.Failure(new ResultError(Faker.Random.Word(), Faker.Lorem.Sentence()));

        // Act
        resultAggregate.AddResult(successResult);
        resultAggregate.AddResult(failureResult);

        // Assert
        resultAggregate.Results.Should().HaveCount(2);
        resultAggregate.IsSuccess.Should().BeFalse();
        resultAggregate.IsFailure.Should().BeTrue();
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
    public void GivenEnsure_WhenResultIgnoreOnFailure_ThenReturn()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();
        aggregate.AddResult(Result<Nothing>.Failure(_testFirstError));

        // Act
        var result = aggregate.Ensure(SuccessfulValidation(), EnsureOnFailure.IgnoreOnFailure);

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
    public void GivenEnsure_WhenResultValidateOnFailure_ThenPerformValidation()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();
        aggregate.AddResult(Result<Nothing>.Failure(_testFirstError));

        // Act
        var result = aggregate.Ensure(SuccessfulValidation(), EnsureOnFailure.ValidateOnFailure);

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

    [Fact]
    public void GivenEnsure_WhenResultSuccessAndFailure_ThenPerformBothValidations()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();

        // Act
        var result = aggregate
            .Ensure(SuccessfulValidation())
            .Ensure(FailedValidation(_testSecondError));

        //Assert
        result.Results.Where(x => x.IsFailure).Should().ContainSingle();
        result.Results.Where(x => x.IsSuccess).Should().ContainSingle();
    }

    [Fact]
    public void GivenSwitch_WhenNoFailureResults_ThenReturnsSuccess()
    {
        // Arrange
        var resultAggregate = ResultAggregate.Create();

        // Act
        var result = resultAggregate.Switch(Result<Nothing>.Success, Result<Nothing>.Failure);

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public void GivenSwitch_WhenSingleFailureResult_ThenReturnsValidationError()
    {
        // Arrange
        var error = Faker.Random.Word();
        var description = Faker.Lorem.Sentence();
        var resultAggregate = ResultAggregate.Create();
        resultAggregate.AddResult(Result<Nothing>.Failure(new ResultError(error, description)));

        // Act
        var result = resultAggregate.Switch(Result<Nothing>.Success, Result<Nothing>.Failure);

        // Assert
        result.Should()
            .BeFailure()
            .And.Subject
            .Error.Should().BeOfType<ResultErrorAggregate>()
            .Which
            .Errors.Should().ContainKey(error)
            .And.ContainSingle(description);
    }

    [Fact]
    public void GivenSwitch_WhenMultipleFailureResults_ThenReturnsValidationError()
    {
        // Arrange
        var error1 = Faker.Random.Word();
        var error2 = Faker.Random.Word();
        var description1 = Faker.Lorem.Sentence();
        var description2 = Faker.Lorem.Sentence();
        var resultAggregate = ResultAggregate.Create();
        resultAggregate.AddResult(Result<Nothing>.Failure(new ResultError(error1, description1)));
        resultAggregate.AddResult(Result<Nothing>.Failure(new ResultError(error2, description2)));

        // Act
        var result = resultAggregate.Switch(Result<Nothing>.Success, Result<Nothing>.Failure);

        // Assert
        result.Should()
            .BeFailure()
            .And.Subject
            .Error.Should().BeOfType<ResultErrorAggregate>()
            .Which
            .Errors.Should().ContainKey(error1)
            .WhoseValue.Should().ContainSingle(description1);
        result.Should()
            .BeFailure()
            .And.Subject
            .Error.Should().BeOfType<ResultErrorAggregate>()
            .Which
            .Errors.Should().ContainKey(error2)
            .WhoseValue.Should().ContainSingle(description2);
    }

    [Fact]
    public void GivenSwitch_WhenDuplicateFailureResults_ThenReturnsValidationErrorWithAggregatedDescriptions()
    {
        // Arrange
        var error = Faker.Random.Word();
        var description1 = Faker.Lorem.Sentence();
        var description2 = Faker.Lorem.Sentence();
        var resultAggregate = ResultAggregate.Create();
        resultAggregate.AddResult(Result<Nothing>.Failure(new ResultError(error, description1)));
        resultAggregate.AddResult(Result<Nothing>.Failure(new ResultError(error, description2)));

        // Act
        var result = resultAggregate.Switch(Result<Nothing>.Success, Result<Nothing>.Failure);

        // Assert
        result.Should()
            .BeFailure()
            .And.Subject
            .Error.Should().BeOfType<ResultErrorAggregate>()
            .Which
            .Errors.Should().ContainKey(error)
            .WhoseValue.Should().HaveCount(2)
            .And.Subject.Should().Contain(description1).And.Contain(description2);
    }

    private static Func<Result<Nothing>> SuccessfulValidation() => Result<Nothing>.Success;

    private static Func<Result<Nothing>> FailedValidation(ResultError resultError) =>
        () => Result<Nothing>.Failure(resultError);
}
