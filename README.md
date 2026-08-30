# poc-swagger-net47

Proof of Concept – Swashbuckle consumer-based Swagger segmentation on **.NET Framework 4.7 / ASP.NET Web API 2**.

## Goal

Generate separate OpenAPI (Swagger) JSON documents per consumer (`consumer1`, `consumer2`) so that each consumer only sees the endpoints and schemas relevant to them. The resulting JSON files can be imported directly into **WSO2 API Manager**.

---

## Project structure

```
PocSwagger/
├── App_Start/
│   ├── SwaggerConfig.cs          # Swashbuckle MultipleApiVersions + Swagger UI
│   └── WebApiConfig.cs           # Attribute routing
├── Attributes/
│   └── ApiConsumerAttribute.cs   # [ApiConsumer("consumer1", "consumer2")]
├── Controllers/
│   └── TestApiController.cs      # Method1 (c1), Method2 (c1+c2), Method3 (c2)
├── Models/
│   └── Dtos.cs                   # Consumer1Request/Response, Consumer2Request/Response, SharedRequest/Response
├── Global.asax / Global.asax.cs
├── Web.config
└── packages.config
```

---

## How to run

### Prerequisites
- Visual Studio 2017 or later (or `msbuild` with Web Application targets)
- .NET Framework 4.7 SDK
- NuGet package restore enabled

### Steps

1. **Restore NuGet packages**

   ```
   nuget restore PocSwagger.sln
   ```

2. **Build**

   Open `PocSwagger.sln` in Visual Studio and press **F5**, or run:

   ```
   msbuild PocSwagger.sln /p:Configuration=Debug
   ```

3. **Run with IIS Express** (Visual Studio)

   Press **F5**. The application will start at `http://localhost:<port>/`.

---

## Swagger UI

| URL | Description |
|-----|-------------|
| [http://localhost:<port>/swagger](http://localhost:<port>/swagger) | Swagger UI with consumer selector |

Use the **"Explore"** dropdown (enabled via `EnableDiscoveryUrlSelector`) to switch between the two consumer documents.

---

## Download OpenAPI JSON

| Consumer | URL |
|----------|-----|
| consumer1 | [http://localhost:<port>/swagger/docs/consumer1](http://localhost:<port>/swagger/docs/consumer1) |
| consumer2 | [http://localhost:<port>/swagger/docs/consumer2](http://localhost:<port>/swagger/docs/consumer2) |

---

## Endpoint matrix

| Endpoint | consumer1 | consumer2 |
|----------|-----------|-----------|
| `POST /api/test/method1` | ✅ | ❌ |
| `POST /api/test/method2` | ✅ | ✅ |
| `POST /api/test/method3` | ❌ | ✅ |

---

## Key components

### `ApiConsumerAttribute`

```csharp
[ApiConsumer("consumer1")]          // only consumer1
[ApiConsumer("consumer1", "consumer2")]  // both consumers
```

Apply to an action method **or** a controller class.

### `SwaggerConfig`

Registers two Swagger documents (`consumer1`, `consumer2`) via `MultipleApiVersions`.  
The resolver checks the `ApiConsumerAttribute` at action level first, then controller level.

