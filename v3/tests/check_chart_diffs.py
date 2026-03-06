import json

with open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reports\comparison_report.json') as f:
    data = json.load(f)

# Check specific chart cases
targets = ['classic91_simple_bar_chart', 'classic93_line_chart', 'classic92_horizontal_bar_chart',
           'classic103_pie_chart_with_labels', 'classic111_chart_with_axis_labels']
for d in data:
    if d['name'] in targets:
        print(f"\n=== {d['name']} ===")
        print(f"  score={d['overall_score']} ts={d['text_similarity']} va={d['visual_avg']}")
        diff = d.get('text_diff', '')
        if diff and diff != '(identical)':
            lines = diff.split('\n')
            # Show first 30 diff lines
            for line in lines[:30]:
                print(f"  {line}")
