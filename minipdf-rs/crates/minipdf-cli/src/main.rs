use std::fs;
use std::path::{Path, PathBuf};

use clap::{Parser, Subcommand};

#[derive(Debug, Parser)]
#[command(name = "minipdf")]
#[command(version)]
#[command(about = "Convert Excel/Word files to PDF with the experimental Rust MiniPdf engine.")]
struct Cli {
    #[command(subcommand)]
    command: Option<Commands>,

    #[arg(value_name = "INPUT")]
    input: Option<PathBuf>,

    #[arg(short, long, value_name = "OUTPUT")]
    output: Option<PathBuf>,

    #[arg(long, value_name = "DIR")]
    fonts: Option<PathBuf>,
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
}

fn main() {
    let cli = Cli::parse();
    let result = match cli.command {
        Some(Commands::Convert(args)) => run_convert(args.input, args.output, args.fonts),
        None => {
            let Some(input) = cli.input else {
                eprintln!("Error: input file is required. Use --help for usage.");
                std::process::exit(1);
            };
            run_convert(input, cli.output, cli.fonts)
        }
    };

    if let Err(err) = result {
        eprintln!("Error: {err}");
        std::process::exit(1);
    }
}

fn run_convert(input: PathBuf, output: Option<PathBuf>, fonts: Option<PathBuf>) -> minipdf::Result<()> {
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
    if ext != "xlsx" && ext != "docx" {
        return Err(minipdf::MiniPdfError::InvalidInput(format!(
            "unsupported file type '.{ext}'. Supported: .xlsx, .docx"
        )));
    }

    if let Some(font_dir) = fonts {
        register_fonts_from_dir(&font_dir)?;
    }

    let output = output.unwrap_or_else(|| input.with_extension("pdf"));
    minipdf::convert_to_pdf(&input, &output)?;
    println!("{}", output.display());
    Ok(())
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
            let name = path
                .file_stem()
                .and_then(|name| name.to_str())
                .unwrap_or("font")
                .to_owned();
            minipdf::register_font(name, fs::read(path)?);
        }
    }

    Ok(())
}