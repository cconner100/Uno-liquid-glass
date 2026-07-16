#!/usr/bin/env python3
"""Merge Windows-only NuGet assets into packages built on macOS."""

from __future__ import annotations

import argparse
import shutil
import tempfile
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path


def merge_nuspec(primary: Path, supplement: Path) -> None:
    namespace = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"
    ET.register_namespace("", namespace)
    primary_tree = ET.parse(primary)
    supplement_tree = ET.parse(supplement)
    primary_dependencies = primary_tree.find(f".//{{{namespace}}}dependencies")
    supplement_dependencies = supplement_tree.find(f".//{{{namespace}}}dependencies")

    if primary_dependencies is not None and supplement_dependencies is not None:
        existing = {
            group.get("targetFramework")
            for group in primary_dependencies.findall(f"{{{namespace}}}group")
        }
        for group in supplement_dependencies.findall(f"{{{namespace}}}group"):
            if group.get("targetFramework") not in existing:
                primary_dependencies.append(group)

    primary_tree.write(primary, encoding="utf-8", xml_declaration=True)


def merge_package(primary: Path, supplement: Path, output: Path) -> None:
    with tempfile.TemporaryDirectory() as temporary:
        root = Path(temporary)
        primary_root = root / "primary"
        supplement_root = root / "supplement"
        with zipfile.ZipFile(primary) as archive:
            archive.extractall(primary_root)
        with zipfile.ZipFile(supplement) as archive:
            archive.extractall(supplement_root)

        for source in supplement_root.rglob("*"):
            if source.is_file():
                destination = primary_root / source.relative_to(supplement_root)
                if not destination.exists():
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copy2(source, destination)

        primary_nuspec = next(primary_root.glob("*.nuspec"))
        supplement_nuspec = next(supplement_root.glob("*.nuspec"))
        merge_nuspec(primary_nuspec, supplement_nuspec)

        output.parent.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as archive:
            for source in sorted(primary_root.rglob("*")):
                if source.is_file():
                    archive.write(source, source.relative_to(primary_root).as_posix())


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("primary", type=Path)
    parser.add_argument("supplement", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()

    for primary in sorted(args.primary.glob("*.nupkg")):
        supplement = args.supplement / primary.name
        if not supplement.exists():
            raise FileNotFoundError(f"Missing supplemental package: {supplement}")
        merge_package(primary, supplement, args.output / primary.name)


if __name__ == "__main__":
    main()
