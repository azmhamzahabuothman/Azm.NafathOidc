# Azm.NafathOidc

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)

A self-contained .NET client for the **Nafath OIDC v2 API**, Saudi Arabia's national digital identity. Drop it into any ASP.NET Core project. No shared base classes, no monolithic settings.

---

## Table of Contents

- [Package Structure](#package-structure)
- [Getting Started](#getting-started)
- [Configuration Reference](#configuration-reference)
- [API Reference](#api-reference)
- [Models](#models)
- [Null Object Pattern](#null-object-pattern)
- [Migrating from Tahakom](#migrating-from-tahakom)
- [Dependencies](#dependencies)

---

## Package Structure

```
Azm.NafathOidc/
├── Azm.NafathOidc.csproj
├── INafathOidcService.cs          // public interface
├── NafathOidcService.cs           // real implementation
├── NullNafathOidcService.cs       // mock / null-object
├── DependencyInjection.cs         // DI extensions
├── Configuration/
│   └── NafathOidcOptions.cs
└── Models/
    ├── NafathOidcSessionResponse.cs
    ├── NafathOidcJwtResponse.cs
    ├── NafathOidcValidateTokenResponse.cs
    └── NafathOidcJwkSet.cs
```

---

## Getting Started

### 1. Install the package

Reference it as a project, or pack it as a NuGet package:

```bash
# Project reference (local)
dotnet add reference ../Azm.NafathOidc/Azm.NafathOidc.csproj

# Or pack and consume as NuGet
dotnet pack Azm.NafathOidc.csproj -c Release
dotnet add package Azm.NafathOidc --source ./bin/Release
```

### 2. Add configuration

Add a `NafathOidc` section to your `appsettings.json`:

```json
{
  "NafathOidc": {
    "BaseUrl": "https://nafath.api.elm.sa",
    "AppId": "your-app-id",
    "AppKey": "your-app-key"
  }
}
```

> ⚠️ Don't commit real `AppId` / `AppKey` values. Use [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables in production.

### 3. Register in DI

In `Program.cs` or your service registration:

```csharp
using Azm.NafathOidc;

// Real implementation — reads from the "NafathOidc" config section
builder.Services.AddNafathOidc(builder.Configuration);

// Or with a custom section name
builder.Services.AddNafathOidc(builder.Configuration, "MyNafathSection");

// Or mock mode (no HTTP calls, returns stub data)
builder.Services.AddNafathOidcNull();
```

### 4. Inject and use

```csharp
public class AuthController : ControllerBase
{
    private readonly INafathOidcService _nafath;

    public AuthController(INafathOidcService nafath)
    {
        _nafath = nafath;
    }

    [HttpGet("nafath/session")]
    public async Task<IActionResult> StartSession(string locale, string requestId)
    {
        var session = await _nafath.InitiateSession(locale, requestId);
        if (session is null) return BadRequest();

        return Ok(session);
    }
}
```

---

## Configuration Reference

The `NafathOidcOptions` class binds to your config section via `IOptions<T>`.

| Property  | Type     | Description                                                     |
|-----------|----------|-----------------------------------------------------------------|
| `BaseUrl` | `string` | Nafath API base URL (e.g. `https://nafath.api.elm.sa`)          |
| `AppId`   | `string` | Application ID issued by Nafath, sent as the `APP-ID` header    |
| `AppKey`  | `string` | Application key issued by Nafath, sent as the `APP-KEY` header  |

**Section name:** defaults to `"NafathOidc"`. Override it by passing a custom section name to `AddNafathOidc(configuration, "YourSection")`.

---

## API Reference

> All methods return `null` when the HTTP call fails or the response cannot be deserialized.

| Method                | HTTP   | Returns                            |
|-----------------------|--------|------------------------------------|
| `InitiateSession`     | `GET`  | `NafathOidcSessionResponse?`       |
| `ExchangeStateForJwt` | `POST` | `NafathOidcJwtResponse?`           |
| `ValidateIdToken`     | `POST` | `NafathOidcValidateTokenResponse?` |
| `GetJwk`              | `GET`  | `NafathOidcJwkSet?`                |

### `InitiateSession`

Starts a new Nafath OIDC session. The user should be redirected to the returned URL.

```csharp
var session = await nafath.InitiateSession(
    locale: "ar",
    requestId: "req-abc-123"
);

// session.Url         → redirect the user here
// session.HashedState → store for callback verification
// session.RequestId   → your original request ID
// session.Id          → Nafath session ID
```

| Parameter   | Type     | Description                     |
|-------------|----------|---------------------------------|
| `locale`    | `string` | Session locale (`"ar"` or `"en"`) |
| `requestId` | `string` | Your unique request identifier  |

### `ExchangeStateForJwt`

Exchanges the callback `state` parameter for a JWT after the user completes authentication.

```csharp
var jwt = await nafath.ExchangeStateForJwt(state: callbackState);

// jwt.Token → the ID token (JWT)
// jwt.State → echoed state
```

### `ValidateIdToken`

Server-side validation of a Nafath ID token.

```csharp
var result = await nafath.ValidateIdToken(idToken: jwt.Token);

if (result?.Valid == true)
{
    // Token is valid — proceed with user claims
}
```

### `GetJwk`

Fetches the JSON Web Key Set for local token signature verification.

```csharp
var jwks = await nafath.GetJwk();

foreach (var key in jwks.Keys)
{
    // key.Kty, key.Kid, key.Alg, key.N, key.E
}
```

---

## Models

| Class                             | Properties                                 |
|-----------------------------------|--------------------------------------------|
| `NafathOidcSessionResponse`       | `Id`, `Url`, `HashedState`, `RequestId`    |
| `NafathOidcJwtResponse`           | `State`, `Token`                           |
| `NafathOidcValidateTokenResponse` | `Valid` (`bool`)                           |
| `NafathOidcJwkSet`                | `Keys` (`List<NafathOidcJwk>`)             |
| `NafathOidcJwk`                   | `Kty`, `Kid`, `Use`, `Alg`, `N`, `E`       |

`NafathOidcJwkSet` and `NafathOidcJwk` use `[JsonProperty]` attributes for Newtonsoft.Json serialization.

---

## Null Object Pattern

`NullNafathOidcService` implements `INafathOidcService` with stub responses. No HTTP calls are made. Use it when Nafath is disabled in a given environment:

```csharp
if (appSettings.NafathOidc.Enabled)
    builder.Services.AddNafathOidc(builder.Configuration);
else
    builder.Services.AddNafathOidcNull();
```

The null service returns mock URLs, a stub JWT token (`"mock-id-token"`), and always validates as `true`.

---

## Migrating from Tahakom

If you're replacing the in-project Nafath OIDC service with this package:

| Before (Tahakom)                                              | After (Azm.NafathOidc)                        |
|---------------------------------------------------------------|-----------------------------------------------|
| `Azm.Tahakom.Application.Interfaces.Auth.INafathOidcService`  | `Azm.NafathOidc.INafathOidcService`           |
| `Azm.Tahakom.Domian.Models.Auth.*`                            | `Azm.NafathOidc.Models.*`                     |
| `AppSettings.NafathOidc.App_Id`                               | `NafathOidcOptions.AppId`                     |
| `AppSettings.NafathOidc.App_Key`                              | `NafathOidcOptions.AppKey`                    |
| `AppSettings.NafathOidc.Url`                                  | `NafathOidcOptions.BaseUrl`                   |
| Manual DI in `IntegrationDependencies.cs`                     | `services.AddNafathOidc(configuration)`       |

**Namespace change:** update `using` directives in all consumers (command handlers, controllers, and validators). The interface signature is identical; only the namespace and model imports change.

---

## Dependencies

| Package                                                 | Version  | Purpose                                  |
|---------------------------------------------------------|----------|------------------------------------------|
| `RestSharp`                                             | 112.1.0  | HTTP client for Nafath API calls         |
| `Newtonsoft.Json`                                       | 13.0.3   | JSON serialization (JWK model attributes)|
| `Serilog`                                               | 3.1.1    | Request/response logging                 |
| `Microsoft.Extensions.Options`                          | 8.0.2    | `IOptions<T>` configuration binding      |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.2    | `IServiceCollection` extensions          |
