from __future__ import annotations

import math
from dataclasses import dataclass
from typing import ClassVar

from .errors import InvalidInputError


@dataclass(frozen=True, slots=True)
class PageSize:
    """PDF page dimensions measured in points."""

    width: float
    height: float

    A4: ClassVar[PageSize]
    LETTER: ClassVar[PageSize]

    def __post_init__(self) -> None:
        if (
            not math.isfinite(self.width)
            or not math.isfinite(self.height)
            or self.width <= 0
            or self.height <= 0
        ):
            raise InvalidInputError("page width and height must be positive finite values")


PageSize.A4 = PageSize(595.28, 841.89)
PageSize.LETTER = PageSize(612.0, 792.0)


@dataclass(frozen=True, slots=True)
class ConversionOptions:
    """Options shared by all supported Office converters."""

    page_size: PageSize | None = None
