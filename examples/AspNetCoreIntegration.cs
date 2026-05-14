using DrifterApps.Seeds.FluentResult;
using Microsoft.AspNetCore.Mvc;

namespace FluentResult.Examples;

// Shows the recommended pattern for mapping Result<T> to HTTP responses
// in both controller and minimal API styles.

// --- Controller style ---

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    {
        var result = await _service.GetUserAsync(id, ct);
        return result.Match(
            onSuccess: user => Ok(user),
            onFailure: err  => MapToActionResult(err)
        );
    }

    [HttpPost("{id:guid}/email")]
    public async Task<IActionResult> ChangeEmail(
        Guid id, [FromBody] ChangeEmailRequest request, CancellationToken ct)
    {
        var result = await _service.ChangeEmailAsync(id, request.NewEmail, ct);
        return result.Match(
            onSuccess: user => Ok(user),
            onFailure: err  => MapToActionResult(err)
        );
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _service.RegisterAsync(
            request.Email, request.Name, request.Age, ct);

        return result.Match(
            onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
            onFailure: err  => err is ResultErrorAggregate agg
                ? UnprocessableEntity(new ValidationProblemResponse(agg))
                : MapToActionResult(err)
        );
    }

    private IActionResult MapToActionResult(ResultError err) => err.Code switch
    {
        "User.NotFound"     => NotFound(new ErrorResponse(err)),
        "User.EmailTaken"   => Conflict(new ErrorResponse(err)),
        "User.InvalidEmail" => BadRequest(new ErrorResponse(err)),
        _                   => Problem(err.Description)
    };
}

// --- Minimal API style ---

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}", GetUser);
        app.MapPost("/users/{id:guid}/email", ChangeEmail);
        return app;
    }

    private static async Task<IResult> GetUser(
        Guid id, UserService svc, CancellationToken ct)
    {
        var result = await svc.GetUserAsync(id, ct);
        return result.Match(
            onSuccess: Results.Ok,
            onFailure: err => err.Code switch
            {
                "User.NotFound" => Results.NotFound(),
                _               => Results.Problem(err.Description)
            }
        );
    }

    private static async Task<IResult> ChangeEmail(
        Guid id, ChangeEmailRequest request, UserService svc, CancellationToken ct)
    {
        var result = await svc.ChangeEmailAsync(id, request.NewEmail, ct);
        return result.Match(
            onSuccess: Results.Ok,
            onFailure: err => err.Code switch
            {
                "User.NotFound"   => Results.NotFound(),
                "User.EmailTaken" => Results.Conflict(),
                _                 => Results.Problem(err.Description)
            }
        );
    }
}

// --- Response DTOs ---

public sealed record ChangeEmailRequest(string NewEmail);
public sealed record RegisterRequest(string Email, string Name, int Age);
public sealed record ErrorResponse(string Code, string Description)
{
    public ErrorResponse(ResultError err) : this(err.Code, err.Description) { }
}
public sealed record ValidationProblemResponse(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public ValidationProblemResponse(ResultErrorAggregate agg)
        : this(agg.Code, agg.Description, agg.Errors) { }
}
