using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PeliculasReviewApi.Controllers;

/// <summary>
/// Equivalente a <c>AppController</c> en NestJS (<c>GET /api</c>).
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class AppController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetHello() => Content("Hello World!", "text/plain; charset=utf-8");
}
