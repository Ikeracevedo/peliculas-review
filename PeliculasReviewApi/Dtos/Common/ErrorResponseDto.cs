namespace PeliculasReviewApi.Dtos.Common;

/// <summary>
/// Cuerpo de error con la misma forma que las excepciones HTTP de NestJS:
/// <c>{ "message": ..., "error": "Not Found", "statusCode": 404 }</c>.
/// <c>message</c> es un string (p. ej. NotFoundException) o un array de strings (ValidationPipe).
/// El frontend lee <c>error.response.data.message</c>.
/// </summary>
public class ErrorResponseDto
{
    public object Message { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
    public int StatusCode { get; init; }

    public static ErrorResponseDto NotFound(string message) =>
        new() { Message = message, Error = "Not Found", StatusCode = StatusCodes.Status404NotFound };

    public static ErrorResponseDto BadRequest(IReadOnlyList<string> messages) =>
        new() { Message = messages, Error = "Bad Request", StatusCode = StatusCodes.Status400BadRequest };

    public static ErrorResponseDto Unauthorized(string message) =>
        new() { Message = message, Error = "Unauthorized", StatusCode = StatusCodes.Status401Unauthorized };

    public static ErrorResponseDto Forbidden(string message) =>
        new() { Message = message, Error = "Forbidden", StatusCode = StatusCodes.Status403Forbidden };

    public static ErrorResponseDto Conflict(string message) =>
        new() { Message = message, Error = "Conflict", StatusCode = StatusCodes.Status409Conflict };
}
