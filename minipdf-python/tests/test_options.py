import math

import pytest

from minipdf import ConversionOptions, InvalidInputError, PageSize


def test_exposes_standard_page_sizes() -> None:
    assert PageSize(595.28, 841.89) == PageSize.A4
    assert PageSize(612.0, 792.0) == PageSize.LETTER


@pytest.mark.parametrize(
    ("width", "height"),
    [(0, 10), (-1, 10), (10, 0), (10, -1), (math.inf, 10), (10, math.nan)],
)
def test_rejects_invalid_page_dimensions(width: float, height: float) -> None:
    with pytest.raises(InvalidInputError, match="positive finite"):
        PageSize(width, height)


def test_conversion_options_default_to_document_page_size() -> None:
    assert ConversionOptions().page_size is None
    assert ConversionOptions(page_size=PageSize.LETTER).page_size == PageSize.LETTER
