// ReSharper disable EqualExpressionComparison

using System.Diagnostics.CodeAnalysis;
using FluentAssertions.Execution;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace FluentResult.Tests;

[UnitTest]
[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
[SuppressMessage("Usage", "xUnit1045:Avoid using TheoryData type arguments that might not be serializable")]
[SuppressMessage("Design", "CA1034:Nested types should not be visible")]
#pragma warning disable CS1718 // Comparison made to same variable
public class ResultErrorAggregateTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void GivenEquals_WhenOtherIsSelf_ThenReturnsTrue()
    {
        // Arrange
        var code = Faker.Random.String();
        var description = Faker.Random.String();
        var property = Faker.Random.Hash();
        var message = Faker.Lorem.Sentence();
        var error1 =
            new ResultErrorAggregate(code, description, new Dictionary<string, string[]> {{property, [message]}});

        // Act
        var result = error1.Equals(error1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GivenEquals_WhenOtherIsNull_ThenReturnsFalse()
    {
        // Arrange
        var code = Faker.Random.String();
        var description = Faker.Random.String();
        var property = Faker.Random.Hash();
        var message = Faker.Lorem.Sentence();
        var error1 =
            new ResultErrorAggregate(code, description, new Dictionary<string, string[]> {{property, [message]}});

        // Act
        var result = error1 == null;

        // Assert
        result.Should().BeFalse();
    }

    [Theory, ClassData(typeof(EqualsData))]
    public void GivenEquals_WhenComparingToOther_ThenReturnsExpected(
        string because,
        string codeSelf, string descriptionSelf, Dictionary<string, string[]> validationErrorsSelf,
        string codeOther, string descriptionOther, Dictionary<string, string[]> validationErrorsOther,
        bool expected)
    {
        // Arrange
        var errorSelf =
            new ResultErrorAggregate(codeSelf, descriptionSelf, validationErrorsSelf);
        var errorOther =
            new ResultErrorAggregate(codeOther, descriptionOther, validationErrorsOther);

        // Act
        var resultEquals = errorSelf.Equals(errorOther);
        var resultOperator = errorSelf == errorOther;
        var resultHashCode = errorSelf.GetHashCode() == errorOther.GetHashCode();

        // Assert
        using var scope = new AssertionScope();
        resultEquals.Should().Be(expected, because);
        resultOperator.Should().Be(expected, because);
        resultHashCode.Should().Be(expected, because);
    }

    internal class EqualsData : TheoryData<
        string,
        string, string, Dictionary<string, string[]>,
        string, string, Dictionary<string, string[]>,
        bool>
    {
        public EqualsData()
        {
            var codeSelf = Faker.Random.Word();
            var descriptionSelf = Faker.Lorem.Sentence();
            var propertySelf = Faker.Random.Hash();
            var messageSelf = Faker.Lorem.Sentence();
            var errorSelf = new Dictionary<string, string[]> {{propertySelf, [messageSelf]}};
            Add("same all", codeSelf, descriptionSelf, errorSelf,
                codeSelf, descriptionSelf, errorSelf, true);
            Add("same but new dictionary", codeSelf, descriptionSelf, errorSelf,
                codeSelf, descriptionSelf, new Dictionary<string, string[]> {{propertySelf, [messageSelf]}}, true);
            Add("different code", codeSelf, descriptionSelf, errorSelf,
                Faker.Random.Word(), descriptionSelf, errorSelf, false);
            Add("different description", codeSelf, descriptionSelf, errorSelf,
                codeSelf, Faker.Lorem.Sentence(), errorSelf, false);
            Add("different key", codeSelf, descriptionSelf, errorSelf,
                codeSelf, descriptionSelf, new Dictionary<string, string[]> {{Faker.Random.Hash(), [messageSelf]}},
                false);
            Add("different key count", codeSelf, descriptionSelf, errorSelf,
                codeSelf, descriptionSelf, new Dictionary<string, string[]>
                {
                    {propertySelf, [messageSelf]},
                    {Faker.Random.Hash(), [Faker.Lorem.Sentence()]},
                }, false);
            Add("different validation", codeSelf, descriptionSelf, errorSelf,
                codeSelf, descriptionSelf, new Dictionary<string, string[]> {{propertySelf, [Faker.Lorem.Sentence()]}},
                false);
            Add("different validation count", codeSelf, descriptionSelf, errorSelf, codeSelf, descriptionSelf,
                new Dictionary<string, string[]>
                {
                    {propertySelf, [messageSelf, Faker.Lorem.Sentence()]},
                }, false);
        }
    }
}
