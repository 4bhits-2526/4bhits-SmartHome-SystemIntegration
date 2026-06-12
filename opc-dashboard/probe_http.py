import urllib.request

for path in ['/', '/docs', '/ws']:
    try:
        with urllib.request.urlopen(f'http://127.0.0.1:8000{path}') as r:
            print(path, r.status, r.getheader('Content-Type'))
            print(r.read(200).decode('utf-8', errors='ignore'))
    except Exception as e:
        print(path, 'ERROR', type(e).__name__, e)
