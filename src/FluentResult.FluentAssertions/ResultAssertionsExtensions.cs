using DrifterApps.Seeds.FluentResult;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

// ReSharper disable once CheckNamespace

#pragma warning disable IDE0130
namespace FluentAssertions;
#pragma warning restore IDE0130

/// <summary>
///     Provides extension methods for asserting <see cref="Result{T}" /> instances.
/// </summary>
public static class ResultAssertionsExtensions
{
    /// <summary>
    ///     Returns an assertion object for the specified <see cref="Result{TValue}" /> instance.
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
    /// <param name="instance">The result instance to assert.</param>
    /// <returns>An assertion object for the specified result instance.</returns>
    public static ResultAssertions<TValue> Should<TValue>(this Result<TValue> instance) => new(instance);

    /// <summary>
    ///     Asserts that the specified result is successful.
    /// </summary>
    /// <typeparam name="TValue">The type of <see cref="Result{T}"/></typeparam>
    /// <param name="assertion">The assertion chain.</param>
    /// <param name="subject">The result to assert.</param>
    /// <returns>A <see cref="Continuation" /> for further assertions.</returns>
    internal static Continuation IsSuccessfulAssertion<TValue>(this AssertionChain assertion, Result<TValue> subject) =>
        assertion
            .ForCondition(subject.IsSuccess)
            .FailWith("Expected {context:result} to be a success{reason}, but it was a failure.");

    /// <summary>
    ///     Asserts that the specified result is a failure.
    /// </summary>
    /// <typeparam name="TValue">The type of <see cref="Result{T}"/></typeparam>
    /// <param name="assertion">The assertion chain.</param>
    /// <param name="subject">The result to assert.</param>
    /// <returns>A <see cref="Continuation" /> for further assertions.</returns>
    internal static Continuation IsFailureAssertion<TValue>(this AssertionChain assertion, Result<TValue> subject) =>
        assertion
            .ForCondition(subject.IsFailure)
            .FailWith("Expected {context:result} to be a failure{reason}, but it was not.");

    /// <summary>
    ///     Asserts that the specified error is as expected.
    /// </summary>
    /// <typeparam name="TValue">The type of <see cref="Result{T}"/></typeparam>
    /// <param name="assertion">The assertion chain.</param>
    /// <param name="subject">The result to assert.</param>
    /// <param name="resultError">The <see cref="ResultError" /> to assert</param>
    /// <returns>A <see cref="Continuation" /> for further assertions.</returns>
    internal static Continuation WithErrorAssertion<TValue>(this AssertionChain assertion,
        Result<TValue> subject,
        ResultError resultError) =>
        assertion
            .ForCondition(subject.Error.Equals(resultError))
            .FailWith("Expected {context:result} to have error {0}{reason}, but found {1}.", resultError,
                subject.Error);
}

/// <summary>
///     Provides assertion methods for <see cref="Result{TValue}" /> instances.
/// </summary>
public class ResultAssertions<TValue>(Result<TValue> instance)
    : ReferenceTypeAssertions<Result<TValue>, ResultAssertions<TValue>>(instance, AssertionChain.GetOrCreate())
{
    /// <summary>
    ///     Gets the identifier for the assertion.
    /// </summary>
    protected override string Identifier => "result";

    /// <summary>
    ///     Asserts that the result is successful.
    /// </summary>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="ResultAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public AndConstraint<ResultAssertions<TValue>> BeSuccessful(string because = "",
        params object[] becauseArgs)
    {
        var assertion = CurrentAssertionChain.BecauseOf(because, becauseArgs).UsingLineBreaks;

        _ = assertion.IsSuccessfulAssertion(Subject);

        return new AndConstraint<ResultAssertions<TValue>>(this);
    }

    /// <summary>
    ///     Asserts that the result is a failure.
    /// </summary>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="ResultAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public AndConstraint<ResultAssertions<TValue>> BeFailure(string because = "", params object[] becauseArgs)
    {
        var assertion = CurrentAssertionChain.BecauseOf(because, becauseArgs).UsingLineBreaks;

        _ = assertion.IsFailureAssertion(Subject);

        return new AndConstraint<ResultAssertions<TValue>>(this);
    }

    /// <summary>
    ///     Asserts that the result has the specified value.
    /// </summary>
    /// <param name="expectedValue">The expected value.</param>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="ResultAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public AndConstraint<ResultAssertions<TValue>> WithValue(TValue expectedValue,
        string because = "", params object[] becauseArgs)
    {
        var assertion = CurrentAssertionChain.BecauseOf(because, becauseArgs).UsingLineBreaks;

        _ = assertion.IsSuccessfulAssertion(Subject)
            .Then
            .ForCondition(Subject.IsSuccess && Subject.Value!.Equals(expectedValue))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:result} to have value {0}{reason}, but found {1}.", expectedValue,
                Subject.Value);

        return new AndConstraint<ResultAssertions<TValue>>(this);
    }

    /// <summary>
    ///     Asserts that the result error is as expected.
    /// </summary>
    /// <param name="resultError">The expected <see cref="ResultError" /></param>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="ResultAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public AndConstraint<ResultAssertions<TValue>> WithError(ResultError resultError, string because = "",
        params object[] becauseArgs)
    {
        var assertion = CurrentAssertionChain.BecauseOf(because, becauseArgs).UsingLineBreaks;

        _ = assertion.WithErrorAssertion(Subject, resultError);

        return new AndConstraint<ResultAssertions<TValue>>(this);
    }
}
