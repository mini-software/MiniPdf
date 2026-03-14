"""Compare current results vs saved baseline to find regressions and improvements."""
import json

# Load current results
current = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Load baseline (need to regenerate or check _latest_bench.txt)
# Read from the latest report or compare per case
# For now, let's just show all cases where text_sim or overall changed significantly

# Since we don't have a saved per-case baseline, let's check for unusual patterns
for c in sorted(current, key=lambda x: x['overall_score']):
    # Show bottom 30
    if c['overall_score'] < 0.97:
        flat = c.get('flat_text_similarity', 0)
        word = c.get('word_text_similarity', 0)
        ts = c['text_similarity']
        # text_similarity is MAX of ts, flat, word
        actual_ts = max(ts, flat, word)
        print(f"{c['name']}: text={actual_ts:.4f} vis={c['visual_avg']:.4f} overall={c['overall_score']:.4f}")
