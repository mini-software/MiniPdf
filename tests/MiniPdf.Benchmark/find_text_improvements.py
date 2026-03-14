"""Identify text_similarity improvement opportunities."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Find cases with text_sim < 1.0 that are NOT unicode/emoji
unicode_words = [
    'cjk', 'emoji', 'rtl', 'bidi', 'indic', 'korean', 'african',
    'musical', 'southeast_asian', 'multilingual', 'multiscript',
    'polyglot', 'zwj', 'caucasus', 'combining', 'ipa_phonetic',
    'box_drawing', 'cyrillic', 'currency_symbols', 'math_symbols',
    'diacritical', 'skin_tone', 'punctuation', 'technical_symbols',
]

print(f"Non-unicode cases with text_similarity < 1.0:")
print(f"{'Name':55s} {'text':>7s} {'vis':>7s} {'pages':>8s} {'overall':>8s}")

cases = []
for c in d:
    name = c['name']
    text = c.get('text_similarity', 0)
    vis = c.get('visual_avg', 0)
    mp = c.get('minipdf_pages', 0)
    rp = c.get('reference_pages', 0)
    overall = c.get('overall_score', 0)
    
    is_unicode = any(w in name.lower() for w in unicode_words)
    if is_unicode:
        continue
    
    if text < 1.0:
        gap = 1.0 - text
        cases.append((gap, name, text, vis, mp, rp, overall))

cases.sort(reverse=True)
for gap, name, text, vis, mp, rp, overall in cases:
    pages = f"{mp}/{rp}"
    print(f"  {name:55s} {text:7.4f} {vis:7.4f} {pages:>8s} {overall:8.4f}")

total_gap = sum(g for g, *_ in cases)
print(f"\nTotal text_sim gap: {total_gap:.4f}")
print(f"Potential avg improvement: {total_gap * 0.4 / 191:.6f}")
