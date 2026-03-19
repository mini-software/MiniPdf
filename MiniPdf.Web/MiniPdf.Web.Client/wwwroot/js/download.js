window.downloadFile = function (fileName, contentType, byteArray) {
    var blob = new Blob([byteArray], { type: contentType });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// Local Font Access API support (Chrome 103+)
// Returns the font-family name of the first matching CJK font, or null.
// The font blob is cached so getLocalFontStream() can return it immediately after.
let _localFontBlob = null;

window.tryLoadLocalFontMeta = async function () {
    if (!('queryLocalFonts' in window)) return null;
    try {
        const fonts = await window.queryLocalFonts();
        const preferred = [
            // Windows
            'Microsoft YaHei', 'Microsoft YaHei UI',
            'SimSun', 'SimHei', 'FangSong', 'KaiTi',
            // macOS / iOS
            'PingFang SC', 'Heiti SC', 'STHeiti', 'STSong',
            // Japanese (Windows / macOS)
            'Meiryo', 'Yu Gothic', 'MS Gothic', 'Hiragino Sans',
            // Korean (Windows / macOS)
            'Malgun Gothic', 'Apple SD Gothic Neo', 'Gulim', 'Dotum',
            // Google Noto / open-source
            'Noto Sans CJK SC', 'Noto Sans SC', 'Source Han Sans SC',
        ];
        let found = null;
        for (const family of preferred) {
            found = fonts.find(f => f.family === family && (f.style === 'Regular' || f.style === 'Normal'));
            if (!found) found = fonts.find(f => f.family === family);
            if (found) break;
        }
        if (!found) return null;
        _localFontBlob = await found.blob();
        return found.family;
    } catch {
        return null;
    }
};

// Returns a ReadableStream for the cached font blob (call after tryLoadLocalFontMeta succeeds).
window.getLocalFontStream = function () {
    const blob = _localFontBlob;
    _localFontBlob = null; // release reference
    return blob ? blob.stream() : null;
};
