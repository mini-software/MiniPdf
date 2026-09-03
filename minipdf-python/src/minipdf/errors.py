class MiniPdfError(Exception):
    """Base exception for MiniPdf conversion failures."""


class InvalidInputError(MiniPdfError, ValueError):
    """Raised when an input or conversion option is invalid."""


class UnsupportedFormatError(MiniPdfError):
    """Raised when an Office package format is not supported."""


class PackageError(MiniPdfError):
    """Raised when an Office ZIP package is malformed or unsafe."""
