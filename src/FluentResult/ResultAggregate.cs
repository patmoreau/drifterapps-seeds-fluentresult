namespace DrifterApps.Seeds.FluentResult;

/// <summary>
/// Collects multiple <see cref="Result{Nothing}"/> instances, enabling combined success or failure checks.
/// </summary>
public partial record ResultAggregate
{
    private readonly List<Result<Nothing>> _results;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultAggregate"/> class.
    /// </summary>
    private ResultAggregate() => _results = [];

    /// <summary>
    /// Gets the collection of results.
    /// </summary>
    public IReadOnlyCollection<Result<Nothing>> Results => [.. _results];

    /// <summary>
    /// Gets a value indicating whether all results are successful.
    /// </summary>
    public bool IsSuccess => _results.TrueForAll(r => r.IsSuccess);

    /// <summary>
    /// Gets a value indicating whether any result is a failure.
    /// </summary>
    public bool IsFailure => _results.Exists(r => r.IsFailure);

    /// <summary>
    /// Adds a result to the aggregate.
    /// </summary>
    /// <param name="result">The result to add.</param>
    public void AddResult(Result<Nothing> result) => _results.Add(result);

    /// <summary>
    /// Creates a new instance of the <see cref="ResultAggregate"/> class.
    /// </summary>
    /// <returns>A new instance of <see cref="ResultAggregate"/>.</returns>
    public static ResultAggregate Create() => new();
}
