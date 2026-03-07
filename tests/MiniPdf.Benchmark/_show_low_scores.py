import json

path = "tests/MiniPdf.Benchmark/reports_docx/comparison_report.json"
with open(path, encoding="utf-8") as f:
    data = json.load(f)

if isinstance(data, list):
    items = sorted(data, key=lambda x: x.get("overall_score", 0))
    print(f"Total: {len(items)}")
    for x in items[:40]:
        score = x.get("overall_score", 0)
        name = x.get("name", "?")
        pages_mp = x.get("minipdf_pages", "?")
        pages_ref = x.get("reference_pages", "?")
        txt = x.get("text_similarity", 0)
        vis = x.get("visual_avg", 0)
        print(f"{score:.4f}  txt={txt:.3f} vis={vis:.3f} pg={pages_mp}/{pages_ref}  {name}")
