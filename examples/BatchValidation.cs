using DrifterApps.Seeds.FluentResult;

namespace FluentResult.Examples;

// Demonstrates all ResultAggregate patterns: AddResult, Ensure,
// EnsureOnFailure, and extracting the grouped error dictionary.
public static class BatchValidationExamples
{
    // Pattern 1: fluent Ensure chain — validates all fields, collects all errors.
    public static Result<Nothing> ValidateRegistration(
        string email, string password, int age)
    {
        return ResultAggregate.Create()
            .Ensure(() => !string.IsNullOrWhiteSpace(email),  Errors.EmailRequired)
            .Ensure(() => email.Contains('@'),                 Errors.EmailInvalid)
            .Ensure(() => password.Length >= 8,               Errors.PasswordTooShort)
            .Ensure(() => ContainsUppercase(password),        Errors.PasswordNoUppercase)
            .Ensure(() => age >= 18,                          Errors.UnderAge,
                    EnsureOnFailure.IgnoreOnFailure) // only checked when prior guards pass
            .OnSuccess(() => Nothing.Value);
    }

    // Pattern 2: AddResult with results from separate validation methods.
    public static Result<OrderSummary> ValidateOrder(
        string productCode, int quantity, decimal unitPrice)
    {
        var aggregate = ResultAggregate.Create();

        aggregate.AddResult(ValidateProductCode(productCode));
        aggregate.AddResult(ValidateQuantity(quantity));
        aggregate.AddResult(ValidatePrice(unitPrice));

        return aggregate.OnSuccess(() =>
            new OrderSummary(productCode, quantity, unitPrice * quantity).ToResult());
    }

    // Pattern 3: Extracting the grouped error dictionary for API responses.
    public static object ProduceValidationProblemDetails(string email, string password)
    {
        var aggregate = ResultAggregate.Create()
            .Ensure(() => !string.IsNullOrWhiteSpace(email),  Errors.EmailRequired)
            .Ensure(() => email.Contains('@'),                 Errors.EmailInvalid)
            .Ensure(() => password.Length >= 8,               Errors.PasswordTooShort);

        return aggregate.Match(
            onSuccess: () => new { Message = "Valid" },
            onFailure: agg =>
            {
                // agg.Errors is IReadOnlyDictionary<string, string[]>
                // e.g. { "Registration.EmailRequired": ["Email is required."] }
                return new { agg.Code, agg.Description, Errors = agg.Errors };
            });
    }

    private static Result<Nothing> ValidateProductCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return OrderErrors.ProductCodeRequired;
        if (code.Length > 20)
            return OrderErrors.ProductCodeTooLong;
        return Nothing.Value;
    }

    private static Result<Nothing> ValidateQuantity(int quantity) =>
        quantity > 0 ? Nothing.Value : OrderErrors.InvalidQuantity;

    private static Result<Nothing> ValidatePrice(decimal price) =>
        price > 0m ? Nothing.Value : OrderErrors.InvalidPrice;

    private static bool ContainsUppercase(string s) =>
        s.Any(char.IsUpper);

    private static class Errors
    {
        internal static readonly ResultError EmailRequired =
            new("Registration.EmailRequired", "Email is required.");

        internal static readonly ResultError EmailInvalid =
            new("Registration.EmailInvalid", "Email format is invalid.");

        internal static readonly ResultError PasswordTooShort =
            new("Registration.PasswordTooShort", "Password must be at least 8 characters.");

        internal static readonly ResultError PasswordNoUppercase =
            new("Registration.PasswordNoUppercase", "Password must contain at least one uppercase letter.");

        internal static readonly ResultError UnderAge =
            new("Registration.UnderAge", "Must be at least 18 years old.");
    }

    private static class OrderErrors
    {
        internal static readonly ResultError ProductCodeRequired =
            new("Order.ProductCodeRequired", "Product code is required.");

        internal static readonly ResultError ProductCodeTooLong =
            new("Order.ProductCodeTooLong", "Product code must not exceed 20 characters.");

        internal static readonly ResultError InvalidQuantity =
            new("Order.InvalidQuantity", "Quantity must be greater than zero.");

        internal static readonly ResultError InvalidPrice =
            new("Order.InvalidPrice", "Unit price must be greater than zero.");
    }
}

public sealed record OrderSummary(string ProductCode, int Quantity, decimal Total);
