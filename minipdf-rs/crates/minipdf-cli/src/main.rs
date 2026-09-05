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
    minipdf::register_system_fonts()
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

#[cfg(test)]
mod tests {
    use super::build_font_alias;

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
}
