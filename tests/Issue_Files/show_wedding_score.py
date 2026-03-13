import json
d = json.load(open('tests/Issue_Files/reports_xlsx/comparison_report.json', encoding='utf-8'))
w = [x for x in d if x['name'] == 'Wedding_timeline_planner1_copy'][0]
print(f"text={w['text_similarity']}, visual_avg={w['visual_avg']}, pages={w['minipdf_pages']}/{w['reference_pages']}")
print(f"visual_scores={w['visual_scores']}")
print(f"overall={w['overall_score']}")
p = [x for x in d if x['name'] == 'payroll-calculator_f'][0]
print(f"\npayroll: text={p['text_similarity']}, visual_avg={p['visual_avg']}, pages={p['minipdf_pages']}/{p['reference_pages']}")
print(f"payroll visual_scores (first 5)={p['visual_scores'][:5]}")
print(f"payroll overall={p['overall_score']}")
