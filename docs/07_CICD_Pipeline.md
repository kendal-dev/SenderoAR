# 07 · CI/CD

> Destilado de `07_CICD_Pipeline.md` v2.0 (21/05/2026). Contrastado contra
> `.github/workflows/` y `Tools/validate_project.py` el 16/10/2026.
> Leyenda de marcas: ver [`README.md`](README.md).

---

## 1. Lo que corre hoy — ✅ en el repo

### `ci.yml` — integridad, sin licencia de Unity

Se dispara en push a `main`, `feat/**`, `fix/**`, en PR a `main` y a mano.

1. `python Tools/validate_project.py`, que comprueba:
   1. todo asset versionado tiene su `.meta`, y todo `.meta` su asset;
   2. no hay GUIDs duplicados;
   3. las escenas de `EditorBuildSettings` existen;
   4. **cada imagen de la `XRReferenceImageLibrary` apunta a una textura versionada**
      (el bug que costó el Sprint 1);
   5. no hay componentes con script faltante (`m_Script: {fileID: 0}`);
   6. cada `AnchorImageName` existe en la librería (aviso, no error).
2. Falla si `ProjectSettings/ProjectVersion.txt` deja de decir `2022.3.62f3`.

Hoy da **0 errores y 5 avisos** (los 5 marcadores que faltan).

### `build-android.yml` — build GameCI, **manual**

Solo `workflow_dispatch`. Usa `game-ci/unity-builder@v4` con caché de `Library/` y sube el
resultado como artefacto. Necesita tres secrets: `UNITY_LICENSE`, `UNITY_EMAIL`,
`UNITY_PASSWORD`. Es manual a propósito: un workflow que falla en cada push es peor que no
tenerlo.

> 🔁 El documento de mayo marcaba los secrets de Unity como "ya configurados en Sprint 0".
> No hay forma de comprobarlo desde el repo; si el build manual falla en la activación,
> faltan.

### Protección del repo

Secret Scanning + Push Protection activados en GitHub desde S0. Si un push se bloquea por un
secreto real: sacarlo del código, guardarlo en GitHub Secrets o en una variable de entorno,
y **reescribir la historia local antes de volver a empujar**. Si el secreto llegó a subirse,
rotarlo: borrarlo del historial no alcanza.

---

## 2. Lo que estaba planificado — 📐 diseño

| Pieza | Diseño | Depende de |
|---|---|---|
| Lint | `dotnet format whitespace --verify-no-changes`, `continue-on-error` | 🔁 No funciona tal cual: `.sln` y `.csproj` están en `.gitignore` (los genera Unity), así que en el CI no existen. Haría falta generarlos con Unity o usar otro linter |
| Tests en CI | `game-ci/unity-test-runner@v4`, `testMode: EditMode` | 🟡 Los tests ya existen (`SenderoAR.Tests.EditMode`, 60 casos, [`04_`](04_MVVM_Patterns.md) §2.2). Falta solo cargar los secrets de Unity |
| Build en cada push a `main` | AAB (`androidAppBundle: true`) | Secrets de Unity; costo en minutos |
| Distribución | Firebase App Distribution (`firebase appdistribution:distribute`) | Proyecto Firebase |
| iOS | Runner `macos-latest`, solo manual o con `[ios]` en el mensaje | Cuenta de Apple Developer |
| Firma release | Keystore en base64 en secrets | Recién cuando haga falta un build firmado |

### Secrets previstos

| Secret | Para qué |
|---|---|
| `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` | Activar Unity en el runner |
| `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASS`, `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASS` | Firma release |
| `FIREBASE_APP_DISTRIBUTION_TOKEN`, `FIREBASE_ANDROID_APP_ID` | Distribución a testers |

Hasta que exista un keystore real, el build usa el `debug.keystore` que genera Unity.

### Keystore release

```bash
keytool -genkey -v -keystore senderoar-release.keystore -alias senderoar \
  -keyalg RSA -keysize 2048 -validity 10000
base64 -i senderoar-release.keystore -o keystore.b64   # contenido → ANDROID_KEYSTORE_BASE64
```

**El keystore nunca entra al repo.** Perderlo significa no poder volver a publicar
actualizaciones de la misma app: guardar una copia fuera de la máquina.

En el workflow:

```yaml
- name: Decode keystore
  run: |
    echo "${{ secrets.ANDROID_KEYSTORE_BASE64 }}" | base64 --decode > release.keystore
    echo "ANDROID_KEYSTORE_PATH=$PWD/release.keystore" >> $GITHUB_ENV
```

### Tiempos esperados ⚠️ (estimados de mayo)

| Job | Primera vez | Con caché |
|---|---|---|
| Tests | ~25 min | ~8 min |
| Build Android | ~45 min | ~18 min |

---

## 3. Flujo de ramas — ✅ vigente

| Regla | Detalle |
|---|---|
| Rama principal | `main` |
| Ramas de trabajo | `feat/…`, `fix/…`, `docs/…` |
| Vida máxima | **2 semanas** sin mergear (`CLAUDE.md` §5 regla 3) |
| Merge | `--no-ff` |
| Commits | Conventional Commits (`feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `perf`, `ci`), cuerpo en español explicando el porqué |
| Antes del commit | `git diff` revisado; nada vacío (regla 2) |
| Antes del push | `python Tools/validate_project.py` → 0 errores (regla 1) |

🔁 El plan de mayo decía "ramas de < 2 días" y "5 commits por día como guía". Ninguna de las
dos se cumplió y la segunda produjo commits vacíos; quedaron reemplazadas por las reglas de
arriba.

### Cierre de una rama

```bash
git checkout main
git pull origin main
git merge --no-ff feat/<rama> -m "merge: <descripción>"
git push origin main
git branch -d feat/<rama>
git push origin --delete feat/<rama>
```
