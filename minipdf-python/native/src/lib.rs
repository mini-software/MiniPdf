//! PyO3 bindings over the Rust MiniPdf engine.
//!
//! The extension is installed as the `minipdf._native` submodule. The Python
//! package prefers this module for conversion and falls back to the pure
//! Python renderers when it is not importable.

use std::path::Path;

use pyo3::exceptions::PyValueError;
use pyo3::prelude::*;
use pyo3::types::PyList;

use minipdf::ConversionOptions as CoreOptions;
use minipdf::PageSize as CorePageSize;

fn to_py_err(error: minipdf::MiniPdfError) -> PyErr {
    PyValueError::new_err(error.to_string())
}

fn to_options(page_size: Option<(f64, f64)>) -> PyResult<CoreOptions> {
    let Some((width, height)) = page_size else {
        return Ok(CoreOptions::default());
    };
    let page_size = CorePageSize::new(width as f32, height as f32).map_err(to_py_err)?;
    Ok(CoreOptions {
        page_size: Some(page_size),
    })
}

fn prepare_fonts() -> PyResult<()> {
    minipdf::register_system_fonts().map_err(to_py_err)
}

#[pyfunction]
#[pyo3(signature = (data, page_size=None))]
fn convert_bytes_to_pdf(data: &[u8], page_size: Option<(f64, f64)>) -> PyResult<Vec<u8>> {
    prepare_fonts()?;
    minipdf::convert_bytes_to_pdf_with_options(data, &to_options(page_size)?).map_err(to_py_err)
}

#[pyfunction]
#[pyo3(signature = (input_path, output_path, page_size=None))]
fn convert_to_pdf(
    input_path: &str,
    output_path: &str,
    page_size: Option<(f64, f64)>,
) -> PyResult<()> {
    prepare_fonts()?;
    minipdf::convert_to_pdf_with_options(
        Path::new(input_path),
        Path::new(output_path),
        &to_options(page_size)?,
    )
    .map_err(to_py_err)
}

#[pyfunction]
#[pyo3(signature = (input_path, page_size=None))]
fn convert_to_pdf_bytes(input_path: &str, page_size: Option<(f64, f64)>) -> PyResult<Vec<u8>> {
    prepare_fonts()?;
    minipdf::convert_to_pdf_bytes_with_options(Path::new(input_path), &to_options(page_size)?)
        .map_err(to_py_err)
}

#[pyfunction]
fn register_font(name: &str, data: &[u8]) {
    minipdf::register_font(name.to_owned(), data.to_vec());
}

#[pyfunction]
fn registered_fonts(py: Python<'_>) -> PyResult<Py<PyList>> {
    let list = PyList::empty(py);
    for font in minipdf::registered_fonts() {
        list.append((font.name, font.data))?;
    }
    Ok(list.into())
}

#[pymodule]
fn _native(_py: Python<'_>, module: &Bound<'_, PyModule>) -> PyResult<()> {
    module.add_function(wrap_pyfunction!(convert_bytes_to_pdf, module)?)?;
    module.add_function(wrap_pyfunction!(convert_to_pdf, module)?)?;
    module.add_function(wrap_pyfunction!(convert_to_pdf_bytes, module)?)?;
    module.add_function(wrap_pyfunction!(register_font, module)?)?;
    module.add_function(wrap_pyfunction!(registered_fonts, module)?)?;
    Ok(())
}
