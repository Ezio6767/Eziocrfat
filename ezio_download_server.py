from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import urlparse
import os

URL_FILE = "current_url.txt"

class RedirectHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        parsed = urlparse(self.path)
        if parsed.path != "/latest.zip":
            self.send_error(404, "Not Found")
            return

        url = self.load_current_url()
        if not url:
            self.send_error(500, "Aucune URL définie dans current_url.txt")
            return

        self.send_response(302)
        self.send_header("Location", url)
        self.end_headers()

    def load_current_url(self):
        try:
            script_dir = os.path.dirname(os.path.abspath(__file__))
            path = os.path.join(script_dir, URL_FILE)
            with open(path, "r", encoding="utf-8") as f:
                return f.read().strip()
        except Exception:
            return None

    def log_message(self, format, *args):
        return


def run(server_class=HTTPServer, handler_class=RedirectHandler, port=5000):
    server_address = ("0.0.0.0", port)
    httpd = server_class(server_address, handler_class)
    print(f"Serveur démarré : http://localhost:{port}/latest.zip")
    print("Mettez à jour current_url.txt avec votre vrai lien ZIP.")
    httpd.serve_forever()


if __name__ == "__main__":
    run()
