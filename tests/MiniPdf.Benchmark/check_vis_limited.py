"""Find cases where visual score is the bottleneck and potentially improvable.
Focus on non-chart cases where visual_avg < text_sim."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

vis_limited = []
for c in d:
    name = c['name']
    ts = max(c['text_similarity'], c.get('flat_text_similarity', 0), c.get('word_text_similarity', 0))
    vs = c['visual_avg']
    gap = 1.0 - c['overall_score']
    
    # VIS-bottleneck: vis < text and gap > 1%
    if vs < ts and gap > 0.01:
        is_chart = any(x in name for x in ['chart', 'pie', 'bar_chart', 'bar_custom', 'line_chart', 
                                              'scatter', 'area', 'radar', 'bubble', 'doughnut', 
                                              'stock', 'combo', 'dashboard', 'stacked', '3d_'])
        vis_limited.append((name, ts, vs, c['overall_score'], gap, is_chart))

vis_limited.sort(key=lambda x: -x[4])
print("NON-CHART visual-limited cases:")
for name, ts, vs, overall, gap, is_chart in vis_limited:
    if not is_chart:
        print(f"  {name}: text={ts:.4f} vis={vs:.4f} overall={overall:.4f} gap={gap:.4f}")
print()
print("CHART visual-limited cases (top 10):")
for name, ts, vs, overall, gap, is_chart in vis_limited[:10]:
    if is_chart:
        print(f"  {name}: text={ts:.4f} vis={vs:.4f} overall={overall:.4f} gap={gap:.4f}")
