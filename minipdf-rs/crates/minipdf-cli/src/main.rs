use std::fs;
use std::path::{Path, PathBuf};

use clap::{Args, Parser, Subcommand, ValueEnum};

#[derive(Debug, Parser)]
#[command(name = "minipdf")]
#[command(version)]
#[command(about = "Convert Office files to PDF with the experimental Rust MiniPdf engine.")]
struct Cli {
    #[command(subcommand)]
    command: Option<Commands>,

    #[arg(value_name = "INPUT")]
    input: Option<PathBuf>,

    #[arg(short, long, value_name = "OUTPUT")]
    output: Option<PathBuf>,

    #[arg(long, value_name = "DIR")]
    fonts: Option<PathBuf>,

    #[command(flatten)]
    page: PageArgs,
}

#[derive(Debug, Subcommand)]
enum Commands {
    Convert(ConvertArgs),
}

#[derive(Debug, Parser)]
struct ConvertArgs {
    #[arg(value_name = "INPUT")]
    input: PathBuf,

    #[arg(short, long, value_name = "OUTPUT")]
    output: Option<PathBuf>,

    #[arg(long, value_name = "DIR")]
    fonts: Option<PathBuf>,

    #[command(flatten)]
    page: PageArgs,
}

#[derive(Debug, Args, Default)]
struct PageArgs {
    #[arg(long, value_enum, value_name = "SIZE")]
    paper_size: Option<PaperSizeArg>,

    #[arg(long, value_name = "POINTS", requires = "page_height")]
    page_width: Option<f32>,

    #[arg(long, value_name = "POINTS", requires = "page_width")]
    page_height: Option<f32>,
}

#[derive(Debug, Clone, Copy, ValueEnum)]
enum PaperSizeArg {
    A4,
    Letter,
}

fn main() {
    let cli = Cli::parse();
    let result = match cli.command {
        Some(Commands::Convert(args)) => {
            run_convert(args.input, args.output, args.fonts, args.page)
        }
        None => {
            let Some(input) = cli.input else {
                eprintln!("Error: input file is required. Use --help for usage.");
                std::process::exit(1);
            };
            run_convert(input, cli.output, cli.fonts, cli.page)
        }
    };

    if let Err(err) = result {
        eprintln!("Error: {err}");
        std::process::exit(1);
    }
}

fn run_convert(
    input: PathBuf,
    output: Option<PathBuf>,
    fonts: Option<PathBuf>,
    page: PageArgs,
) -> minipdf::Result<()> {
    if !input.exists() {
        return Err(minipdf::MiniPdfError::InvalidInput(format!(
            "file not found: {}",
            input.display()
        )));
    }

    let ext = input
        .extension()
        .and_then(|ext| ext.to_str())
        .map(|ext| ext.to_ascii_lowercase())
        .unwrap_or_default();
    if ext != "xlsx" && ext != "docx" && ext != "pptx" {
        return Err(minipdf::MiniPdfError::InvalidInput(format!(
            "unsupported file type '.{ext}'. Supported: .xlsx, .docx, .pptx"
        )));
    }

    if let Some(font_dir) = fonts {
        register_fonts_from_dir(&font_dir)?;
    } else {
        register_system_fallback_fonts()?;
    }

    let options = conversion_options(page)?;
    let output = output.unwrap_or_else(|| input.with_extension("pdf"));
    minipdf::convert_to_pdf_with_options(&input, &output, &options)?;
    println!("{}", output.display());
    Ok(())
}

fn conversion_options(page: PageArgs) -> minipdf::Result<minipdf::ConversionOptions> {
    if page.paper_size.is_some() && page.page_width.is_some() {
        return Err(minipdf::MiniPdfError::InvalidInput(
            "use either --paper-size or --page-width/--page-height, not both".to_owned(),
        ));
    }
    let page_size = match (page.paper_size, page.page_width, page.page_height) {
        (Some(PaperSizeArg::A4), None, None) => Some(minipdf::PageSize::A4),
        (Some(PaperSizeArg::Letter), None, None) => Some(minipdf::PageSize::LETTER),
        (None, Some(width), Some(height)) => Some(minipdf::PageSize::new(width, height)?),
        (None, None, None) => None,
        _ => {
            return Err(minipdf::MiniPdfError::InvalidInput(
                "--page-width and --page-height must be specified together".to_owned(),
            ));
        }
    };
    Ok(minipdf::ConversionOptions { page_size })
}

fn register_fonts_from_dir(font_dir: &Path) -> minipdf::Result<()> {
    if !font_dir.is_dir() {
        return Err(minipdf::MiniPdfError::InvalidInput(format!(
            "font directory not found: {}",
            font_dir.display()
        )));
    }

    for entry in fs::read_dir(font_dir)? {
        let entry = entry?;
        let path = entry.path();
        let ext = path
            .extension()
            .and_then(|ext| ext.to_str())
            .map(|ext| ext.to_ascii_lowercase());
        if matches!(ext.as_deref(), Some("ttf" | "ttc" | "otf")) {
            let data = fs::read(&path)?;
            let name = font_alias_from_data(&data).unwrap_or_else(|| {
                path.file_stem()
                    .and_then(|name| name.to_str())
                    .unwrap_or("font")
                    .to_owned()
            });
            minipdf::register_font(name, data);
        }
    }

    Ok(())
}

fn register_system_fallback_fonts() -> minipdf::Result<()> {
    let candidates = system_fallback_font_paths();
    for path in candidates.into_iter().filter(|path| path.is_file()) {
        let name = path
            .file_stem()
            .and_then(|name| name.to_str())
            .unwrap_or("font")
            .to_owned();
        minipdf::register_font(name, fs::read(path)?);
    }
    register_office_cloud_fonts()?;
    Ok(())
}

#[cfg(target_os = "windows")]
fn register_office_cloud_fonts() -> minipdf::Result<()> {
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
                    minipdf::register_font(name, data);
                }
            }
        }
    }
    Ok(())
}

#[cfg(not(target_os = "windows"))]
fn register_office_cloud_fonts() -> minipdf::Result<()> {
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
    fn registers_all_arial_style_variants() {
        let names = system_fallback_font_paths()
            .into_iter()
            .filter_map(|path| path.file_name().map(|name| name.to_owned()))
            .collect::<Vec<_>>();

        for name in ["arial.ttf", "arialbd.ttf", "ariali.ttf", "arialbi.ttf"] {
            assert!(names.iter().any(|candidate| candidate == name));
        }

        for name in ["simkai.ttf", "simsun.ttc"] {
            assert!(names.iter().any(|candidate| candidate == name));
        }
    }
}
