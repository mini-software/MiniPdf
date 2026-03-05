import json
with open('reports/comparison_report.json','r',encoding='utf-8') as f:
    data = json.load(f)

# Visual-only: text_sim >= 0.99 but vis < 0.99
visual_only = [r for r in data if r.get('text_similarity',0) >= 0.99 and r.get('visual_avg',0) < 0.99 and r.get('overall_score',0) < 0.99]
visual_only.sort(key=lambda r: r['visual_avg'], reverse=True)

print(f"Visual-only failures: {len(visual_only)}")
for r in visual_only:
    vs = r.get('visual_avg',0)
    ts = r.get('text_similarity',0)
    mp = r.get('minipdf_pages','?')
    rp = r.get('reference_pages','?')
    print(f"  {r['overall_score']:.4f} vis={vs:.4f} text={ts:.4f} pages={mp}/{rp} {r['name']}")

# Also show cases where visual is the bottleneck (text >=0.97 but vis < 0.95)
print("\n\nVisual bottleneck cases (text>=0.97, vis<0.95):")
vis_bot = [r for r in data if r.get('text_similarity',0) >= 0.97 and r.get('visual_avg',0) < 0.95 and r.get('overall_score',0) < 0.99]
vis_bot.sort(key=lambda r: r['visual_avg'], reverse=True)
for r in vis_bot:
    vs = r.get('visual_avg',0)
    ts = r.get('text_similarity',0)
    print(f"  {r['overall_score']:.4f} vis={vs:.4f} text={ts:.4f} {r['name']}")
