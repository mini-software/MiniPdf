import json, os

# Analyze text diffs for closest XLSX text-fixable cases
cases = [
    "classic86_software_screenshot_features",
    "classic68_restaurant_menu",
    "classic49_contact_list",
    "classic51_product_catalog",
    "classic153_currency_symbols",
    "classic140_rotated_text",
    "classic44_employee_roster",
    "classic74_dashboard_with_kpi_image",
    "classic90_project_status_with_milestones",
    "classic171_ipa_phonetic",
    "classic170_emoji_dashboard",
    "classic165_southeast_asian",
    "classic154_math_symbols",
    "classic178_caucasus_ethiopic",
]

with open("reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

for item in data:
    name = item["name"]
    if name not in cases:
        continue
    
    text_diff = item.get("text_diff", "")
    print(f"\n{'='*80}")
    print(f"{name}: text={item.get('text_similarity',0):.4f} vis={item.get('visual_avg',0):.4f} overall={item.get('overall_score',0):.4f}")
    print(f"{'='*80}")
    
    if text_diff:
        lines = text_diff.split('\n')
        # Show only diff lines (+ and -)
        diff_lines = [l for l in lines if l.startswith('+') or l.startswith('-')]
        for l in diff_lines[:40]:
            print(l)
        if len(diff_lines) > 40:
            print(f"... ({len(diff_lines)} total diff lines)")
    else:
        print("No text_diff available")
