namespace FluentResult.Tests;

[UnitTest]
public class NothingTests
{
    [Fact]
    public void GivenValue_WhenAccessingValue_ThenShouldBeNothing()
    {
        // Arrange

        // Act
        var result = Nothing.Value;

        // Assert
        result.Should().NotBeNull().And.BeEquivalentTo(Nothing.Value);
    }

    [Fact]
    public async Task GivenTask_WhenAccessingValue_ThenShouldBeNothing()
    {
        // Arrange

        // Act
        var result = await Nothing.Task;

        // Assert
        result.Should().NotBeNull().And.BeEquivalentTo(Nothing.Value);
    }

    [Fact]
    public void GivenCompareTo_WhenComparingToNothing_ThenShouldBeZero()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing.CompareTo(Nothing.Value);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GivenCompareTo_WhenComparingToObject_ThenShouldBeZero()
    {
        // Arrange
        Nothing nothing;
        var obj = new object();

        // Act
        var result = (nothing as IComparable).CompareTo(obj);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GivenGetHashCode_WhenInvoked_ThenShouldBeZero()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing.GetHashCode();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GivenEquals_WhenComparingToNothing_ThenShouldBeTrue()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing.Equals(Nothing.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenComparingToObject_ThenShouldBeFalse()
    {
        // Arrange
        Nothing nothing;
        var obj = new object();

        // Act
        var result = nothing.Equals(obj);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GivenEquals_WhenComparingToNothingObject_ThenShouldBeTrue()
    {
        // Arrange
        Nothing nothing;
        object obj = nothing;

        // Act
        var result = nothing.Equals(obj);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GivenEqualityOperator_WhenComparingToNothing_ThenShouldBeTrue()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing == Nothing.Value;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GivenNotEqualityOperator_WhenComparingToNothing_ThenShouldBeFalse()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing != Nothing.Value;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GivenLowerThanOperator_WhenComparingToNothing_ThenShouldBeFalse()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing < Nothing.Value;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GivenGreaterThanOperator_WhenComparingToNothing_ThenShouldBeFalse()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing > Nothing.Value;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GivenLowerThanOrEqualityOperator_WhenComparingToNothing_ThenShouldBeTrue()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing <= Nothing.Value;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GivenGreaterThanOrEqualityOperator_WhenComparingToNothing_ThenShouldBeTrue()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing >= Nothing.Value;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GivenToString_WhenInvoked_ThenShouldBeNothing()
    {
        // Arrange
        Nothing nothing;

        // Act
        var result = nothing.ToString();

        // Assert
        result.Should().Be("()");
    }
}
