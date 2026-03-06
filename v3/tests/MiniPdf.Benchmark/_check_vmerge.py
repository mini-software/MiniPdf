import re

with open('generate_classic_xlsx.py', encoding='utf-8') as f:
    content = f.read()

# Find all function definitions and their merge_cells calls
current_func = None
funcs_with_vmerge = {}
for line in content.split('\n'):
    m = re.match(r'def (classic\d+\w+)\(\)', line)
    if m:
        current_func = m.group(1)
    m2 = re.search(r'merge_cells\(["\']([A-Z]+)(\d+):([A-Z]+)(\d+)["\']\)', line)
    if m2 and current_func:
        start_row = int(m2.group(2))
        end_row = int(m2.group(4))
        if end_row > start_row:
            rng = m2.group(1) + m2.group(2) + ':' + m2.group(3) + m2.group(4)
            if current_func not in funcs_with_vmerge:
                funcs_with_vmerge[current_func] = []
            funcs_with_vmerge[current_func].append(rng)

print(f"Files with vertical merges: {len(funcs_with_vmerge)}")
for func, merges in sorted(funcs_with_vmerge.items()):
    print(f"  {func}: {', '.join(merges)}")
