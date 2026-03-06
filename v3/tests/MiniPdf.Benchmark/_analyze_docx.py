import json, sys
with open(sys.argv[1], 'r') as f:
    data = json.load(f)
for r in sorted(data, key=lambda x: x.get('overall_score', 0)):
    s = r.get('overall_score', 0)
    if s < 0.95:
        name = r['name']
        mp = r.get('minipdf_pages', '?')
        rp = r.get('reference_pages', '?')
        ts = r.get('text_score', 0)
        vs = r.get('visual_score', 0)
        print(f"{name}: {s:.4f}  pages={mp}/{rp}  text={ts:.3f}  visual={vs:.3f}")
