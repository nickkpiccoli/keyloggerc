from http.server import BaseHTTPRequestHandler, HTTPServer
import json

class SimpleHTTPRequestHandler(BaseHTTPRequestHandler):
    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()

    def do_POST(self):
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        decoded_data = post_data.decode('utf-8')

        try:
            # Carica il JSON dal corpo della richiesta
            json_data = json.loads(decoded_data)
            self._set_response()
            self.wfile.write("Body della richiesta POST:\n".encode('utf-8'))
            self.wfile.write(json.dumps(json_data, indent=2).encode('utf-8'))
        except json.JSONDecodeError as e:
            self.send_response(400)
            self.send_header('Content-type', 'text/plain')
            self.end_headers()
            self.wfile.write(f"Errore nel parsing del JSON: {str(e)}".encode('utf-8'))

def run(server_class=HTTPServer, handler_class=SimpleHTTPRequestHandler, port=8000):
    server_address = ('localhost', port)
    httpd = server_class(server_address, handler_class)
    print(f'Server in esecuzione sulla porta {port}')
    httpd.serve_forever()

if __name__ == '__main__':
    run()
