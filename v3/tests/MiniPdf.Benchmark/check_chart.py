import zipfile, os, re, sys

xlsx = os.path.join("..", "MiniPdf.Scripts", "output", "classic91_simple_bar_chart.xlsx")
with zipfile.ZipFile(xlsx) as z:
    # Find chart files
    chart_files = [n for n in z.namelist() if 'chart' in n.lower()]
    print(f"Chart files: {chart_files}")
    
    for cf in chart_files:
        if cf.endswith('.xml'):
            with z.open(cf) as f:
                content = f.read().decode()
            print(f"\n=== {cf} ===")
            # Show axis titles
            if 'title' in content.lower():
                # Find all <c:title> or title tags
                import xml.etree.ElementTree as ET
                root = ET.fromstring(content)
                ns = {'c': 'http://schemas.openxmlformats.org/drawingml/2006/chart',
                      'a': 'http://schemas.openxmlformats.org/drawingml/2006/main'}
                
                # Find chart type
                for elem in root.iter():
                    if 'barChart' in elem.tag or 'lineChart' in elem.tag:
                        print(f"Chart type element: {elem.tag}")
                
                # Find axis titles
                for ax in root.iter():
                    if ax.tag.endswith('}valAx') or ax.tag.endswith('}catAx') or ax.tag.endswith('}dateAx'):
                        ax_type = ax.tag.split('}')[1]
                        title_el = None
                        for child in ax:
                            if 'title' in child.tag.lower():
                                title_el = child
                        if title_el is not None:
                            texts = [t.text for t in title_el.iter() if t.text and t.text.strip()]
                            print(f"  {ax_type} title: {texts}")
                        else:
                            print(f"  {ax_type}: no title element")
                
                # Find chart title
                for elem in root.iter():
                    if elem.tag.endswith('}chart'):
                        for child in elem:
                            if 'title' in child.tag.lower():
                                texts = [t.text for t in child.iter() if t.text and t.text.strip()]
                                print(f"  Chart title: {texts}")

                # Show grouping
                for elem in root.iter():
                    if elem.tag.endswith('}grouping'):
                        print(f"  Grouping: {elem.get('val')}")

if len(sys.argv) > 1:
    name = sys.argv[1]
else:
    print("\nUsage: python check_chart.py [chart_name]")
