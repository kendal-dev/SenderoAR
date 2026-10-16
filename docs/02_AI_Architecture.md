# 02 · Arquitectura del chatbot

> Destilado de `02_AI_Architecture.md` v2.0 (21/05/2026). Contrastado contra
> `Core/Contracts/IChatbotClient.cs` y `Models/ChatbotErrorKind.cs` el 16/10/2026.
> Leyenda de marcas: ver [`README.md`](README.md).
>
> **Todo este documento es 📐 salvo el contrato (§6.1) y el enum de errores (§6.2).**
> No hay paquetes de Firebase en `Packages/manifest.json`.

---

## 1. Decisiones

| Componente | Decisión | Motivo |
|---|---|---|
| SDK | Firebase Unity SDK 13.10.0+ con **Firebase AI Logic** | Proxy a Gemini sin API key en el cliente |
| Backend | **Vertex AI**, no la Developer API | SLA empresarial |
| Región | **`southamerica-east1`** (São Paulo), explícita en el código | Sin región, GCP usa `us-central1` (~150 ms más de RTT desde Bolivia) ⚠️ |
| Modelo | **`gemini-3.5-flash`**, leído de Remote Config | 🔁 En mayo se descartó `gemini-3.1-flash-lite` por un supuesto bug de "Greedy Search" (respuestas cortadas con `finish_reason=STOP`) ⚠️. El README todavía dice "3.1 Flash-Lite" en la sección de visión |
| Modelo de respaldo | Otro 3.x vía Remote Config | No usar 2.5-flash: se anunció su retiro para oct-2026 ⚠️ |
| Plan GCP | Blaze (pago por uso) | Necesario para cuotas de producción |
| System prompt | **Español**, personaje "Padre Felipe" | Mejor cobertura del vocabulario patrimonial (reducción, horcones, chiquitano) |
| Cuota | **5 RPM por usuario** | Contra *denial-of-wallet* |
| Seguridad | **App Check enforced** + protección de replay | Antes de cualquier uso público |

> ⚠️ **Verificar al llegar el sprint del chatbot**: nombres de modelos, precios, fechas de
> retiro y la deprecación de parámetros (§2) salieron de reportes de mayo de 2026. Se
> confirman en la consola de Firebase y en la documentación del SDK **el día que se integre**,
> no antes.

---

## 2. Configuración de generación

Según el reporte de mayo ⚠️, `temperature`, `topP` y `topK` quedaron **deprecados** en
Firebase AI Logic 13.10+. El comportamiento se controla con las *system instructions*.

```csharp
var generationConfig = new GenerationConfig(
    maxOutputTokens: 1024,
    stopSequences: new[] { "\n\n---\n\n", "Nota del autor:" }
);
// Sin temperature, topP, topK.
```

Presupuesto previsto: entrada media ~1 500 tokens (historial + instrucciones), salida ~200.
Alertas de presupuesto en GCP: **5 / 8 / 10 USD por mes** (la última como corte).

---

## 3. System instruction — "Padre Felipe"

> ⚠️ Los hechos del bloque "Conocimiento de dominio" son los mismos de
> [`01_Monuments_Database.md`](01_Monuments_Database.md) y **ninguno tiene fuente**. Si el
> prompt le da al modelo un dato falso como verdad, el modelo lo repite con seguridad, que
> es justo lo que la regla "NUNCA inventes fechas" intenta evitar. Este bloque se reescribe
> con datos verificados antes de conectarlo.

```
Identidad:
Eres el "Padre Felipe", un guía historiador del Conjunto Misional de
San José de Chiquitos, Bolivia. Eres un personaje culto, paciente y
profundamente respetuoso del legado jesuítico y la cultura chiquitana.

Tono:
Académico pero accesible. Español neutro con vocabulario claro. Explica
los tecnicismos arquitectónicos. Tono cálido, nunca arrogante. Si la
pregunta es confusa, pide aclaración con gentileza.

Conocimiento de dominio (los 5 monumentos):
1. Templo de San José de Chiquitos (1745-1760): único templo jesuítico
   chiquitano construido íntegramente en piedra y cal. Dirigido por el
   Padre Bartolomé de Mora con mano de obra Penoca.
2. Capilla Mortuoria o Miserere (c. 1750): capilla anexa de ritos
   fúnebres. Nombre del salmo "Miserere mei, Deus".
3. Torre del Campanario (c. 1748): estructura exenta, separada del templo.
4. La Bóveda (1754): antiguo Colegio Misional, hoy Espacio Cultural
   Chiquitos. Murales de Gregorio Villarroel (1810), restaurados por
   Hans Roth.
5. Pila Bautismal (c. 1652): su datación precede al templo actual.
Restauración general: arquitecto suizo Hans Roth.
Patrimonio Mundial UNESCO desde 1990.

Reglas firmes:
- NUNCA inventes fechas, nombres o eventos. Si no estás seguro, di:
  "No tengo registro confirmado de ese detalle, le recomiendo
  consultar al Plan Misiones".
- Eres un padre jesuita historiador, no un guía turístico moderno.
- Si preguntan por temas fuera del Conjunto Misional, redirige
  amablemente al tema patrimonial.
- Si preguntan en otro idioma, responde en ese idioma y ofrece
  continuar en español.
- Máximo 3-4 oraciones por turno, salvo que pidan profundidad.

Formato de salida:
Texto plano narrativo. Sin viñetas, sin markdown, sin tablas.
```

El prompt queda **siempre en español**, aunque el usuario pregunte en otro idioma.

> Nota de diseño: el contrato `QueryAsync` recibe un `LanguageCode`. La implementación
> decide cómo usarlo (por ejemplo, anteponer "Responde en inglés" al turno), sin cambiar
> el system prompt.

---

## 4. Remote Config — claves previstas

| Clave | Default | Uso |
|---|---|---|
| `active_gemini_model` | `gemini-3.5-flash` | Cambiar de modelo sin publicar una versión nueva |
| `system_instructions_version` | `v2.0_padre_felipe_es` | Versionado del prompt |
| `max_output_tokens` | `1024` | |
| `ai_feedback_enabled` | `true` | Botones 👍/👎 (post-MVP) |
| `cache_threshold` | `0.85` | Umbral de similitud del cache offline ([`08_`](08_Demo_Mode_Defense.md) §2) |

**Nunca** escribir el nombre del modelo como literal en el código: siempre Remote Config
con default.

---

## 5. App Check

| Plataforma | Proveedor |
|---|---|
| Android | Play Integrity |
| iOS | DeviceCheck / App Attest |
| Editor | `DebugAppCheckProviderFactory` |

- Modo **Enforced** (no Monitoring) antes de cualquier uso público; replay protection activa.
- SHA-1 y SHA-256 del keystore en Firebase Console → Project Settings → Android app. Se
  sacan con `keytool -list -v -keystore <keystore> -alias <alias>` cuando exista el keystore.

```csharp
public static class AppCheckBootstrap
{
    public static void Configure()
    {
#if UNITY_ANDROID
        FirebaseAppCheck.SetAppCheckProviderFactory(PlayIntegrityProviderFactory.Instance);
#elif UNITY_IOS
        FirebaseAppCheck.SetAppCheckProviderFactory(DeviceCheckProviderFactory.Instance);
#else
        FirebaseAppCheck.SetAppCheckProviderFactory(DebugAppCheckProviderFactory.Instance);
#endif
    }
}
```

**El cliente Unity nunca contiene una API key.** Toda autenticación pasa por Firebase + App Check.

---

## 6. Cliente C#

### 6.1. Contrato — ✅ en el repo

`Assets/_SenderoAR/Core/Contracts/IChatbotClient.cs`:

```csharp
public interface IChatbotClient : IDisposable
{
    bool IsReady { get; }
    event Action<ChatbotErrorKind> ErrorOccurred;

    // Inicializa Firebase, valida App Check, descarga Remote Config.
    Task InitializeAsync(CancellationToken cancellationToken = default);

    // Devuelve "" si hubo un error categorizado (ver ErrorOccurred).
    Task<string> QueryAsync(
        string userQuery,
        LanguageCode language,
        CancellationToken cancellationToken = default,
        int timeoutSeconds = 15);

    Task ResetSessionAsync(CancellationToken cancellationToken = default);
}
```

> 🔁 Los ejemplos de mayo (en este documento y en `04_` / `08_`) usaban
> `IsServiceReady`, `OnAIError`, `AIErrorType`, `InitializeServiceAsync` y
> `QueryHeritageExpertAsync` **sin** parámetro de idioma. Esos nombres no existen. Manda la
> firma de arriba.

### 6.2. Manejo de errores — ✅ enum en el repo, 📐 comportamiento

`ChatbotErrorKind.cs` cita esta sección. Mapeo previsto:

| Origen | `ChatbotErrorKind` | Acción |
|---|---|---|
| HTTP 429 `RESOURCE_EXHAUSTED` | `QuotaExceeded` | Backoff exponencial + mensaje amable + cache offline |
| HTTP 503 `SERVICE_UNAVAILABLE` | `BackendUnavailable` | 1 reintento, luego cache offline |
| HTTP 400 `INVALID_ARGUMENT` | `InvalidRequest` | Log + sanear el prompt |
| Sin red | `NetworkUnreachable` | Directo al cache, sin reintento |
| Timeout (`timeoutSeconds`) | `Timeout` | "Está tardando más de lo esperado" + cache |
| `OperationCanceledException` por el `CancellationToken` del llamador | `Cancelled` | Silencioso: el usuario se fue |
| Token de App Check rechazado | `AppCheckRejected` | Log; no reintentar |
| Cualquier otro | `Unknown` | Log + mensaje genérico |

### 6.3. Inicialización del backend

```csharp
// Correcto: Vertex AI con región explícita
_firebaseAI = FirebaseAI.GetInstance(FirebaseAI.Backend.VertexAI("southamerica-east1"));

// Incorrecto: Developer API, sin SLA
// FirebaseAI.GetInstance(FirebaseAI.Backend.GoogleAI());

// Incorrecto: sin región, cae en us-central1
// FirebaseAI.GetInstance(FirebaseAI.Backend.VertexAI());
```

### 6.4. Estructura prevista

```
Core/Services/                    (App.Services.FirebaseAI.asmdef, autoReferenced = false)
├── ChiquitaniaHeritageChatService.cs   implementa IChatbotClient
└── AppCheckBootstrap.cs
ViewModels/
└── ChatViewModel.cs
```

El `.asmdef` aislado evita que el resto del proyecto dependa de las DLL de Firebase.
Ver [`04_MVVM_Patterns.md`](04_MVVM_Patterns.md) §2.

---

## 7. Checklist de QA del chatbot

- [ ] 50 RPM simulados sin errores 429
- [ ] Conversación de 15–20 turnos que conserva el contexto
- [ ] Preguntas en EN y PT → responde en ese idioma y ofrece español
- [ ] `FirebaseApp.CheckAndFixDependenciesAsync()` correcto en el A52s
- [ ] El `CancellationToken` aborta pedidos colgados
- [ ] Cambio de modelo vía Remote Config sin publicar versión nueva
- [ ] Pedido sin token de App Check → rechazado
- [ ] Latencia desde Santa Cruz < 800 ms con `southamerica-east1`
- [ ] Red cortada a mitad de pedido → responde el cache offline
- [ ] Pregunta sobre un dato que no está en el prompt → usa la frase de "no tengo registro"

---

## 8. Orden de implementación previsto

1. Proyecto Firebase + Blaze + alertas de presupuesto + `google-services.json`.
2. SDK con `.asmdef` aislado; App Check en modo Debug.
3. Smoke test: 1 pedido desde el Editor y 1 desde el APK.
4. `ChiquitaniaHeritageChatService` + Remote Config.
5. Cache offline Levenshtein ([`08_`](08_Demo_Mode_Defense.md) §2).
6. `ChatViewModel` + vista.
7. Tests de cancelación, timeout, cuota y fallback. App Check pasa a Enforced.

> `google-services.json` y `GoogleService-Info.plist` **no son secretos** en sentido estricto,
> pero el repo es público: decidir si se versionan antes de crearlos, y en cualquier caso
> dejar App Check enforced para que no se puedan usar desde fuera.
