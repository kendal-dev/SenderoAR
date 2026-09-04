#!/usr/bin/env python3
"""
Sendero AR - validador de integridad del repositorio.

Corre SIN licencia de Unity y SIN abrir el editor: solo lee los archivos
versionados. Su premisa es la unica que importa para un proyecto de grado:
un clone limpio del repo tiene que producir la misma app que corre en la
maquina del autor.

Que comprueba (04/09/2026):
  1. Todo asset versionado tiene su .meta y todo .meta tiene su asset.
  2. No hay dos .meta con el mismo GUID.
  3. Las escenas declaradas en EditorBuildSettings existen en disco.
  4. Cada imagen de la XRReferenceImageLibrary apunta a una textura que
     SI esta versionada.  <-- el bug que motivo este script: la libreria
     guarda el GUID de la textura, no sus pixeles, y Art/ estaba excluida
     entera del repo.
  5. No hay componentes con script faltante (m_Script: {fileID: 0}).
  6. Cada AnchorImageName de los MonumentDataSO existe en la libreria (aviso).

Salida: 0 si no hay errores fatales, 1 si los hay.
"""

import os
import re
import struct
import subprocess
import sys
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ERRORS = []
WARNINGS = []
INFO = []


def error(msg):
    ERRORS.append(msg)


def warn(msg):
    WARNINGS.append(msg)


def info(msg):
    INFO.append(msg)


def tracked_files():
    """Archivos versionados. Es lo que veria un clone limpio."""
    out = subprocess.run(
        ["git", "ls-files"], cwd=ROOT, capture_output=True, text=True, check=True
    ).stdout
    return [line for line in out.splitlines() if line]


def unity_guid(guid_low, guid_high):
    """Convierte el par (m_GuidLow, m_GuidHigh) al GUID hex de los .meta."""
    b = struct.pack("<Q", guid_low) + struct.pack("<Q", guid_high)
    d1 = struct.unpack_from("<I", b, 0)[0]
    d2 = struct.unpack_from("<H", b, 4)[0]
    d3 = struct.unpack_from("<H", b, 6)[0]
    return f"{d1:08x}{d2:04x}{d3:04x}" + b[8:].hex()


def read(path):
    with open(os.path.join(ROOT, path), encoding="utf-8", errors="replace") as fh:
        return fh.read()


# --------------------------------------------------------------------------
def check_meta_pairs(files):
    assets = {f for f in files if f.startswith("Assets/") and not f.endswith(".meta")}
    metas = {f for f in files if f.startswith("Assets/") and f.endswith(".meta")}

    for asset in sorted(assets):
        if asset + ".meta" not in metas:
            error(f"[meta] Asset versionado sin .meta: {asset}")

    for meta in sorted(metas):
        target = meta[: -len(".meta")]
        if target in assets:
            continue
        # Un .meta de carpeta es legal: la carpeta existe pero Git no la versiona.
        if os.path.isdir(os.path.join(ROOT, target)):
            continue
        error(f"[meta] .meta huerfano (su asset no esta versionado): {meta}")

    info(f"[meta] {len(assets)} assets y {len(metas)} metas revisados.")


def collect_guids(files):
    guid_re = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.MULTILINE)
    by_guid = defaultdict(list)
    for meta in [f for f in files if f.endswith(".meta")]:
        match = guid_re.search(read(meta))
        if match:
            by_guid[match.group(1)].append(meta)

    for guid, owners in sorted(by_guid.items()):
        if len(owners) > 1:
            error(f"[guid] GUID duplicado {guid}: {', '.join(owners)}")

    info(f"[guid] {len(by_guid)} GUIDs unicos.")
    return by_guid


def check_build_scenes(files):
    path = "ProjectSettings/EditorBuildSettings.asset"
    if path not in files:
        error(f"[build] Falta {path}")
        return
    scenes = re.findall(r"path:\s*(Assets/[^\s]+\.unity)", read(path))
    if not scenes:
        error("[build] EditorBuildSettings no declara ninguna escena.")
    for scene in scenes:
        if scene not in files:
            error(f"[build] Escena declarada en Build Settings y ausente del repo: {scene}")
    info(f"[build] {len(scenes)} escenas declaradas: {', '.join(scenes)}")


def check_reference_libraries(files, by_guid):
    libs = [f for f in files if f.endswith(".asset") and "m_Images:" in read(f)]
    if not libs:
        warn("[ar] No se encontro ninguna XRReferenceImageLibrary versionada.")
        return {}

    names_by_lib = {}
    for lib in libs:
        body = read(lib)
        entries = re.findall(
            r"m_SerializedTextureGuid:\s*\n\s*m_GuidLow:\s*(\d+)\s*\n\s*m_GuidHigh:\s*(\d+)"
            r"(?:.*?\n)*?\s*m_Name:\s*(\S+)",
            body,
        )
        found_names = []
        for low, high, name in entries:
            found_names.append(name)
            guid = unity_guid(int(low), int(high))
            if guid not in by_guid:
                error(
                    f"[ar] {lib}: la imagen '{name}' apunta a la textura GUID {guid}, "
                    "que NO esta versionada. Un clone limpio no puede compilar el "
                    "imgdb de ARCore y la app no reconoce ese marcador."
                )
        names_by_lib[lib] = found_names
        info(f"[ar] {lib}: {len(found_names)} imagen(es) -> {', '.join(found_names) or 'ninguna'}")
    return names_by_lib


def check_missing_scripts(files):
    targets = [f for f in files if f.endswith((".unity", ".prefab"))]
    for path in targets:
        body = read(path)
        hits = len(re.findall(r"m_Script:\s*\{fileID:\s*0\}", body))
        if hits:
            error(f"[script] {path}: {hits} componente(s) con script faltante (m_Script fileID 0).")
    info(f"[script] {len(targets)} escenas/prefabs revisados.")


def check_monument_anchors(files, names_by_lib):
    known = {name for names in names_by_lib.values() for name in names}
    monuments = [
        f for f in files
        if f.endswith(".asset") and "/Monuments/" in f
    ]
    for path in monuments:
        body = read(path)
        match = re.search(r"AnchorImageName:\s*(\S+)", body)
        if not match:
            warn(f"[data] {path}: sin AnchorImageName.")
            continue
        anchor = match.group(1)
        if anchor not in known:
            warn(
                f"[data] {path}: AnchorImageName '{anchor}' no existe en ninguna "
                "XRReferenceImageLibrary. Ese monumento nunca va a dispararse."
            )
    info(f"[data] {len(monuments)} monumentos revisados contra la libreria.")


# --------------------------------------------------------------------------
def main():
    files = tracked_files()
    check_meta_pairs(files)
    by_guid = collect_guids(files)
    check_build_scenes(files)
    names_by_lib = check_reference_libraries(files, by_guid)
    check_missing_scripts(files)
    check_monument_anchors(files, names_by_lib)

    print("=" * 68)
    print("Sendero AR - integridad del repositorio")
    print("=" * 68)
    for line in INFO:
        print("  ok   " + line)
    for line in WARNINGS:
        print("  AVISO " + line)
    for line in ERRORS:
        print("  ERROR " + line)
    print("-" * 68)
    print(f"{len(ERRORS)} error(es) fatal(es), {len(WARNINGS)} aviso(s).")
    return 1 if ERRORS else 0


if __name__ == "__main__":
    sys.exit(main())
