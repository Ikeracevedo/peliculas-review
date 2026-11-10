namespace PeliculasReviewApi.Models;

/// <summary>
/// Rol del usuario. Equivale al enum <c>Rol</c> de Prisma.
/// Los nombres se mantienen en MAYÚSCULAS para que, serializados como texto
/// ("USUARIO" / "ADMIN"), coincidan exactamente con el contrato de NestJS.
/// </summary>
public enum Rol
{
    USUARIO,
    ADMIN
}
