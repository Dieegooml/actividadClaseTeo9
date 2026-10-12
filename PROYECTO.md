# Lab05DiegoMedina — Documentación del proyecto

> Estado actual del proyecto a fecha de la última revisión. Este fichero documenta la
> implementación existente; cuando se definan los nuevos requisitos se actualizará.

## 1. Visión general

**Lab05DiegoMedina** es una API REST para la gestión de un colegio. Expone operaciones CRUD
sobre estudiantes, profesores, cursos, materias, matrículas, asistencias y evaluaciones, y
gestiona usuarios con autenticación JWT y autorización basada en roles y claims.

### Stack tecnológico

| Componente            | Tecnología                                          |
| --------------------- | --------------------------------------------------- |
| Lenguaje              | C# (nullable habilitado, implicit usings)           |
| Framework             | .NET 10 (ASP.NET Core Web API)                      |
| ORM                   | Entity Framework Core 9 + Pomelo MySQL              |
| Base de datos         | MySQL 8 (collation `utf8mb4_0900_ai_ci`)            |
| Autenticación         | JWT Bearer (HS256) + BCrypt para contraseñas        |
| Documentación API     | Swagger / Swashbuckle 10.2.3                        |
| DI de paquetes        | BCrypt.Net-Next 4.2.0, JwtBearer 10.0.12, EF Tools 9.0.1 |

### Estructura de carpetas

```
Lab05DiegoMedina/
├── Controllers/        → Endpoints REST de la API (uno por modelo)
├── Models/             → Entidades del dominio + DbContext
├── Repositories/       → Repositorio genérico (IGenericRepository<T>)
├── Services/           → Lógica de negocio (un servicio + interfaz por modelo, JWT, Hash)
├── UnitOfWork/         → Patrón Unit of Work
├── Program.cs          → Configuración, DI, JWT, políticas y Swagger
├── appsettings.json    → Configuración general + cadena de conexión
└── Lab05DiegoMedina.http → Ejemplos de peticiones para pruebas
```

> **Nota (simplificación LAB06):** se eliminaron las carpetas `Security/` (`JwtOptions`,
> `Policies`) y `DTOs/`. La configuración JWT se lee con `IConfiguration` desde `appsettings`,
> las políticas se definen como texto en `Program.cs` y los usuarios se devuelven como objetos
> simples (`UsuarioInfo`/`AuthResult`) que nunca exponen el hash de la contraseña.

## 2. Estructura por capas

### 2.1 Models (`Models/`)

Entidades creadas por scaffolding a partir de la base de datos `Colegio`:

| Entidad       | Descripción                                              | Relaciones                                   |
| ------------- | -------------------------------------------------------- | -------------------------------------------- |
| `Usuario`     | Cuenta de acceso (login). Roles: **Estudiante/Profesor/Admin**. | —                                            |
| `Estudiante`  | Datos personales del estudiante.                         | 1-N Asistencia, Evaluaciones, Matriculas     |
| `Profesore`   | Datos del profesor.                                      | —                                            |
| `Curso`       | Curso del colegio con créditos.                          | 1-N Asistencia, Evaluaciones, Materias, Matriculas |
| `Materia`     | Materia perteneciente a un curso (N-1 Curso).            | N-1 Curso                                    |
| `Matricula`   | Inscripción de estudiante a un curso por semestre.       | N-1 Curso, N-1 Estudiante                    |
| `Asistencia`  | Registro de asistencia de un estudiante a un curso.      | N-1 Curso, N-1 Estudiante                    |
| `Evaluacione` | Calificación de un estudiante en un curso.               | N-1 Curso, N-1 Estudiante                    |

Detalles relevantes del `ColegioDBContext`:

- Collation global `utf8mb4_0900_ai_ci`.
- `Usuario.Rol` se persiste como texto (`Admin/Profesor/Estudiante`), mapeado a un **enum `Rol`** en C#.
- `Usuario.UserName` tiene índice único (`ix_usuarios_user_name`).
- `Usuario.CreadoEn` usa `CURRENT_TIMESTAMP(6)` por defecto.
- Las entidades de dominio tienen la convención de nombres generada por scaffolding
  (`Profesore`, `Evaluacione`, etc.).

### 2.2 Repositories (`Repositories/GenericRepository/`)

Patrón repositorio genérico para reducir código repetido:

- `IGenericRepository<T>` — `GetAllAsync()`, `GetByIdAsync(int)`, `FindAsync(expresión)`,
  `AddAsync`, `Update`, `Delete`, `ExistsAsync(int)`.
- `GenericRepository<T>` — implementación sobre `DbSet<T>`.
  - **Add no hace commit**: el guardado lo delega en `UnitOfWork.Complete()`.
  - **Update** hace `ChangeTracker.Clear()` antes de adjuntar la entidad, marcándola como
    `Modified` (patrón simple de adjuntado/desadjuntado).

### 2.3 UnitOfWork (`UnitOfWork/`)

- `IUnitOfWork.Repository<T>()` — devuelve un `IGenericRepository<T>` (se cachean en un
  `Hashtable` para reutilizar el mismo repositorio por llamada por tipo).
- `Complete()` — persiste los cambios con `SaveChangesAsync()`.

### 2.4 Services (`Services/`)

Un servicio (con su interfaz) por cada modelo; todos acceden a datos mediante
`IUnitOfWork.Repository<T>()` sin crear repositorios por modelo:

| Servicio             | Modelo       | Responsabilidad                                         |
| -------------------- | ------------ | ------------------------------------------------------- |
| `AuthService`        | `Usuario`    | Login, registro, listado/detalle/borrado de usuarios, cambio de rol, reporte. |
| `EstudianteService`  | `Estudiante` | CRUD.                                                   |
| `ProfesorService`    | `Profesore`  | CRUD.                                                   |
| `CursoService`       | `Curso`      | CRUD.                                                   |
| `MateriaService`     | `Materia`    | CRUD.                                                   |
| `MatriculaService`   | `Matricula`  | Matriculación con validaciones y CRUD.                  |
| `AsistenciaService`  | `Asistencia` | CRUD.                                                   |
| `EvaluacionService`  | `Evaluacione`| CRUD.                                                   |
| `BCryptPasswordHasher` | —          | Hash y verificación de contraseñas con BCrypt (work factor 12). |
| `JwtTokenService`    | —            | Generación del token JWT firmado (HS256) con claims del usuario. |

Los servicios CRUD siguen una forma uniforme: `CreateAsync` lanza `InvalidOperationException`
si el id ya existe; `UpdateAsync` lanza `InvalidOperationException` si el id no coincide y
`KeyNotFoundException` si no existe; `DeleteAsync` lanza `KeyNotFoundException` si el id no
existe. Los controladores traducen las excepciones a `400`/`404`.

#### `AuthService` — reglas de negocio

- **Login** devuelve `null` (→ 401) si las credenciales fallan o el usuario está inactivo; se
  compara siempre contra un hash para no revelar si el usuario existe.
- **Registro público** siempre asigna rol `Estudiante`.
- **Cambiar rol**: no se permite cambiarse el propio rol ni quitar el rol Admin al último admin.
- **Eliminar usuario**: no se permite eliminar la propia cuenta ni al último admin.
- Devuelve `UsuarioInfo`/`AuthResult`/`ReporteInfo` (records simples) que nunca incluyen el
  hash de la contraseña.

### 2.5 Configuración JWT y políticas (en `Program.cs`)

Ya no existe la carpeta `Security/`:

- **JWT** se lee directo de `appsettings` (`Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`,
  `Jwt:ExpirationMinutes`) con `IConfiguration`, tanto en `Program.cs` (validación del bearer)
  como en `JwtTokenService` (generación del token).
- **Políticas** definidas inline en `AddAuthorization` con su **nombre en texto**:
  - `"SoloAdmin"` → rol `Admin`.
  - `"ProfesorOAdmin"` → roles `Profesor` o `Admin`.
  - `"EstudianteOAdmin"` → roles `Estudiante` o `Admin`.
- Los controladores usan `[Authorize(Policy = "SoloAdmin")]`, etc., como texto.

#### Claims del JWT (`JwtTokenService`)

| Claim      | Tipo                            | Uso                              |
| ---------- | ------------------------------- | -------------------------------- |
| `sub`      | `JwtRegisteredClaimNames.Sub`   | Id del usuario (`IdUsuarioActual()` en AuthController). |
| `unique_name` | userName                     | Nombre de usuario.               |
| `name`     | Nombre completo                 | Nombre legible.                  |
| `email`    | Correo                          | Correo del usuario.              |
| `role`     | Rol (texto del enum)            | Autorización por políticas.      |
| `jti`, `iat` | uuid / timestamp             | Identificador y emisión.         |

En `Program.cs` se configura `MapInboundClaims = false` y se fijan `NameClaimType` /
`RoleClaimType` para que las políticas `RequireRole` lea el claim `role` correctamente.

## 3. Autenticación y autorización

Flujo:

1. `POST /api/Auth/login?userName=...&password=...` (parámetros simples, sin DTOs) →
   devuelve `accessToken`, `expiresIn` y `usuario` (`UsuarioInfo`, sin hash de contraseña).
2. El cliente envía el token en el header `Authorization: Bearer <token>`.
3. `Program.cs` valida el token (issuer, audience, lifetime, firma) y aplica el claim `role`.
4. Cada endpoint aplica su política con `[Authorize(Policy = "SoloAdmin")]` etc.

**Verificación del token (LAB06)** — login `admin` devuelve un JWT con estos claims:

```
sub: 1                  unique_name: admin    name: Administrador del Colegio
email: admin@colegio.edu  role: Admin         jti: <uuid>
nbf / exp / iat / iss / aud
```

El claim `role` alimenta las políticas por rol. Comprobado en ejecución (ver § 7).

Matriz de acceso por endpoint:

| Endpoint                                                         | Sin token | Autenticado | Profesor | Estudiante | Admin |
| ---------------------------------------------------------------- | :-------: | :---------: | :------: | :--------: | :---: |
| `POST /api/Auth/login` / `register`                              |    ✔️     |      —      |    —     |     —      |   —   |
| `GET/PUT/DELETE /api/Auth/usuarios...`, `admin/reporte`           |     |      ✖️     |          |            |  ✔️   |
| `GET /api/Cursos`, `GET /api/Materias`                           |           |      ✔️     |    ✔️    |     ✔️     |  ✔️   |
| `POST/PUT/DELETE /api/Cursos`, `/api/Materias`                   |           |             |          |            |  ✔️   |
| `GET /api/Estudiantes`, `/api/Profesores`, `/api/Asistencias`    |           |             |    ✔️    |            |  ✔️   |
| `POST/PUT /api/Estudiantes`, `/api/Profesores`, `/api/Asistencias`, `/api/Evaluaciones` | | |  ✔️  |    | ❌✔️¹ |
| `GET /api/Matriculas`, `GET /api/Evaluaciones` (lista)            |           |             |    ✔️    |            |  ✔️   |
| `GET /api/Evaluaciones/{id}`, `GET /api/Matriculas/{id}`          |           |             |    ✔️    |     ✔️     |  ✔️   |
| `POST /api/Matriculas`                                           |           |             |    ✔️    |            |  ✔️   |
| `DELETE` general (Cursos, Materias, Estudiantes, Profesores, Asistencias, Evaluaciones, Matriculas) | | | | |  ✔️   |

¹ Nota: en `EvaluacionesController`, `POST/PUT` usan `ProfesorOAdmin`, y `DELETE` usa `SoloAdmin`.

## 4. Endpoints de la API

| Método | Ruta                              | Descripción                                   |
| ------ | --------------------------------- | --------------------------------------------- |
| POST   | `/api/Auth/login`                 | Login por query (`userName`, `password`), devuelve JWT.              |
| POST   | `/api/Auth/register`              | Registro público por query (rol Estudiante).        |
| GET    | `/api/Auth/usuarios`              | Listar usuarios (SoloAdmin).                  |
| GET    | `/api/Auth/usuarios/{id}`         | Ver un usuario (SoloAdmin).                   |
| PUT    | `/api/Auth/usuarios/{id}/rol`     | Cambiar rol (SoloAdmin).                      |
| DELETE | `/api/Auth/usuarios/{id}`         | Eliminar usuario (SoloAdmin).                 |
| GET    | `/api/Auth/admin/reporte`         | Totales (estudiantes, profesores, cursos, usuarios) — SoloAdmin. |
| GET    | `/api/Cursos` / `/{id}`           | Listar / ver cursos (autenticado).            |
| POST/PUT/DELETE | `/api/Cursos...`        | CRUD de cursos (SoloAdmin).                   |
| GET    | `/api/Materias` / `/{id}`         | Listar / ver materias (autenticado).          |
| POST/PUT/DELETE | `/api/Materias...`      | CRUD de materias (SoloAdmin).                 |
| GET    | `/api/Estudiantes`                | Listar estudiantes (ProfesorOAdmin).          |
| GET    | `/api/Estudiantes/{id}`           | Ver estudiante (autenticado).                 |
| POST/PUT | `/api/Estudiantes...`           | Crear / actualizar (SoloAdmin).               |
| DELETE | `/api/Estudiantes/{id}`           | Eliminar (SoloAdmin).                         |
| GET    | `/api/Profesores` / `/{id}`       | Listar / ver (ProfesorOAdmin).                |
| POST/PUT | `/api/Profesores...`            | Crear / actualizar (SoloAdmin).               |
| DELETE | `/api/Profesores/{id}`            | Eliminar (SoloAdmin).                         |
| GET    | `/api/Asistencias` / `/{id}`      | Listar / ver (ProfesorOAdmin).                |
| POST/PUT | `/api/Asistencias...`           | Crear / actualizar (ProfesorOAdmin).          |
| DELETE | `/api/Asistencias/{id}`           | Eliminar (SoloAdmin).                         |
| GET    | `/api/Evaluaciones`               | Listar (ProfesorOAdmin).                      |
| GET    | `/api/Evaluaciones/{id}`          | Ver (EstudianteOAdmin).                       |
| POST/PUT | `/api/Evaluaciones...`          | Crear / actualizar (ProfesorOAdmin).          |
| DELETE | `/api/Evaluaciones/{id}`          | Eliminar (SoloAdmin).                         |
| GET    | `/api/Matriculas`                 | Listar (ProfesorOAdmin).                      |
| GET    | `/api/Matriculas/{id}`            | Ver (EstudianteOAdmin).                       |
| POST   | `/api/Matriculas`                 | Matricular con validaciones (ProfesorOAdmin). |
| DELETE | `/api/Matriculas/{id}`            | Eliminar (SoloAdmin).                         |

## 5. Configuración y ejecución

### Requisitos

- .NET SDK 10
- MySQL 8 (local en `localhost:3306`) con base de datos `Colegio`

### Cadena de conexión (`appsettings.json`)

```
Server=localhost;Port=3306;Database=Colegio;User=root;Password=;
```

El `DbContext` se registra con `UseMySql` y `ServerVersion.AutoDetect(connectionString)`.
Las entidades se asumen creadas por scaffolding (`--no-onconfiguring`); no hay migraciones
EF en el proyecto.

### Configuración JWT (`appsettings.Development.json`)

- `Issuer`: `Lab05DiegoMedina.Api`
- `Audience`: `Lab05DiegoMedina.Clients`
- `Key`: secreto de 64 caracteres (min. 32)
- `ExpirationMinutes`: `60`

### Puesta en marcha

```bash
cd Lab05DiegoMedina
dotnet run          # perfil http → http://localhost:5024
```

- Swagger disponible en `/swagger` (solo entorno Development), con botón **Authorize** para
  pegar el token.
- `Lab05DiegoMedina.http` contiene ejemplos listos para ejecutar (login admin/profesor/estudiante,
  registro, gestión de usuarios, listado de cursos y estudiantes). Claves de prueba:

| Usuario     | Clave           | Rol       |
| ----------- | --------------- | --------- |
| `admin`     | `Admin1234`     | Admin     |
| `profesor`  | `Profesor1234`  | Profesor  |
| `estudiante`| `Estudiante1234`| Estudiante|

## 6. Observaciones y deuda técnica

1. **Versiones mixtas de EF Core**: `Microsoft.EntityFrameworkCore.Design` y `.Tools` son `9.0.1`
   mientras el target es `net10.0`. Conviene subir ambos paquetes a la versión 10 para alinearlos,
   o confirmar que no generan conflictos en tiempo de diseño.
2. **`GenericRepository<T>.Update`** hace `ChangeTracker.Clear()` en cada llamada: es un riesgo
   dentro de un Unit of Work en el que se toquen varias entidades antes del commit, y puede
   avisar de desajustes al adjuntar entidades ya rastreadas.
3. **Sin migraciones EF**: el esquema vive en MySQL y los modelos se mantienen por scaffolding;
   cualquier cambio de esquema debe replicarse a mano o añadir migraciones.
4. **Modelos expuestos directamente** en los controladores CRUD (no DTOs): el cliente recibe y
   envía las entidades de dominio completas. En `Auth` sí se devuelven objetos simples
   (`UsuarioInfo`/`AuthResult`) que no exponen el hash de la contraseña.
5. **Login/registro por query params**: aceptado como "parámetros simples" del laboratorio,
   pero envían credenciales en la URL (visible en logs); en producción convendría un POST con
   cuerpo cifrado o formulario seguro.
6. **Credenciales de ejemplo en `appsettings.json`**: cadena de conexión con `root` sin password.
   Solo apto para desarrollo local.
7. **Nombres de entidades del scaffolding** (`Profesore`, `Evaluacione`) que no siguen la
   convención de C#; renombrarlas implicaría tocar mapeos o generar un nuevo scaffolding.
8. **`Reporte`** en `AuthService` consulta los catálogos pero agrega solo conteos simples;
   es un endpoint candidato a extenderse.
9. **`DELETE /api/Matriculas/{id}`** en `MatriculaService.DeleteAsync` no valida que el id exista
   (los demás servicios sí lo hacen), aunque el controlador lo comprueba previamente.
10. **Sin tests** automatizados ni pipeline CI/CD.

## 7. LAB06 — JWT y autorización (simplificado y verificado)

### Simplificación aplicada

1. **Eliminada la carpeta `Security/`** (`JwtOptions.cs`, `Policies.cs`).
   - JWT leído con `IConfiguration` desde `appsettings` en `Program.cs` y `JwtTokenService`.
   - Políticas `"SoloAdmin"`, `"ProfesorOAdmin"`, `"EstudianteOAdmin"`
     definidas inline en `AddAuthorization` y usadas como texto en los controladores.
2. **Eliminada la carpeta `DTOs/`**.
   - Login/registro reciben **parámetros simples** (query).
   - Los servicios devuelven records simples (`UsuarioInfo`, `AuthResult`, `ReporteInfo`) sin
     el hash de la contraseña.
3. **Endpoints eliminados de `AuthController`/`AuthService`**:
   `POST /api/Auth/cambiar-password`, `POST /api/Auth/usuarios` (crear con rol),
   `GET /api/Auth/me` y `GET /api/Auth/mi-correo`. Se mantienen `login`, `register`,
   `GET/PUT/DELETE /api/Auth/usuarios...` y `admin/reporte`.
4. **Servicios por modelo**: creados `EstudianteService`, `ProfesorService`, `CursoService`,
   `MateriaService`, `AsistenciaService`, `EvaluacionService` (interfaz + implementación
   propias, sin servicio genérico), registrados en `Program.cs`. Los controladores CRUD dejan
   de usar `IUnitOfWork` directo y delegan en su servicio.
5. **`Lab05DiegoMedina.http`** actualizado a los endpoints vigentes y parámetros por query.

### Verificación (en ejecución, MySQL local)

| Prueba                                                            | Resultado |
| ----------------------------------------------------------------- | :-------: |
| Login `admin` (query params) → token JWT con claims               | 200 |
| Claims del token (`role`, `email`, `sub`, `unique_name`, `name`, ...) | ✔ |
| Respuesta de login sin hash de contraseña (`UsuarioInfo`)         | ✔ |
| `GET /api/Auth/usuarios` Admin / Estudiante                       | **200** / **403** |
| `GET /api/Estudiantes` Profesor                                   | **200** |
| `GET /api/Auth/admin/reporte` Admin                               | **200** (`ReporteInfo`) |
| CRUD Cursos por `CursoService`: POST/PUT/GET/DELETE Admin         | **201 / 204 / 200 / 204** |
| `POST /api/Cursos` con rol Estudiante                             | **403** |
| Swagger: `GET /swagger/v1/swagger.json`                            | 200 e incluye los endpoints de Auth |

La base de datos quedó con los 3 usuarios originales.

### Observaciones y conclusiones del laboratorio

- La autorización por **roles** con `RequireRole` cubre las políticas `SoloAdmin`,
  `ProfesorOAdmin` y `EstudianteOAdmin` usando el claim `role` del token.
- El token incluye `role`, `email`, `sub`, `unique_name` y `name`; no fue necesario generar
  claims adicionales para las políticas.
- Se confirma el pipeline `autenticación → autorización`: **401** (sin token) → **403**
  (autenticado sin el rol requerido) → **200** (autorizado).
- La compilación de `[FromBody] Usuario` para login/registro falla por la validación implícita
  de referencias no-nulas de los modelos (`Nombre` requerido), por eso se eligieron
  parámetros simples por query (opción permitida por la especificación del laboratorio).
- Swagger sigue exponiendo los endpoints y el botón **Authorize** funciona con el token Bearer
  obtenido en login.
- Las políticas por rol y todos los endpoints existentes se mantuvieron sin romper.

## 8. Pendiente

Cuando se reciban los requisitos del siguiente paso, se documentarán aquí los cambios planeados
y su implementación sobre esta base.