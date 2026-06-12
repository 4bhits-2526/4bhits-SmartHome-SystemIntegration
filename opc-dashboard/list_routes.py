from importlib import import_module

app = import_module('src.main').app
for route in app.routes:
    print(type(route).__name__, getattr(route, 'path', None), getattr(route, 'methods', None))
