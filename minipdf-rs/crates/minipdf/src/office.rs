use std::io::{Read, Seek};

use zip::ZipArchive;

use crate::Result;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum OfficeFormat {
    Unknown,
    Xlsx,
    Docx,
}

pub(crate) fn detect_office_format<R: Read + Seek>(reader: R) -> Result<OfficeFormat> {
    let mut archive = ZipArchive::new(reader)?;
    for index in 0..archive.len() {
        let file = archive.by_index(index)?;
        let name = file.name().replace('\\', "/");
        if name.starts_with("word/") {
            return Ok(OfficeFormat::Docx);
        }
        if name.starts_with("xl/") {
            return Ok(OfficeFormat::Xlsx);
        }
    }
    Ok(OfficeFormat::Unknown)
}
