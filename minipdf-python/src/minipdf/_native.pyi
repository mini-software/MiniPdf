# Type stub for the optional native extension `minipdf._native`, built from
# `minipdf-python/native`. At runtime the module is imported dynamically and
# is absent unless the extension has been built and installed.

def convert_bytes_to_pdf(
    data: bytes,
    page_size: tuple[float, float] | None = None,
) -> bytes: ...
def convert_to_pdf(
    input_path: str,
    output_path: str,
    page_size: tuple[float, float] | None = None,
) -> None: ...
def convert_to_pdf_bytes(
    input_path: str,
    page_size: tuple[float, float] | None = None,
) -> bytes: ...
def register_font(name: str, data: bytes) -> None: ...
def registered_fonts() -> list[tuple[str, bytes]]: ...
