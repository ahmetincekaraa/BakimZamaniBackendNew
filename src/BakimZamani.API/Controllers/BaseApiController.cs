namespace BakimZamani.API.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Base API controller with common functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID from JWT claims.
    /// </summary>
    protected string? CurrentUserId => User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;

    /// <summary>
    /// Gets the current user's email from JWT claims.
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirst("email")?.Value;

    /// <summary>
    /// Gets the current user's role from JWT claims.
    /// </summary>
    protected string? CurrentUserRole => User.FindFirst("role")?.Value;
}

