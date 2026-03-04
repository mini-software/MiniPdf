import json

with open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reports\comparison_report.json') as f:
    data = json.load(f)

targets = ['classic13_date_strings', 'classic17_currency_strings', 'classic40_scientific_notation', 
           'classic58_mixed_numeric_formats', 'classic41_integer_vs_float', 'classic49_contact_list']
for d in data:
    if d['name'] in targets:
        print(f"\n=== {d['name']} (score={d['overall_score']}) ===")
        diff = d.get('text_diff', '')
        if diff and diff != '(identical)':
            lines = diff.split('\n')
            for line in lines[:25]:
                print(f"  {line}")
