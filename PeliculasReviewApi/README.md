# Películas Review API

## Documentación y pruebas con Swagger UI

Desde la raíz del repositorio, inicia la API con:

```bash
dotnet run --project PeliculasReviewApi --launch-profile http
```

Abre [Swagger UI](http://localhost:5000/swagger) para consultar y probar los endpoints. El documento generado por Microsoft.AspNetCore.OpenApi está disponible en [OpenAPI JSON](http://localhost:5000/openapi/v1.json).

Para probar operaciones protegidas:

1. Ejecuta `POST /api/auth/login` o `POST /api/auth/registro` con **Try it out** y copia el valor `access_token` de la respuesta.
2. Pulsa **Authorize**, pega solamente el token (sin `Bearer `) y confirma con **Authorize**.
3. Ejecuta la operación deseada con **Try it out**. Las operaciones de usuarios requieren un token con rol `ADMIN`; otras operaciones pueden tener restricciones adicionales de propietario.

En una base de datos nueva, los datos iniciales incluyen `admin@ejemplo.com` con contraseña `Password123!`. Si la base de datos ya contiene usuarios, utiliza las credenciales existentes.
