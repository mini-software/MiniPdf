#!/bin/bash
# startup.sh — Azure App Service custom startup script
# Downloads common CJK + Latin fonts to /home/fonts/ on first run.
# /home/ is persistent storage on Azure App Service Linux.

FONT_DIR="/home/fonts"

if [ ! -f "$FONT_DIR/.done" ]; then
    echo "[startup] Downloading fonts to $FONT_DIR ..."
    mkdir -p "$FONT_DIR"

    # Noto Sans SC (Simplified Chinese)
    curl -fsSL -o "$FONT_DIR/NotoSansSC-Regular.ttf" \
        "https://github.com/google/fonts/raw/main/ofl/notosanssc/NotoSansSC%5Bwght%5D.ttf" || true

    # Noto Sans TC (Traditional Chinese)
    curl -fsSL -o "$FONT_DIR/NotoSansTC-Regular.ttf" \
        "https://github.com/google/fonts/raw/main/ofl/notosanstc/NotoSansTC%5Bwght%5D.ttf" || true

    # Noto Sans JP (Japanese)
    curl -fsSL -o "$FONT_DIR/NotoSansJP-Regular.ttf" \
        "https://github.com/google/fonts/raw/main/ofl/notosansjp/NotoSansJP%5Bwght%5D.ttf" || true

    # Noto Sans KR (Korean)
    curl -fsSL -o "$FONT_DIR/NotoSansKR-Regular.ttf" \
        "https://github.com/google/fonts/raw/main/ofl/notosanskr/NotoSansKR%5Bwght%5D.ttf" || true

    # Noto Sans (Latin / Western)
    curl -fsSL -o "$FONT_DIR/NotoSans-Regular.ttf" \
        "https://github.com/google/fonts/raw/main/ofl/notosans/NotoSans%5Bwdth%2Cwght%5D.ttf" || true

    # Noto Serif SC (思源宋体 — 宋体替代)
    curl -fsSL -o "$FONT_DIR/NotoSerifSC-Regular.ttf" \
        "https://github.com/google/fonts/raw/main/ofl/notoserifsc/NotoSerifSC%5Bwght%5D.ttf" || true

    # Noto Serif TC (思源宋体 繁体)
    curl -fsSL -o "$FONT_DIR/NotoSerifTC-Regular.ttf" \
        "https://github.com/google/fonts/raw/main/ofl/notoseriftc/NotoSerifTC%5Bwght%5D.ttf" || true

    # LXGW WenKai (霞鹜文楷 — 楷体替代)
    curl -fsSL -o "$FONT_DIR/LXGWWenKai-Regular.ttf" \
        "https://github.com/lxgw/LxgwWenKai/releases/download/v1.501/LXGWWenKai-Regular.ttf" || true

    touch "$FONT_DIR/.done"
    echo "[startup] Fonts downloaded."
else
    echo "[startup] Fonts already present, skipping download."
fi

# Start the application
cd /home/site/wwwroot
exec dotnet MiniPdf.Api.dll
