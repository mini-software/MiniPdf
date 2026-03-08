import json

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

below99 = [r for r in data if r['overall_score'] < 0.99]
print(f'Below 99: {len(below99)}')

# Show lowest visual scores
vis_low = sorted(data, key=lambda x: x['visual_avg'])
print('\n--- Lowest Visual Scores ---')
for r in vis_low[:25]:
    print(f"{r['name']}: overall={r['overall_score']:.4f} text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")

# Show lowest text scores
txt_low = sorted(data, key=lambda x: x['text_similarity'])
print('\n--- Lowest Text Scores ---')
for r in txt_low[:25]:
    print(f"{r['name']}: overall={r['overall_score']:.4f} text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
