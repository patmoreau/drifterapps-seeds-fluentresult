namespace FluentResult.Tests;

[UnitTest]
public class ResultAggregateTests
{
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
        resultAggregate.Results.Should().ContainSingle().Which.Should().BeSameAs(successResult);
        resultAggregate.IsSuccess.Should().BeTrue();
        resultAggregate.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void AddResult_WithFailureResult_UpdatesAggregate()
    {
        // Arrange
        var resultAggregate = ResultAggregate.Create();
        var failureResult = Result<Nothing>.Failure(new ResultError("Error", "Description"));

        // Act
        resultAggregate.AddResult(failureResult);

        // Assert
        resultAggregate.Results.Should().ContainSingle().Which.Should().BeSameAs(failureResult);
        resultAggregate.IsSuccess.Should().BeFalse();
        resultAggregate.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddResult_WithMixedResults_UpdatesAggregate()
    {
        // Arrange
        var resultAggregate = ResultAggregate.Create();
        var successResult = Result<Nothing>.Success();
        var failureResult = Result<Nothing>.Failure(new ResultError("Error", "Description"));

        // Act
        resultAggregate.AddResult(successResult);
        resultAggregate.AddResult(failureResult);

        // Assert
        resultAggregate.Results.Should().HaveCount(2);
        resultAggregate.IsSuccess.Should().BeFalse();
        resultAggregate.IsFailure.Should().BeTrue();
    }
}
