using Xunit.v3;

namespace FluentResult.Tests;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class UnitTestAttribute : Attribute, ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        [new("Category", "Unit")];
}
