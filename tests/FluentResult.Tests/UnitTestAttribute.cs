using Xunit.v3;

namespace FluentResult.Tests;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
#pragma warning disable CA1515
public sealed class UnitTestAttribute : Attribute, ITraitAttribute
#pragma warning restore CA1515
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        [new("Category", "Unit")];
}
