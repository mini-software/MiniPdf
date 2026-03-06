import json
d=json.load(open('d:/git/MiniPdf/tests/MiniPdf.Benchmark/reports/comparison_report.json',encoding='utf-8'))
below=[x for x in d if x['overall_score']<0.99]
print('below99=%d avg=%.4f' % (len(below), sum(x['overall_score'] for x in d)/len(d)))
