use std::fs;
use std::path::PathBuf;
use std::sync::{Mutex, OnceLock};

use crate::Result;

static SYSTEM_FONTS_REGISTERED: OnceLock<Mutex<bool>> = OnceLock::new();

pub fn register_system_fonts() -> Result<()> {
    let state = SYSTEM_FONTS_REGISTERED.get_or_init(|| Mutex::new(false));
    let mut registered = state
        .lock()
        .expect("system font registration lock poisoned");
    if *registered {
        return Ok(());
    }

    for path in system_fallback_font_paths()
        .into_iter()
        .filter(|path| path.is_file())
    {
        let name = path
            .file_stem()
            .and_then(|name| name.to_str())
            .unwrap_or("font")
            .to_owned();
        crate::register_font(name, fs::read(path)?);
    }
    register_office_cloud_fonts()?;
    *registered = true;
    Ok(())
}

#[cfg(target_os = "windows")]
fn register_office_cloud_fonts() -> Result<()> {
    let Some(local_app_data) = std::env::var_os("LOCALAPPDATA") else {
        return Ok(());
    };
    let cache_root = PathBuf::from(local_app_data)
        .join("Microsoft")
        .join("FontCache");
    let Ok(cache_versions) = fs::read_dir(cache_root) else {
        return Ok(());
    };
    for cloud_root in cache_versions
        .filter_map(std::result::Result::ok)
        .map(|entry| entry.path().join("CloudFonts"))
        .filter(|path| path.is_dir())
    {
        for family in ["Grandview", "Grandview Display", "STKaiti"] {
            let directory = cloud_root.join(family);
            let Ok(entries) = fs::read_dir(directory) else {
                continue;
            };
            for entry in entries.filter_map(std::result::Result::ok) {
                let path = entry.path();
                if path.extension().and_then(|ext| ext.to_str()) != Some("ttf") {
                    continue;
                }
                let Ok(data) = fs::read(path) else {
                    continue;
                };
                if let Some(name) = font_alias_from_data(&data) {
                    crate::register_font(name, data);
                }
            }
        }
    }
    Ok(())
}

#[cfg(not(target_os = "windows"))]
fn register_office_cloud_fonts() -> Result<()> {
    Ok(())
}

fn font_alias_from_data(data: &[u8]) -> Option<String> {
    let face = ttf_parser::Face::parse(data, 0).ok()?;
    let name = |id| {
        face.names()
            .into_iter()
            .find(|name| name.name_id == id && name.is_unicode())
            .and_then(|name| name.to_string())
    };
    let family = name(ttf_parser::name_id::TYPOGRAPHIC_FAMILY)
        .or_else(|| name(ttf_parser::name_id::FAMILY))?;
    let subfamily = name(ttf_parser::name_id::TYPOGRAPHIC_SUBFAMILY)
        .or_else(|| name(ttf_parser::name_id::SUBFAMILY))
        .unwrap_or_default();
    Some(build_font_alias(
        &family,
        &subfamily,
        face.is_bold(),
        face.is_italic(),
    ))
}

fn build_font_alias(family: &str, subfamily: &str, bold: bool, italic: bool) -> String {
    let mut alias = normalize_font_alias(family);
    if subfamily.to_ascii_lowercase().contains("display") && !alias.contains("display") {
        alias.push_str("display");
    }
    if bold && italic {
        alias.push('z');
    } else if bold {
        alias.push('b');
    } else if italic {
        alias.push('i');
    }
    alias
}

fn normalize_font_alias(name: &str) -> String {
    name.chars()
        .filter(|character| character.is_ascii_alphanumeric())
        .flat_map(char::to_lowercase)
        .collect()
}

fn system_fallback_font_paths() -> Vec<PathBuf> {
    #[cfg(target_os = "windows")]
    {
        let directory = std::env::var_os("WINDIR")
            .map(PathBuf::from)
            .unwrap_or_else(|| PathBuf::from(r"C:\Windows"))
            .join("Fonts");
        return [
            "arial.ttf",
            "arialbd.ttf",
            "ariali.ttf",
            "arialbi.ttf",
            "calibri.ttf",
            "calibrib.ttf",
            "calibrii.ttf",
            "calibriz.ttf",
            "pala.ttf",
            "palab.ttf",
            "palai.ttf",
            "palabi.ttf",
            "corbel.ttf",
            "corbelb.ttf",
            "corbeli.ttf",
            "corbelz.ttf",
            "GARA.TTF",
            "GARABD.TTF",
            "GARAIT.TTF",
            "TCM_____.TTF",
            "TCB_____.TTF",
            "TCMI____.TTF",
            "TCBI____.TTF",
            "verdana.ttf",
            "verdanab.ttf",
            "verdanai.ttf",
            "verdanaz.ttf",
            "NotoSans-Regular.ttf",
            "NotoSansArmenian-Regular.ttf",
            "NotoSansArmenian-Bold.ttf",
            "NotoSansGeorgian-Regular.ttf",
            "NotoSansGeorgian-Bold.ttf",
            "ebrima.ttf",
            "ebrimabd.ttf",
            "YuGothR.ttc",
            "NotoSansSC-VF.ttf",
            "simkai.ttf",
            "simsun.ttc",
            "simhei.ttf",
            "malgunsl.ttf",
            "malgun.ttf",
            "micross.ttf",
            "LeelawUI.ttf",
            "Nirmala.ttf",
            "seguisym.ttf",
            "seguiemj.ttf",
        ]
        .into_iter()
        .map(|name| directory.join(name))
        .collect();
    }

    #[cfg(target_os = "linux")]
    {
        return [
            "/usr/share/fonts/truetype/noto/NotoSans-Regular.ttf",
            "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
            "/usr/share/fonts/truetype/noto/NotoSansThai-Regular.ttf",
            "/usr/share/fonts/truetype/noto/NotoSansDevanagari-Regular.ttf",
            "/usr/share/fonts/truetype/noto/NotoColorEmoji.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        ]
        .into_iter()
        .map(PathBuf::from)
        .collect();
    }

    #[cfg(target_os = "macos")]
    {
        return [
            "/System/Library/Fonts/Supplemental/Arial Unicode.ttf",
            "/System/Library/Fonts/AppleSDGothicNeo.ttc",
            "/System/Library/Fonts/ヒラギノ角ゴシック W3.ttc",
            "/System/Library/Fonts/Kohinoor.ttc",
            "/System/Library/Fonts/Apple Color Emoji.ttc",
        ]
        .into_iter()
        .map(PathBuf::from)
        .collect();
    }

    #[allow(unreachable_code)]
    Vec::new()
}

#[cfg(test)]
mod tests {
    use super::build_font_alias;

    #[cfg(target_os = "windows")]
    use super::system_fallback_font_paths;

    #[test]
    fn builds_stable_cloud_font_aliases() {
        assert_eq!(
            build_font_alias("Grandview", "Regular", false, false),
            "grandview"
        );
        assert_eq!(
            build_font_alias("Grandview", "Bold", true, false),
            "grandviewb"
        );
        assert_eq!(
            build_font_alias("Grandview", "Display", false, false),
            "grandviewdisplay"
        );
    }

    #[cfg(target_os = "windows")]
    #[test]
    fn registers_windows_latin_and_cjk_fallbacks() {
        let names = system_fallback_font_paths()
            .into_iter()
            .filter_map(|path| path.file_name().map(|name| name.to_owned()))
            .collect::<Vec<_>>();

        for name in [
            "arial.ttf",
            "arialbd.ttf",
            "ariali.ttf",
            "arialbi.ttf",
            "simkai.ttf",
            "simsun.ttc",
        ] {
            assert!(names.iter().any(|candidate| candidate == name));
        }
    }
}
