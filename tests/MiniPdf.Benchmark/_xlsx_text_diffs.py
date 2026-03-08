"""Deep dive into near-miss XLSX text failures to find fixable patterns."""
import json
import fitz

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

# Focus on cases where text improvement could push overall >= 0.99
# or significant text gap indicating fixable issues
cases_of_interest = [
    'classic86_software_screenshot_features',  # text=0.9730
    'classic68_restaurant_menu',  # text=0.9881
    'classic49_contact_list',  # text=0.9737
    'classic51_product_catalog',  # text=0.9747
    'classic153_currency_symbols',  # text=0.9723
    'classic44_employee_roster',  # text=0.9652
    'classic90_project_status_with_milestones',  # text=0.9572
    'classic74_dashboard_with_kpi_image',  # text=0.9595
    'classic09_long_text',  # text=0.6785 - page count issue
    'classic150_kitchen_sink_styles',  # text=0.9677
]

for name in cases_of_interest:
    r = [x for x in data if x['name'] == name][0]
    print(f"\n{'='*60}")
    print(f"{name}: text={r['text_similarity']:.4f} flat={r['flat_text_similarity']:.4f} vis={r['visual_avg']:.4f} pages={r['minipdf_pages']}vs{r['reference_pages']}")
    
    try:
        mini = fitz.open(f"../MiniPdf.Scripts/pdf_output/{name}.pdf")
        ref = fitz.open(f"reference_pdfs/{name}.pdf")
        
        mini_text = ""
        ref_text = ""
        for p in mini:
            mini_text += p.get_text()
        for p in ref:
            ref_text += p.get_text()
        
        print(f"  Text chars: mini={len(mini_text)} ref={len(ref_text)}")
        
        # Find differences (char by char)
        from difflib import SequenceMatcher
        sm = SequenceMatcher(None, mini_text, ref_text)
        blocks = sm.get_matching_blocks()
        
        # Show non-matching regions
        diff_count = 0
        for tag, i1, i2, j1, j2 in sm.get_opcodes():
            if tag != 'equal' and diff_count < 5:
                mini_snippet = repr(mini_text[i1:i2][:50])
                ref_snippet = repr(ref_text[j1:j2][:50])
                print(f"  {tag}: mini[{i1}:{i2}]={mini_snippet}")
                print(f"        ref[{j1}:{j2}]={ref_snippet}")
                diff_count += 1
        
        mini.close()
        ref.close()
    except Exception as e:
        print(f"  Error: {e}")
