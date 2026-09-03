from __future__ import annotations

import io
import zipfile
from enum import Enum

from .errors import PackageError

MAX_ENTRY_SIZE = 64 * 1024 * 1024
MAX_PACKAGE_SIZE = 256 * 1024 * 1024


class OfficeFormat(Enum):
    UNKNOWN = "unknown"
    XLSX = "xlsx"
    DOCX = "docx"
    PPTX = "pptx"


class OfficePackage:
    def __init__(self, data: bytes) -> None:
        try:
            self._archive = zipfile.ZipFile(io.BytesIO(data))
        except (OSError, zipfile.BadZipFile) as error:
            raise PackageError("input is not a valid Office ZIP package") from error
        self._validate()

    def _validate(self) -> None:
        total_size = 0
        for entry in self._archive.infolist():
            normalized = entry.filename.replace("\\", "/")
            parts = normalized.split("/")
            if normalized.startswith("/") or ".." in parts:
                raise PackageError(f"unsafe package path: {entry.filename}")
            if entry.flag_bits & 0x1:
                raise PackageError("encrypted Office packages are not supported")
            if entry.file_size > MAX_ENTRY_SIZE:
                raise PackageError(f"package entry is too large: {entry.filename}")
            total_size += entry.file_size
            if total_size > MAX_PACKAGE_SIZE:
                raise PackageError("Office package expands beyond the safety limit")

    @property
    def format(self) -> OfficeFormat:
        names = (entry.filename.replace("\\", "/") for entry in self._archive.infolist())
        roots = {name.split("/", 1)[0] for name in names if "/" in name}
        if "word" in roots:
            return OfficeFormat.DOCX
        if "xl" in roots:
            return OfficeFormat.XLSX
        if "ppt" in roots:
            return OfficeFormat.PPTX
        return OfficeFormat.UNKNOWN

    def read(self, name: str) -> bytes | None:
        try:
            return self._archive.read(name)
        except KeyError:
            return None
        except (OSError, RuntimeError, zipfile.BadZipFile) as error:
            raise PackageError(f"cannot read package entry: {name}") from error


def detect_office_format(data: bytes) -> OfficeFormat:
    return OfficePackage(data).format
