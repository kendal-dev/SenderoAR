# 05 · Narración trilingüe (TTS)

> Destilado de `05_TTS_Pipeline.md` v2.0 (21/05/2026). Contrastado contra
> `IAudioPlaybackService`, `AudioCue`, `AddressableAudioReference` y `LanguageCode` el
> 16/10/2026. Leyenda de marcas: ver [`README.md`](README.md).
>
> **Todo es 📐 salvo los tipos de §6.1.** Addressables no está instalado.

---

## 1. Proveedores

Ningún proveedor sirve bien para los tres idiomas; uno por idioma:

| Idioma | `LanguageCode` | Proveedor | Voz | Motivo |
|---|---|---|---|---|
| Español (Bolivia) | `EsBO` | Azure Neural TTS | `es-BO-MarceloNeural` (o `es-BO-SofiaNeural`) | Único con voz boliviana nativa ⚠️ |
| Inglés (EE. UU.) | `EnUS` | ElevenLabs Multilingual v3 | "Bill L. Oxley" (narrador de audiolibros) | Narración documental |
| Portugués (Brasil) | `PtBR` | ElevenLabs Multilingual v3 | "Dara de Alcantara" o "Ricardo Vianna" | Fonética pt-BR natural |

**5 monumentos × 3 idiomas = 15 narraciones**, ~90 s cada una (~130 palabras/min,
~13 500 caracteres en total).

### Costos y licencias ⚠️ (precios de mayo de 2026)

- Azure: capa gratuita de 500 000 caracteres/mes → el español sale a **0**.
- ElevenLabs Starter: **5 USD/mes**, 30 000 caracteres. Abrir la cuenta **recién cuando se
  vayan a generar los audios**, para no pagar meses sin uso.
- ⚠️ **El plan Free de ElevenLabs prohíbe uso comercial y exige atribución.** No usarlo ni
  para pruebas que vayan a quedar en el build. Con plan pago, los audios generados quedan
  con derechos aunque se cancele después.
- **Prohibido clonar voces** de personas reales sin consentimiento escrito.

---

## 2. Por qué WAV y no OGG

🔁 El diseño original era empaquetar `.ogg`. iOS no reproduce Vorbis de forma nativa. Solución:

1. Se generan **WAV PCM** (44.1 kHz, 16 bit, mono).
2. Se importan a Unity como `AudioClip`.
3. Unity recomprime **por plataforma**: Vorbis en Android, **AAC** en iOS.
4. Se empaquetan con **Addressables**, un grupo por idioma.
5. El grupo pt-BR se sirve desde **Firebase Hosting** (no Storage), con CDN.

> El `<remarks>` de `IAudioPlaybackService` todavía dice "OGG Vorbis 64-96 kbps": se refiere
> al formato comprimido que llega al dispositivo Android, no al archivo fuente. Al
> implementar, actualizar ese comentario.

---

## 3. Pipeline batch

```
[1] Guiones en Markdown versionados
      scripts/mon_01_templo_es.md, …_en.md, …_pt.md   (15 archivos)
[2] Script Python asíncrono
      ES → Azure REST       → raw_es.wav
      EN → ElevenLabs API   → raw_en.wav
      PT → ElevenLabs API   → raw_pt.wav
[3] Post-producción (pydub + FFmpeg)
      recorte de silencios · normalización a -16 LUFS · WAV 44.1 kHz mono
[4] Assets/_SenderoAR/Art/Audio/Source/   ← fuera de Git
[5] Importer por plataforma (§6.3)
[6] Grupos de Addressables por idioma
[7] Build de Addressables → ServerData/<plataforma>/
[8] firebase deploy --only hosting   (solo pt-BR)
```

> Los **guiones** (texto) sí conviene versionarlos: son contenido académico revisable. Los
> **WAV** no (regla 7 de `CLAUDE.md`).

### Script de referencia

```python
import os, asyncio
from pydub import AudioSegment
import azure.cognitiveservices.speech as speechsdk
from elevenlabs import ElevenLabs

AZURE_KEY    = os.getenv("AZURE_TTS_KEY")      # nunca en el código
AZURE_REGION = os.getenv("AZURE_TTS_REGION")
ELEVEN_KEY   = os.getenv("ELEVENLABS_KEY")
RAW_DIR      = "./pipeline/raw"
FINAL_DIR    = "./Assets/_SenderoAR/Art/Audio/Source"
TARGET_LUFS  = -16.0

async def generate_azure(ssml, out):
    cfg = speechsdk.SpeechConfig(subscription=AZURE_KEY, region=AZURE_REGION)
    synth = speechsdk.SpeechSynthesizer(cfg, speechsdk.audio.AudioOutputConfig(filename=out))
    return synth.speak_ssml_async(ssml).get().reason == speechsdk.ResultReason.SynthesizingAudioCompleted

async def generate_eleven(text, voice_id, out):
    stream = ElevenLabs(api_key=ELEVEN_KEY).text_to_speech.convert(
        voice_id=voice_id, output_format="pcm_44100", text=text,
        model_id="eleven_multilingual_v3")
    with open(out, "wb") as f:
        for chunk in stream:
            if chunk: f.write(chunk)
    return True

def master(raw, final_name):
    audio = AudioSegment.from_file(raw)
    audio = audio.strip_silence(silence_len=200, silence_thresh=-50, padding=50)
    audio = audio.apply_gain(TARGET_LUFS - audio.dBFS)   # aproximación por dBFS, no LUFS real
    audio.export(os.path.join(FINAL_DIR, final_name), format="wav",
                 parameters=["-ar", "44100", "-ac", "1"])
    os.remove(raw)
```

> `apply_gain(TARGET_LUFS - dBFS)` normaliza por volumen RMS, **no** por LUFS. Si hace falta
> LUFS real, usar el filtro `loudnorm` de FFmpeg. Verificar con
> `ffmpeg -i x.wav -af "ebur128=peak=true" -f null -`.

Nombre de salida: `<MonumentId>_<código ISO>.wav` → `mon_01_templo_es-BO.wav`.

---

## 4. Pronunciación (SSML / alias)

| Término | Idioma | Indicación |
|---|---|---|
| Chiquitos | EN | "Chee-kee-tos" |
| Penoca | EN | "Pe-no-ka" |
| Hans Roth | EN/PT | "Hans Roht" |
| Bartolomé de Mora | EN | "Bar-to-lo-may de Mo-rah" |
| Miserere | EN | Latín: "Mi-se-re-re" |
| Padre Felipe | EN/PT | Mantener pronunciación española |

```xml
<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="es-BO">
  <voice name="es-BO-MarceloNeural">
    <prosody rate="-5%">
      Bienvenido al Templo de
      <phoneme alphabet="ipa" ph="san xoˈse de tʃiˈkitos">San José de Chiquitos</phoneme>.
    </prosody>
  </voice>
</speak>
```

> Los guiones heredan los datos de [`01_`](01_Monuments_Database.md): **no grabar ningún
> dato ⚠️ sin verificarlo**. Regrabar 15 audios por una fecha mal dicha cuesta más que
> verificarla antes.

---

## 5. Peso y distribución

| Idioma | Peso estimado por idioma (Vorbis Q60) | Dónde va |
|---|---|---|
| es-BO | ~3.5 MB | Dentro del APK |
| en-US | ~3.5 MB | Dentro del APK |
| pt-BR | ~3.5 MB | **Descarga bajo demanda** desde Firebase Hosting |

🔁 El contexto de mayo decía "~48 MB descargable" para pt-BR; el cálculo del pipeline da
~3.5 MB. Con 15 clips de 90 s, ~3.5 MB por idioma es lo coherente.

Antes de una demo, descargar el paquete pt-BR con buena conexión (queda en caché).

---

## 6. Integración con Unity

### 6.1. Tipos — ✅ en el repo

```csharp
public enum LanguageCode { EsBO = 0, EnUS = 1, PtBR = 2 }

public readonly struct AudioCue                 // qué narración reproducir
{
    public string MonumentIdentifier { get; }
    public LanguageCode Language { get; }
}

[Serializable] public class AddressableAudioReference   // en MonumentDataSO.NarrationClips
{
    public LanguageCode Language;
    public string AddressableKey;
}

public enum PlaybackEventKind { Started, Paused, Resumed, Completed, Stopped, Failed }
```

```csharp
public interface IAudioPlaybackService
{
    bool IsPlaying { get; }                       // no incluye Paused
    // + el cue cargado actualmente (o null)
    event Action<AudioCue, PlaybackEventKind> PlaybackStateChanged;   // main thread
    Task PlayAsync(AudioCue cue, CancellationToken cancellationToken = default);
    Task PauseAsync();
    Task ResumeAsync();
    Task StopAsync();
}
```

Regla del contrato: **un solo audio a la vez**. `PlayAsync` con otro sonando emite primero
`Stopped` del anterior y después `Started` del nuevo.

> 🔁 El ejemplo de mayo usaba `PlayNarrationAsync(string, LanguageCode)`,
> `StopNarration()`, `UnloadResources()`, `IsCurrentlyPlaying` y
> `LanguageCode.SpanishBolivia` + `.ToCode()`. **Nada de eso existe.** Mandan las firmas de
> arriba. Para el código ISO (`"es-BO"`), agregar un método de extensión en `Models/`.

### 6.2. Implementación — 📐 referencia

```csharp
public sealed class AddressableAudioPlaybackService : IAudioPlaybackService, IDisposable
{
    private readonly AudioSource _source;
    private AsyncOperationHandle<AudioClip> _handle;

    public async Task PlayAsync(AudioCue cue, CancellationToken ct = default)
    {
        await StopAsync();                    // emite Stopped si había algo
        Release();

        // La key sale de MonumentDataSO.NarrationClips[i].AddressableKey,
        // no se arma a mano con strings.
        _handle = Addressables.LoadAssetAsync<AudioClip>(KeyFor(cue));
        var clip = await _handle.Task;        // asíncrono: no bloquea el main thread
        ct.ThrowIfCancellationRequested();

        if (clip == null) { Raise(cue, PlaybackEventKind.Failed); return; }
        _source.clip = clip;
        _source.Play();
        Raise(cue, PlaybackEventKind.Started);
    }

    private void Release()
    {
        if (_handle.IsValid()) Addressables.Release(_handle);
    }

    public void Dispose() => Release();
    // PauseAsync / ResumeAsync / StopAsync / Raise / KeyFor …
}
```

`AudioSource`: `spatialBlend = 0` (2D, narrativo) · `loop = false` · `playOnAwake = false`
· `volume = 0.85`. Un solo `AudioListener` en la escena.

Cambio de idioma: el ViewModel escucha `ILocalizationService.LanguageChanged` y llama a
`PlayAsync` con un `AudioCue` nuevo; el contrato ya garantiza que el anterior se detiene.

### 6.3. Importer de audio (en cada `.wav`)

```
Default:   Decompress On Load · Vorbis · Quality 70
Android:   Compressed In Memory · Vorbis · Quality 60
iOS:       Compressed In Memory · AAC    · Quality 60
```

### 6.4. Grupos de Addressables

| Grupo | Build path | Load path | Compresión |
|---|---|---|---|
| `AudioPack_es-BO` | `[Addressables.BuildPath]/[BuildTarget]` | `{Addressables.RuntimePath}/[BuildTarget]` | LZ4 |
| `AudioPack_en-US` | (igual) | (igual) | LZ4 |
| `AudioPack_pt-BR` | `ServerData/[BuildTarget]` | URL de Firebase Hosting + `/audio/[BuildTarget]` | LZ4 |

### 6.5. Descarga de pt-BR

```csharp
public async Task EnsurePortugueseAvailable(IProgress<float> progress)
{
    var locations = await Addressables.LoadResourceLocationsAsync("AudioPack_pt-BR", typeof(AudioClip)).Task;
    if (locations.Count == 0) return;

    var download = Addressables.DownloadDependenciesAsync("AudioPack_pt-BR", autoReleaseHandle: false);
    while (!download.IsDone)
    {
        progress?.Report(download.PercentComplete);
        await Task.Yield();
    }
    Addressables.Release(download);
}
```

### 6.6. Firebase Hosting

```json
{
  "hosting": {
    "public": "ServerData",
    "ignore": ["firebase.json", "**/.*"],
    "headers": [{
      "source": "**/*.bundle",
      "headers": [{ "key": "Cache-Control", "value": "max-age=2592000" }]
    }]
  }
}
```

`max-age=2592000` = 30 días de caché. La capa gratuita ⚠️ (10 GB, 360 MB/día) sobra para un
paquete de ~3.5 MB.

---

## 7. Checklist antes de producir los audios

- [ ] Guiones ES/EN/PT escritos **con datos verificados** y revisados por un hablante nativo
- [ ] Cuenta Azure con clave en variable de entorno
- [ ] ElevenLabs en plan pago (no Free)
- [ ] Voces probadas con un párrafo por idioma
- [ ] SSML probado con los términos de §4
- [ ] Script probado con **un** audio antes del lote
- [ ] FFmpeg y pydub instalados
- [ ] Carpeta `Art/Audio/Source/` creada (queda fuera de Git)
- [ ] Paquete Addressables instalado y grupos creados
- [ ] Un solo `AudioListener` en la escena
