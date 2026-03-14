"""Check which similarity measure dominates for each case, 
and find cases where the best text sim could improve."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Filter to text_data cases (non-chart, non-unicode) with gap > 0.01
# and where text is the bottleneck
text_limited = []
for c in d:
    name = c['name']
    ts = c['text_similarity']
    flat = c.get('flat_text_similarity', 0)
    word = c.get('word_text_similarity', 0)
    best_text = max(ts, flat, word)
    vs = c['visual_avg']
    
    which = 'page' if ts == best_text else ('flat' if flat == best_text else 'word')
    
    if best_text < vs and best_text < 0.99:
        text_limited.append((name, ts, flat, word, best_text, vs, which))

text_limited.sort(key=lambda x: x[4])
print(f"{'Name':<45} {'page':>6} {'flat':>6} {'word':>6} {'best':>6} {'vis':>6} {'src':>5}")
for name, ts, flat, word, best, vs, which in text_limited[:30]:
    print(f"{name:<45} {ts:6.4f} {flat:6.4f} {word:6.4f} {best:6.4f} {vs:6.4f} {which:>5}")
