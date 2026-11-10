#!/usr/bin/env python3
"""Une los informes cobertura (coverlet.collector) de varias suites y calcula la cobertura de líneas del código de producción.

Uso: python scripts/coverage-merge.py <directorio-con-un-subdirectorio-por-suite>
Solo biblioteca estándar. Cada línea cuenta como cubierta si alguna suite la ejecutó (unión). Se excluye el código de pruebas y el
generado. No consulta ni escribe en ningún servicio: solo lee los XML locales (docs/quality/coverage-baseline.md, pm-056).
"""
import collections
import glob
import os
import sys
import xml.etree.ElementTree as ET


def main(root: str) -> int:
    union: dict[tuple[str, str], dict[int, int]] = {}
    per_suite: dict[str, dict[tuple[str, str], dict[int, int]]] = collections.defaultdict(dict)
    for report in glob.glob(os.path.join(root, "*", "*", "coverage.cobertura.xml")):
        suite = os.path.basename(os.path.dirname(os.path.dirname(report)))
        for package in ET.parse(report).iter("package"):
            project = package.get("name") or ""
            if not project.startswith("PetMate.") or "Tests" in project:
                continue
            for cls in package.iter("class"):
                name = (cls.get("filename") or "").replace("\\", "/")
                if "/obj/" in name or name.endswith(".g.cs") or "LoggerMessage" in name:
                    continue
                merged = union.setdefault((project, name), {})
                own = per_suite[suite].setdefault((project, name), {})
                for line in cls.iter("line"):
                    number, hits = int(line.get("number")), int(line.get("hits"))
                    merged[number] = max(merged.get(number, 0), hits)
                    own[number] = max(own.get(number, 0), hits)

    def totals(files: dict) -> tuple[int, int]:
        total = sum(len(lines) for lines in files.values())
        covered = sum(1 for lines in files.values() for hits in lines.values() if hits > 0)
        return covered, total

    if not union:
        print("No se encontraron informes de cobertura.")
        return 1

    by_project: dict[str, dict] = collections.defaultdict(dict)
    for (project, name), lines in union.items():
        by_project[project][name] = lines
    grand_covered = grand_total = 0
    print("Cobertura de líneas (unión de las suites):")
    for project in sorted(by_project):
        covered, total = totals(by_project[project])
        grand_covered, grand_total = grand_covered + covered, grand_total + total
        print(f"  {project}: {covered}/{total} = {100 * covered / total:.1f}%")
    print(f"  TOTAL: {grand_covered}/{grand_total} = {100 * grand_covered / grand_total:.1f}%")
    print("Por suite (sobre el total de líneas):")
    for suite in sorted(per_suite):
        covered, _ = totals(per_suite[suite])
        print(f"  {suite}: {covered}/{grand_total} = {100 * covered / grand_total:.1f}%")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1] if len(sys.argv) > 1 else "coverage"))
