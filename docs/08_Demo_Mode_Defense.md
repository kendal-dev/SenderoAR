# 08 · Modo demo y preparación de la defensa

> Destilado de `08_Demo_Mode_Defense.md` v1.0 (21/05/2026). **Reescrito en parte**: ver §1.
> Leyenda de marcas: ver [`README.md`](README.md).
>
> **Todo es 📐.** No existe `Core/DemoMode/` ni el símbolo `DEMO_MODE`.

---

## 1. Qué cambió respecto del documento original, y por qué

El documento de mayo combinaba dos cosas distintas:

1. **Ingeniería de resiliencia** — que la demo no se caiga si falla el Wi-Fi o Vertex AI.
   Eso es legítimo y se conserva entero.
2. **Ocultarle al tribunal qué estaba viendo** — latencia artificial "para no levantar
   sospecha", no pronunciar nunca las palabras *cache* o *fallback*, reformular la pregunta
   para que la capture el cache sin decirlo, una frase preparada que atribuía cualquier
   falla a "saturación de red". **Eso se eliminó.**

Motivos:

- El tribunal evalúa **el sistema que se construyó**. Si ve una respuesta guardada creyendo
  que es Gemini en vivo, está evaluando otra cosa.
- El cache offline **es una funcionalidad del diseño** (respuesta sin conexión en un sitio
  rural con mala cobertura), no un truco. Mostrarlo como tal suma puntos: es un caso de
  degradación elegante que se puede defender técnicamente.
- Si un miembro del tribunal pregunta "¿esto está en vivo?", la respuesta tiene que ser
  verdadera y fácil. Lo es si la interfaz ya lo dice.

**Regla que reemplaza a lo eliminado:** cada respuesta del chatbot muestra su origen
(🌐 *en vivo* / 💾 *guardada, sin conexión*), y la latencia es la real.

---

## 2. Cache offline con distancia de Levenshtein

### Por qué Levenshtein

| Alternativa | Veredicto |
|---|---|
| **Levenshtein** | ✅ Determinista, sin dependencias, ~5 KB de código |
| Embeddings (Unity Sentis) | ❌ Modelo de 50+ MB y latencia variable para este alcance |
| Hash exacto | ❌ Falla con cualquier typo o paráfrasis |

```
similitud(A, B) = 1 - Levenshtein(A, B) / max(|A|, |B|)      ∈ [0, 1]
```

**Umbral: 0.85** (configurable por Remote Config, clave `cache_threshold`).

### Normalización de la consulta

```csharp
private static string Normalize(string text)
{
    var decomposed = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder(decomposed.Length);
    foreach (var c in decomposed)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue; // tildes
        if (c == '¿' || c == '?' || c == ',' || c == '.') continue;
        sb.Append(c);
    }
    return sb.ToString().Trim();
}
```

`"¿Quién construyó la torre del campanario?"` → `"quien construyo la torre del campanario"`.

> Levenshtein a nivel de caracteres es sensible al largo y al orden de las palabras: dos
> paráfrasis con las mismas palabras en otro orden pueden quedar bajo 0.85. Probar el umbral
> con preguntas reales antes de fijarlo, y cubrir las variantes más probables como entradas
> separadas del JSON.

### Implementación de referencia

```csharp
public sealed class LevenshteinQACache : ILocalQACache
{
    private readonly struct Entry
    {
        public readonly string Question, Answer;
        public Entry(string q, string a) { Question = q; Answer = a; }
    }

    private List<Entry> _entries;
    public bool IsReady => _entries != null && _entries.Count > 0;

    public (bool matched, string answer, float similarity) FindBestMatch(string query, float threshold)
    {
        var q = Normalize(query);
        float best = 0f; string answer = null;
        foreach (var e in _entries)
        {
            var s = Similarity(q, e.Question);
            if (s > best) { best = s; answer = e.Answer; }
        }
        return (best >= threshold, answer, best);
    }

    public string FallbackResponse =>
        "No tengo registro confirmado de ese detalle. Le recomiendo consultar al Plan Misiones.";

    private static float Similarity(string a, string b)
    {
        if (a == b) return 1f;
        int max = Math.Max(a.Length, b.Length);
        return max == 0 ? 1f : 1f - (float)Levenshtein(a, b) / max;
    }

    private static int Levenshtein(string s, string t)
    {
        // Dos filas en vez de la matriz completa: O(min(n,m)) de memoria.
        if (s.Length < t.Length) (s, t) = (t, s);
        var prev = new int[t.Length + 1];
        var curr = new int[t.Length + 1];
        for (int j = 0; j <= t.Length; j++) prev[j] = j;
        for (int i = 1; i <= s.Length; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }
        return prev[t.Length];
    }
    // LoadAsync (lee el JSON), Normalize, Dispose …
}
```

Es lógica pura: va en `App.Domain` o `App.MVVM` y se prueba con NUnit sin Unity. **Buen
candidato para los primeros tests del proyecto.**

### Formato del JSON

```json
{
  "version": "1.0",
  "language": "es-BO",
  "items": [
    {
      "id": "q01",
      "category": "historico_arquitectonico",
      "question": "¿Quién construyó el templo de San José?",
      "answer": "…",
      "source": "Autor, obra, año, página"
    }
  ]
}
```

Se agrega el campo **`source`**: cada respuesta guardada es un dato que el chatbot afirma
como hecho, así que lleva fuente igual que la tesis (regla 4). Ubicación prevista:
`Assets/_SenderoAR/Art/Cache/qa_es.json`. Al ser contenido público y revisable, conviene
**versionarlo** (agregar la excepción al `.gitignore`, como `Art/Tracking/`).

### Banco de preguntas (30, por categoría)

| Categoría | # | Ejemplos |
|---|---|---|
| Histórico-arquitectónico | 8 | ¿Quién construyó el templo? · ¿Cuál es su singularidad? |
| Restauración | 5 | ¿Qué restauró Hans Roth? ¿En qué período? |
| Patrimonio UNESCO | 4 | ¿Cuándo se declaró? ¿Con qué criterios? |
| Pila Bautismal | 4 | ¿Por qué tiene fecha de 1652? ⚠️ (ver [`01_`](01_Monuments_Database.md)) |
| Técnica AR / software | 5 | ¿Cómo funciona el image tracking? ¿Por qué Unity? |
| Patrimonio boliviano | 4 | ¿Qué es Plan Misiones? ¿Qué otras misiones existen? |

Las redacta el autor, con las respuestas ya verificadas.

---

## 3. Decoradores — 📐 diseño

El patrón Decorator permite agregar el cache **sin tocar** `IChatbotClient` ni la
implementación real. Ver [`04_`](04_MVVM_Patterns.md) §3 para dónde se ensambla.

```
ViewModel ──► IChatbotClient
                 │
                 ▼
     OfflineFallbackChatbotDecorator      ← producción: red primero, cache si falla
                 │
                 ▼
     ChiquitaniaHeritageChatService       ← Vertex AI
```

- **Producción**: red primero; si `ErrorOccurred` reporta `NetworkUnreachable`,
  `BackendUnavailable`, `QuotaExceeded` o `Timeout`, se busca en el cache. Esto no es solo
  para la defensa: es el comportamiento offline que promete el diseño.
- **`DEMO_MODE`** (opcional): mismo decorador más el HUD del operador (§5) y el control
  remoto (§6). `App.DemoMode.asmdef` con `defineConstraints: ["DEMO_MODE"]` hace que ese
  código ni siquiera entre en un build sin el símbolo.

Para que la Vista pueda mostrar el origen de la respuesta (§1), el decorador necesita
informarlo. Opciones: un evento propio del decorador, o devolver un tipo
`ChatAnswer { string Text; AnswerSource Source; }`. La segunda **cambia el contrato**
`QueryAsync`; decidirlo antes de implementar el ViewModel del chat.

```csharp
// Esqueleto — firmas reales de IChatbotClient
public sealed class OfflineFallbackChatbotDecorator : IChatbotClient
{
    private readonly IChatbotClient _inner;
    private readonly ILocalQACache _cache;
    private readonly INetworkDetector _network;
    private readonly float _threshold;

    public bool IsReady => _inner.IsReady || _cache.IsReady;
    public event Action<ChatbotErrorKind> ErrorOccurred
    {
        add => _inner.ErrorOccurred += value;
        remove => _inner.ErrorOccurred -= value;
    }

    public async Task<string> QueryAsync(string userQuery, LanguageCode language,
        CancellationToken cancellationToken = default, int timeoutSeconds = 15)
    {
        if (await _network.CheckConnectivityAsync())
        {
            var live = await _inner.QueryAsync(userQuery, language, cancellationToken, timeoutSeconds);
            if (!string.IsNullOrEmpty(live)) return live;      // "" = error categorizado
        }
        var (matched, cached, _) = _cache.FindBestMatch(userQuery, _threshold);
        return matched ? cached : _cache.FallbackResponse;
    }

    public Task InitializeAsync(CancellationToken ct = default) => _inner.InitializeAsync(ct);
    public Task ResetSessionAsync(CancellationToken ct = default) => _inner.ResetSessionAsync(ct);
    public void Dispose() { _inner.Dispose(); _cache.Dispose(); }
}
```

> El cache está en español. Si `language` no es `EsBO` y no hay red, decidir si se responde
> en español con un aviso o con el `FallbackResponse` traducido.

---

## 4. Detección de red

```csharp
public sealed class NetworkDetector : INetworkDetector
{
    private const string PingUrl = "https://www.gstatic.com/generate_204";
    private static readonly HttpClient Http = new HttpClient();   // una sola instancia

    public bool IsOnline { get; private set; }

    public async Task<bool> CheckConnectivityAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var response = await Http.GetAsync(PingUrl, cts.Token);
            return IsOnline = response.StatusCode == HttpStatusCode.NoContent;
        }
        catch { return IsOnline = false; }
    }
}
```

`generate_204` devuelve 204 solo con internet real: detecta Wi-Fi conectado pero detrás de
un portal cautivo (habitual en redes universitarias). `Application.internetReachability`
no distingue ese caso.

---

## 5. HUD del operador — 📐 (`#if DEMO_MODE`)

Panel chico en una esquina, opacidad baja, para quien opera el teléfono:

| Indicador | Valor |
|---|---|
| FPS | Frame rate actual |
| Net | Online / Offline |
| AI | Vertex / Cache / Caído |
| Sim | Similitud de la última consulta al cache |
| Thermal | OK / Warm / Hot (heurística con batería y FPS) |
| Mode | DEMO / PROD |

`FPSDiagnosticsHUD` (✅ en el repo, `Core/Diagnostics/`) es el punto de partida.

---

## 6. Control remoto Bluetooth — 📐 opcional

Un presentador tipo Logitech R500s emula PageUp/PageDown. Con el Input System:
adelante → siguiente monumento · atrás → anterior · mantener 3 s → cambiar idioma. Evita
tocar la pantalla mientras se señala la mesa.

---

## 7. Guion de la demostración (15–20 min)

| Min | Acción | Si falla |
|---|---|---|
| 0–2 | Presentación; mostrar las bases en la mesa | — |
| 2–5 | Templo: modelo + animación + texto orbital + narración ES | Segundo dispositivo |
| 5–8 | Cambio ES → EN en vivo (texto y audio) | — |
| 8–12 | Torre: campanas + texto orbital | Tap-to-place ([`03_`](03_AR_Tracking_Specs.md) §9) |
| 12–15 | Chatbot: 3–4 preguntas; el origen de cada respuesta se ve en pantalla | Cache offline, **dicho en voz alta**: es una funcionalidad |
| 15–18 | Pila: agua animada + su historia | — |
| 18–20 | Cierre: arquitectura (MVVM, Vertex AI, presupuesto de rendimiento) | — |

Preguntas sugeridas para el chatbot (definirlas con datos verificados):
1. "Padre Felipe, ¿quién construyó este templo y en qué época?"
2. "¿Por qué la pila bautismal tiene una fecha anterior al templo?" ⚠️ solo si el dato se confirma
3. "¿Qué hizo Hans Roth por estas misiones?"

Una pregunta no anticipada del tribunal se responde en vivo si hay red; si no, el cache o la
respuesta de "no tengo registro confirmado". Las dos son comportamientos correctos del
sistema.

---

## 8. Checklists

### Víspera
- [ ] Build con el símbolo correcto instalado en los dos dispositivos
- [ ] Paquete pt-BR descargado en ambos
- [ ] Baterías al 100 %; cargador y cable en la mochila
- [ ] Bases y miniaturas en cajas rígidas
- [ ] Control remoto con pilas
- [ ] Video de respaldo en dos USB y en la nube
- [ ] Ensayo completo cronometrado

### Una hora antes
- [ ] Mesa de ≥ 1.5 × 1.0 m; luz ≥ 300 lux sin contraluz (idealmente medida en una visita previa)
- [ ] Bases a 10–15 cm entre sí; miniaturas centradas
- [ ] App abierta en ambos: FPS ≥ 28, red y AI en verde
- [ ] Prueba rápida de tracking en cada marcador
- [ ] Wi-Fi verificado contra `generate_204`
- [ ] Otras apps cerradas, "No molestar" activado, A52s enchufado

### Durante
1. Si un marcador no trackea → pasar al otro dispositivo (el A52s puede estar en throttling).
2. Si el chatbot no tiene red → el cache responde y la interfaz lo indica; explicarlo.
3. Si todo falla → pasar al video de respaldo **diciendo que es una grabación**.

### No hacer
- Tocar la pantalla repetidamente si el tracking tarda (usar el control).
- Sacudir el teléfono: arruina la odometría visual-inercial.
- Cerrar y reabrir la app en medio de la demo: se pierde la sesión AR.
- Mostrar una funcionalidad que no está estable.
- Cambiar de Wi-Fi a mitad de la presentación.
- Desenchufar el A52s.

---

## 9. Video de respaldo

| Parámetro | Valor |
|---|---|
| Captura | `scrcpy` (espejo del A52s) + tomas por encima del hombro con otro dispositivo |
| Edición | DaVinci Resolve |
| Master | 1920×1080, 60 fps, **H.264** High (no H.265: proyectores viejos), MP4, ~20 Mbps, AAC 256 kbps |
| Marca de agua | `REC /// GRABACIÓN DE RESPALDO — SENDERO AR v1.0` |
| Música | Libre de derechos, con la licencia guardada |
| Copias | 2 USB + nube |

La marca de agua deja claro que es una grabación. Revisarlo con el tutor antes de la defensa.
