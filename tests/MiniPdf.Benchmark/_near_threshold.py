import json

# Analyze both DOCX and XLSX reports for cases closest to 0.99
for label, path in [("DOCX", "reports_docx/comparison_report.json"), ("XLSX", "reports/comparison_report.json")]:
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    
    below = []
    for item in data:
        score = item.get("overall_score", 0)
        if score < 0.99:
            text = item.get("text_similarity", 0)
            vis = item.get("visual_avg", 0)
            pages_match = 1.0 if item.get("minipdf_pages") == item.get("reference_pages") else 0.0
            below.append({
                "name": item["name"],
                "overall": score,
                "text": text,
                "vis": vis,
                "pages": pages_match,
                "gap": 0.99 - score
            })
    
    below.sort(key=lambda x: x["gap"])
    
    print(f"\n{'='*80}")
    print(f"{label}: {len(below)} below 0.99 (avg {sum(x['overall'] for x in below)/len(below):.4f})")
    print(f"{'='*80}")
    
    # Show top 30 closest to threshold
    print(f"\nTop 30 closest to 0.99:")
    print(f"{'Name':<50} {'Overall':>8} {'Gap':>7} {'Text':>6} {'Vis':>6} {'Pg':>4}")
    for item in below[:30]:
        print(f"{item['name']:<50} {item['overall']:>8.4f} {item['gap']:>7.4f} {item['text']:>6.2f} {item['vis']:>6.2f} {item['pages']:>4.1f}")
    
    # Categorize by what's needed
    text_fixable = [x for x in below if x["text"] < 0.99 and x["vis"] >= 0.99 and x["pages"] == 1.0]
    vis_fixable = [x for x in below if x["vis"] < 0.99 and x["text"] >= 0.99 and x["pages"] == 1.0]
    page_fixable = [x for x in below if x["pages"] == 0.0 and x["text"] >= 0.99 and x["vis"] >= 0.99]
    both_needed = [x for x in below if x["text"] < 0.99 and x["vis"] < 0.99]
    
    print(f"\nCategories:")
    print(f"  Text-only fix needed: {len(text_fixable)}")
    print(f"  Visual-only fix needed: {len(vis_fixable)}")
    print(f"  Page count fix needed: {len(page_fixable)}")
    print(f"  Both text+visual needed: {len(both_needed)}")
    
    # What text improvement would cross threshold?
    print(f"\nCases where small text improvement crosses 0.99:")
    for item in below[:50]:
        if item["text"] < 1.0 and item["vis"] >= 0.95:
            # What text score needed?
            # overall = text*0.4 + vis*0.4 + page*0.2
            needed_text = (0.99 - item["vis"]*0.4 - item["pages"]*0.2) / 0.4
            if needed_text <= 1.0 and needed_text > item["text"]:
                delta = needed_text - item["text"]
                if delta < 0.05:
                    print(f"  {item['name']:<45} text={item['text']:.4f} need={needed_text:.4f} delta={delta:.4f} vis={item['vis']:.4f}")
    
    print(f"\nCases where small visual improvement crosses 0.99:")
    for item in below[:50]:
        if item["vis"] < 1.0 and item["text"] >= 0.95:
            needed_vis = (0.99 - item["text"]*0.4 - item["pages"]*0.2) / 0.4
            if needed_vis <= 1.0 and needed_vis > item["vis"]:
                delta = needed_vis - item["vis"]
                if delta < 0.05:
                    print(f"  {item['name']:<45} vis={item['vis']:.4f} need={needed_vis:.4f} delta={delta:.4f} text={item['text']:.4f}")
