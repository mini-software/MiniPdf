"""Analyze text differences in worst DOCX text failures"""
import json

data = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))

# Focus on cases where text improvement could help most
targets = [
    'docx_classic50_long_table_with_formatting',  # text=0.8153
    'docx_classic08_bullet_list',                  # text=0.9180
    'docx_classic64_multi_column_layout',          # text=0.8837
    'docx_classic21_nested_lists',                 # text=0.9371
    'docx_classic100_multi_page_table',            # text=0.9658
    'docx_classic32_superscript_subscript',         # text=0.9590
    'docx_classic24_two_column_table_layout',      # text=0.9789
    'docx_classic49_cjk_document',                 # text=0.9057
    'docx_classic108_comparison_matrix',           # text=0.9753
]

for d in data:
    if d['name'] in targets:
        print(f"\n{'='*60}")
        print(f"{d['name']}: text={d['text_similarity']:.4f} vis={d['visual_avg']:.4f} overall={d['overall_score']:.4f}")
        print(f"Pages: {d['minipdf_pages']}/{d['reference_pages']}")
        if d.get('text_diff'):
            diff = d['text_diff']
            # Show first 500 chars of diff
            print(f"Text diff ({len(diff)} chars):")
            print(diff[:800])
