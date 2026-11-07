using Microsoft.AspNetCore.Mvc;

namespace PeliculasReviewApi.Controllers;

/// <summary>
/// Equivalente a <c>AppController</c> en NestJS (<c>GET /api</c>).
/// Endpoint de lectura pública (healthcheck / bienvenida).
/// </summary>
[ApiController]
[Route("api")]
public class AppController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetHello() => Content("Hello World!", "text/plain; charset=utf-8");
}
