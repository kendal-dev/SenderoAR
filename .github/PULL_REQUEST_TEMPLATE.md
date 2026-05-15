# 📝 Pull Request — Sendero AR

## 🎯 Descripción

<!-- Explicá qué hace este PR. Sé específico. -->

## 🔗 Issue Relacionado

<!-- Si este PR resuelve un issue, linkealo: Closes #123 -->

Closes #

## 📊 Tipo de Cambio

<!-- Marcá con [x] la opción correcta -->

- [ ] 🐛 **fix**: Corrección de bug (no rompe nada existente)
- [ ] ✨ **feat**: Nueva característica (no rompe nada existente)
- [ ] 💥 **breaking**: Cambio que rompe compatibilidad
- [ ] 📚 **docs**: Solo cambios en documentación
- [ ] 🎨 **style**: Cambios de formato (espacios, comas, sin afectar lógica)
- [ ] ♻️ **refactor**: Refactorización sin cambio de comportamiento
- [ ] ⚡ **perf**: Mejora de rendimiento
- [ ] 🧪 **test**: Agregar o actualizar tests
- [ ] 🔧 **chore**: Cambios en build, dependencias, CI/CD

## 🧪 ¿Cómo se ha probado?

<!-- Describí los tests que corriste. Sé específico. -->

- [ ] Tests unitarios pasan localmente
- [ ] Compila sin warnings en Unity
- [ ] Probado en device físico (mencioná modelo): ___________
- [ ] Probado en Editor (Play Mode)
- [ ] Probado en XR Simulation

## 📸 Capturas / Videos

<!-- Si aplica, agregá imágenes o videos del cambio funcionando -->

## ✅ Checklist de Self-Review

### Código

- [ ] Mi código sigue las convenciones del proyecto (MVVM puro, sin frameworks externos)
- [ ] He revisado mi propio código antes de pedir review
- [ ] He comentado código complejo, especialmente las decisiones arquitectónicas (el "por qué", no el "qué")
- [ ] Los ViewModels son POCOs y no heredan de MonoBehaviour
- [ ] No hay magic strings — uso `nameof()` o `[CallerMemberName]`
- [ ] Toda suscripción a eventos está en un `CompositeDisposable`
- [ ] No hay `Debug.Log` olvidados en código de producción
- [ ] No hay TODOs sin link a un issue

### Performance

- [ ] No introduzco asignaciones de memoria en bucles `Update()`
- [ ] No uso `GetComponent` en cada frame (cachear referencias en `Awake/Start`)
- [ ] No agrego más de 100 draw calls al frame
- [ ] No supero los 30,000 triángulos por modelo

### Seguridad

- [ ] No hay API keys hardcodeadas en el código
- [ ] No subí archivos `google-services.json` ni `GoogleService-Info.plist` al repo
- [ ] No subí ningún archivo `.keystore`, `.p8`, `.p12` o `.mobileprovision`
- [ ] Las llamadas a Firebase pasan por `ChiquitaniaHeritageChatService`

### Documentación

- [ ] He actualizado el README si fue necesario
- [ ] He actualizado los documentos en `/docs/` si cambié arquitectura
- [ ] Los commits siguen Conventional Commits (`feat:`, `fix:`, `docs:`, etc.)

## 🚨 Notas para el Reviewer

<!-- ¿Hay algo específico en lo que querés que el reviewer ponga atención? -->

## 🔄 Plan de Rollback

<!-- Si este cambio rompe algo en producción, ¿cómo lo revertimos? -->

- [ ] Cambio reversible con `git revert`
- [ ] Cambio requiere acción manual: ___________
