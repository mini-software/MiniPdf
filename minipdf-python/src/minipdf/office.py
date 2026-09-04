from __future__ import annotations

import io
import posixpath
import xml.etree.ElementTree as ET
import zipfile
from dataclasses import dataclass
from enum import Enum
from urllib.parse import unquote, urlsplit

from .errors import PackageError

MAX_ENTRY_SIZE = 64 * 1024 * 1024
MAX_PACKAGE_SIZE = 256 * 1024 * 1024


class OfficeFormat(Enum):
    UNKNOWN = "unknown"
    XLSX = "xlsx"
    DOCX = "docx"
    PPTX = "pptx"


@dataclass(frozen=True, slots=True)
class OfficeRelationship:
    relationship_type: str
    target: str


class OfficePackage:
    def __init__(self, data: bytes) -> None:
        try:
            self._archive = zipfile.ZipFile(io.BytesIO(data))
        except (OSError, zipfile.BadZipFile) as error:
            raise PackageError("input is not a valid Office ZIP package") from error
        self._validate()
        self._entries = {
            entry.filename.replace("\\", "/"): entry for entry in self._archive.infolist()
        }

    def _validate(self) -> None:
        total_size = 0
        normalized_names: set[str] = set()
        for entry in self._archive.infolist():
            normalized = entry.filename.replace("\\", "/")
            parts = normalized.split("/")
            if normalized.startswith("/") or ".." in parts:
                raise PackageError(f"unsafe package path: {entry.filename}")
            if normalized in normalized_names:
                raise PackageError(f"duplicate package path: {entry.filename}")
            normalized_names.add(normalized)
            if entry.flag_bits & 0x1:
                raise PackageError("encrypted Office packages are not supported")
            if entry.file_size > MAX_ENTRY_SIZE:
                raise PackageError(f"package entry is too large: {entry.filename}")
            total_size += entry.file_size
            if total_size > MAX_PACKAGE_SIZE:
                raise PackageError("Office package expands beyond the safety limit")

    @property
    def format(self) -> OfficeFormat:
        roots = {name.split("/", 1)[0] for name in self._entries if "/" in name}
        if "word" in roots:
            return OfficeFormat.DOCX
        if "xl" in roots:
            return OfficeFormat.XLSX
        if "ppt" in roots:
            return OfficeFormat.PPTX
        return OfficeFormat.UNKNOWN

    @property
    def names(self) -> tuple[str, ...]:
        return tuple(self._entries)

    def read(self, name: str) -> bytes | None:
        entry = self._entries.get(name.replace("\\", "/"))
        if entry is None:
            return None
        try:
            return self._archive.read(entry)
        except KeyError:
            return None
        except (OSError, RuntimeError, zipfile.BadZipFile) as error:
            raise PackageError(f"cannot read package entry: {name}") from error

    def relationships(self, source_name: str) -> dict[str, OfficeRelationship]:
        directory, filename = posixpath.split(source_name)
        relationships_name = posixpath.join(directory, "_rels", f"{filename}.rels")
        data = self.read(relationships_name)
        if data is None:
            return {}
        try:
            root = ET.fromstring(data)
        except ET.ParseError as error:
            raise PackageError(f"{relationships_name} is malformed") from error

        relationships: dict[str, OfficeRelationship] = {}
        for node in root.iter():
            if node.tag.rsplit("}", 1)[-1] != "Relationship":
                continue
            relationship_id = node.get("Id")
            relationship_type = node.get("Type")
            target = node.get("Target")
            if not relationship_id or not relationship_type or not target:
                continue
            if node.get("TargetMode", "").lower() == "external":
                continue
            target_path = unquote(urlsplit(target.replace("\\", "/")).path)
            if target_path.startswith("/"):
                resolved = posixpath.normpath(target_path.lstrip("/"))
            else:
                resolved = posixpath.normpath(posixpath.join(directory, target_path))
            if resolved == ".." or resolved.startswith("../"):
                raise PackageError(f"unsafe relationship target: {target}")
            relationships[relationship_id] = OfficeRelationship(relationship_type, resolved)
        return relationships


def detect_office_format(data: bytes) -> OfficeFormat:
    return OfficePackage(data).format
