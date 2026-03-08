import json

# Load new results (with bold)
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    new_data = json.load(f)
if isinstance(new_data, list):
    new_results = {r.get('name',''): r for r in new_data}
else:
    new_results = {r.get('name',''): r for r in new_data['results']}

# We'll load a saved baseline to compare... but we don't have it saved.
# Let me instead just look at which cases are worst in the new results
results = new_data if isinstance(new_data, list) else new_data['results']
below99 = [r for r in results if r['overall_score'] < 0.99]
below99.sort(key=lambda r: r['overall_score'], reverse=True)

# Count categories
text_only = len([r for r in below99 if r.get('text_similarity',0) < 0.99 and r.get('visual_avg',0) >= 0.99])
vis_only = len([r for r in below99 if r.get('text_similarity',0) >= 0.99 and r.get('visual_avg',0) < 0.99])
both = len([r for r in below99 if r.get('text_similarity',0) < 0.99 and r.get('visual_avg',0) < 0.99])
print(f'Below99: {len(below99)}')
print(f'Text-only: {text_only}, Vis-only: {vis_only}, Both: {both}')

# Show new page mismatches
for r in results:
    if r.get('minipdf_pages',0) != r.get('reference_pages',0):
        name = r.get('name','')
        print(f'  Page mismatch: {name} {r["minipdf_pages"]}vs{r["reference_pages"]}')

# Show cases that are now above 0.99 (would be improvements if any)
above99 = [r for r in results if r['overall_score'] >= 0.99]
print(f'\nAbove 99: {len(above99)}')

# Show distribution of visual scores 
vis_scores = [r.get('visual_avg',0) for r in results]
print(f'Visual avg: {sum(vis_scores)/len(vis_scores):.4f}')
print(f'Visual min: {min(vis_scores):.4f}')
