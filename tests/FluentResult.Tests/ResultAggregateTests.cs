// ReSharper disable RedundantArgumentDefaultValue

using System.Diagnostics.CodeAnalysis;

namespace FluentResult.Tests;

[UnitTest]
[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public class ResultAggregateTests
{
    private static readonly Faker Faker = new();
    private static readonly ResultError TestFirstError = new(Faker.Random.Word(), Faker.Lorem.Sentence());
    private static readonly ResultError TestSecondError = new(Faker.Random.Word(), Faker.Lorem.Sentence());

    public static TheoryData<ResultAggregate, bool, bool> MatchData =>
        new()
        {
            {ResultAggregate.Create(), true, false},
            {CreateWithErrors(), false, true}
        };

    public static TheoryData<ResultAggregate, bool> OnSuccessData =>
        new()
        {
            {ResultAggregate.Create(), true},
            {CreateWithErrors(), false}
        };

    public static TheoryData<ResultAggregate, bool> OnFailureData =>
        new()
        {
            {ResultAggregate.Create(), false},
            {CreateWithErrors(), true}
        };

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
        Result<Nothing> successResult = Nothing.Value;

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
        Result<Nothing> failureResult = new ResultError(Faker.Random.Word(), Faker.Lorem.Sentence());

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
        Result<Nothing> successResult = Nothing.Value;
        Result<Nothing> failureResult = new ResultError(Faker.Random.Word(), Faker.Lorem.Sentence());

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
        aggregate.AddResult(TestFirstError);

        // Act
        var result = aggregate.Ensure(() => true, TestSecondError, EnsureOnFailure.IgnoreOnFailure);

        //Assert
        result.Results.Should().ContainSingle();
    }

    [Fact]
    public void GivenEnsure_WhenResultIgnoreOnFailure_ThenReturn()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();
        aggregate.AddResult(TestFirstError);

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
        aggregate.AddResult(TestFirstError);

        // Act
        var result = aggregate.Ensure(() => true, TestSecondError, EnsureOnFailure.ValidateOnFailure);

        //Assert
        result.Results.Where(x => x.IsFailure).Should().ContainSingle();
        result.Results.Where(x => x.IsSuccess).Should().ContainSingle();
    }

    [Fact]
    public void GivenEnsure_WhenResultValidateOnFailure_ThenPerformValidation()
    {
        // Arrange
        var aggregate = ResultAggregate.Create();
        aggregate.AddResult(TestFirstError);

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
            .Ensure(() => true, TestFirstError)
            .Ensure(() => false, TestSecondError);

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
            .Ensure(FailedValidation(TestSecondError));

        //Assert
        result.Results.Where(x => x.IsFailure).Should().ContainSingle();
        result.Results.Where(x => x.IsSuccess).Should().ContainSingle();
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public void GivenMatchOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = resultIn.Match(
            () => { calledSuccess = true; },
            _ => { calledFailure = true; });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public void GivenMatchOfTOut_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = resultIn.Match(
            () =>
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
    public async Task GivenMatchOfTask_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = await resultIn.Match(
            () =>
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
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        result.IsFailure.Should().Be(expectedFailure);
        calledSuccess.Should().Be(expectedSuccess);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public async Task GivenMatchOfTaskTOut_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var calledSuccess = false;
        var calledFailure = false;

        // Act
        var result = await resultIn.Match(
            () =>
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
    public void GivenOnSuccessOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess)
    {
        // Arrange
        // Arrange
        var calledSuccess = false;

        // Act
        var result = resultIn.OnSuccess(
            () => { calledSuccess = true; });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public void GivenOnSuccessOfTOut_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess)
    {
        // Arrange
        // Arrange
        var calledSuccess = false;

        // Act
        var result = resultIn.OnSuccess(
            () =>
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
    public async Task GivenOnSuccessOfTask_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess)
    {
        // Arrange
        // Arrange
        var calledSuccess = false;

        // Act
        var result = await resultIn.OnSuccess(
            () =>
            {
                calledSuccess = true;
                return Task.CompletedTask;
            });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsSuccess.Should().Be(expectedSuccess);
        calledSuccess.Should().Be(expectedSuccess);
    }

    [Theory]
    [MemberData(nameof(OnSuccessData))]
    public async Task GivenOnSuccessOfTaskTOut_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedSuccess)
    {
        // Arrange
        // Arrange
        var calledSuccess = false;

        // Act
        var result = await resultIn.OnSuccess(
            () =>
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
    public void GivenOnFailureOfVoid_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedFailure)
    {
        // Arrange
        var calledFailure = false;

        // Act
        var result = resultIn.OnFailure(
            _ => { calledFailure = true; });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsFailure.Should().Be(expectedFailure);
        calledFailure.Should().Be(expectedFailure);
    }

    [Theory]
    [MemberData(nameof(OnFailureData))]
    public async Task GivenOnFailureOfTask_WhenInvoked_ThenAppropriateMethodIsCalled(ResultAggregate resultIn,
        bool expectedFailure)
    {
        // Arrange
        var calledFailure = false;

        // Act
        var result = await resultIn.OnFailure(
            _ =>
            {
                calledFailure = true;
                return Task.CompletedTask;
            });

        // Assert
        result.Should().BeOfType<Result<Nothing>>();
        result.IsFailure.Should().Be(expectedFailure);
        calledFailure.Should().Be(expectedFailure);
    }

    [Fact]
    public void GivenToErrorAggregate_WhenMultipleFailureResults_ThenReturnsValidationError()
    {
        // Arrange
        var error1 = Faker.Random.Word();
        var error2 = Faker.Random.Word();
        var description1 = Faker.Lorem.Sentence();
        var description2 = Faker.Lorem.Sentence();
        var resultAggregate = ResultAggregate.Create();
        resultAggregate.AddResult(new ResultError(error1, description1));
        resultAggregate.AddResult(new ResultError(error2, description2));

        // Act
        var result = resultAggregate.ToErrorAggregate<Nothing>();

        // Assert
        result.Should().BeOfType<ResultErrorAggregate>()
            .Which
            .Errors.Should().ContainKey(error1)
            .WhoseValue.Should().ContainSingle(description1);
        result.Should().BeOfType<ResultErrorAggregate>()
            .Which
            .Errors.Should().ContainKey(error2)
            .WhoseValue.Should().ContainSingle(description2);
    }

    [Fact]
    public void GivenToErrorAggregate_WhenDuplicateFailureResults_ThenReturnsValidationErrorWithAggregatedDescriptions()
    {
        // Arrange
        var error = Faker.Random.Word();
        var description1 = Faker.Lorem.Sentence();
        var description2 = Faker.Lorem.Sentence();
        var resultAggregate = ResultAggregate.Create();
        resultAggregate.AddResult(new ResultError(error, description1));
        resultAggregate.AddResult(new ResultError(error, description2));

        // Act
        var result = resultAggregate.ToErrorAggregate<Nothing>();

        // Assert
        result.Should().BeOfType<ResultErrorAggregate>()
            .Which
            .Errors.Should().ContainKey(error)
            .WhoseValue.Should().HaveCount(2)
            .And.Subject.Should().Contain(description1).And.Contain(description2);
    }

    private static ResultAggregate CreateWithErrors()
    {
        var aggregate = ResultAggregate.Create();
        for (var i = 0; i < Faker.Random.Int(1, 5); i++)
        {
            aggregate.AddResult(new ResultError(Faker.Lorem.Word(), Faker.Lorem.Sentence()));
        }

        return aggregate;
    }

    private static Func<Result<Nothing>> SuccessfulValidation() => () => Nothing.Value;

    private static Func<Result<Nothing>> FailedValidation(ResultError resultError) =>
        () => (Result<Nothing>) resultError;
}
