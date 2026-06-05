import ctypes
import os
import subprocess
import sys
import sysconfig
import time
import zipfile
import urllib.request
import threading
import tkinter as tk
import tkinter.font as tkfont
from tkinter import filedialog, messagebox

# Configuration
DEFAULT_GAME_NAME = "Eziocraft"
DEFAULT_INSTALL_DIR = os.path.join(os.environ.get("LOCALAPPDATA", os.path.expanduser("~")), DEFAULT_GAME_NAME)
DEFAULT_ZIP_URL = "https://my.microsoftpersonalcontent.com/personal/1434fbbfed89af18/_layouts/15/download.aspx?UniqueId=5363638b-8e45-445b-8fa2-f155969bcff5&Translate=false&tempauth=v1e.eyJzaXRlaWQiOiJjY2NmNjdhNC0zYmE4LTRkZTItOTRlZi1lZGM0NDE4OWFmOWUiLCJhdWQiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAvbXkubWljcm9zb2Z0cGVyc29uYWxjb250ZW50LmNvbUA5MTg4MDQwZC02YzY3LTRjNWItYjExMi0zNmEzMDRiNjZkYWQiLCJleHAiOiIxNzc3NzM5MjEzIn0.rBPq5YNsgdnyL6jO5RtY_p4Bm6jAJ0cMXV0_btum208p8Z7kzX1p8alspZvTIjlCGsBIGKsDxW5i9qbb5yvZtbdGofxd4ZpLAC2fWhyIIJzg4i5HeV_YbbCJZd0jVhdckjs-DUSgaEZ4mQecw6DYj6L-4KShMqs2KNu2ELYP17vb8_jVGK7EQDXZ47Xzfz9ytrTWIpvQkjJB6K27FXNMQuo2WZXW0YiGlVDDTNkFVR8utkJLCEECorI14wwUAvuGaQjAFeZRPFXrUBdFxKOQVNBl05c1wApCCdZrm085wuyk4Js76VcOflXowEBxTbo3rtwLObA8hvI8qRPGLJ7utP979xAPA74AVj9B4UXPJNsOOnEbMNyPeTC6NyrFinCFtMUv2sTRexMjY4lMWzVZ3BA7RbxxiAbaL0qG6QXtl1blRSKS__OV4_xN1dtSN0j0rLO21AyR4sSYRc-AlWt0uBJtGbKUU0VjXQhIIo2srYMsAgkBYcxXeZGWe7p1fO_1q-Lp9gL_S9UB_yJ8w67wLQ.ICvybfZQa3qp0VkZoWFUOFI5hnVrOenX5_3qE-Jgn3o&ApiVersion=2.0"
URL_CONFIG_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "eziocraft_url.txt")


def ensure_url_config() -> None:
    try:
        if not os.path.exists(URL_CONFIG_PATH):
            os.makedirs(os.path.dirname(URL_CONFIG_PATH), exist_ok=True)
            with open(URL_CONFIG_PATH, "w", encoding="utf-8") as config_file:
                config_file.write(DEFAULT_ZIP_URL)
    except Exception:
        pass


def load_zip_url() -> str:
    ensure_url_config()
    try:
        with open(URL_CONFIG_PATH, "r", encoding="utf-8") as config_file:
            url = config_file.read().strip()
            if url:
                return url
    except Exception:
        pass
    return DEFAULT_ZIP_URL


ZIP_URL = load_zip_url()
ZIP_NAME = "eziocraft.zip"
ZIP_PATH = os.path.join(DEFAULT_INSTALL_DIR, ZIP_NAME)
EXE_NAME = "Eziocraft.exe"  # Remplacez si nécessaire
CUSTOM_FONT_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Minecraftia-Regular.ttf")


def register_font(font_path: str) -> bool:
    if not os.path.exists(font_path):
        return False
    FR_PRIVATE = 0x10
    FR_NOT_ENUM = 0x20
    return ctypes.windll.gdi32.AddFontResourceExW(font_path, FR_PRIVATE, 0) > 0

FONT_NAME = "Minecraftia" if register_font(CUSTOM_FONT_PATH) else "Segoe UI"


def ensure_install_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)


def download_zip(url: str, dest: str) -> None:
    if "example.com" in url:
        raise RuntimeError(
            "URL du ZIP invalide. Modifiez la variable ZIP_URL dans le script pour pointer vers le fichier ZIP réel du jeu."
        )
    ensure_install_dir(os.path.dirname(dest))

    headers = {"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"}
    request = urllib.request.Request(url, headers=headers)

    max_attempts = 3
    for attempt in range(1, max_attempts + 1):
        try:
            with urllib.request.urlopen(request, timeout=600) as response, open(dest, "wb") as out_file:
                total_size = response.getheader("Content-Length")
                total_size = int(total_size) if total_size and total_size.isdigit() else 0
                downloaded = 0
                while True:
                    chunk = response.read(8192)
                    if not chunk:
                        break
                    out_file.write(chunk)
                    downloaded += len(chunk)
                    if total_size > 0:
                        percent = min(100, int(downloaded * 100 / total_size))
                    else:
                        percent = 0
                    status_label.config(text=f"Téléchargement... {percent}%")
                    root.update_idletasks()
            return
        except urllib.error.HTTPError as exc:
            error_message = f"Échec du téléchargement ({exc.code}) : {exc.reason}. Vérifiez l'URL du ZIP."
            if exc.code in (401, 403):
                error_message += f" Le lien semble privé ou expiré. Mettez à jour {URL_CONFIG_PATH} avec un lien public direct."
            raise RuntimeError(error_message)
        except Exception as exc:
            if attempt < max_attempts:
                status_label.config(text=f"Erreur réseau, nouvelle tentative {attempt + 1}/{max_attempts}...")
                root.update_idletasks()
                time.sleep(2)
                continue
            raise RuntimeError(f"Échec du téléchargement : {exc}")


def extract_zip(zip_path: str, target_dir: str) -> None:
    if not os.path.exists(zip_path):
        raise FileNotFoundError("Le fichier ZIP est introuvable.")

    with zipfile.ZipFile(zip_path, "r") as archive:
        archive.extractall(target_dir)


def find_game_executable(root_dir: str, exe_name: str) -> str | None:
    if os.path.isdir(root_dir):
        for root, _, files in os.walk(root_dir):
            if exe_name in files:
                return os.path.join(root, exe_name)

        # Fallback : retourne le premier .exe trouvé si le nom exact n'existe pas.
        for root, _, files in os.walk(root_dir):
            for file_name in files:
                if file_name.lower().endswith(".exe"):
                    return os.path.join(root, file_name)
    return None


def install_or_update() -> None:
    def _task() -> None:
        try:
            status_label.config(text="Téléchargement du zip...")
            download_zip(ZIP_URL, ZIP_PATH)
            status_label.config(text="Extraction du jeu...")
            extract_zip(ZIP_PATH, DEFAULT_INSTALL_DIR)
            status_label.config(text="Installation terminée.")
            messagebox.showinfo("Succès", "Installation / mise à jour terminée.")
        except Exception as exc:
            status_label.config(text="Erreur pendant l'installation.")
            messagebox.showerror("Erreur", str(exc))

    threading.Thread(target=_task, daemon=True).start()


def launch_game() -> None:
    exe_path = find_game_executable(DEFAULT_INSTALL_DIR, EXE_NAME)
    if exe_path is None:
        messagebox.showerror("Jeu introuvable", "Impossible de trouver l'exécutable du jeu. Installez d'abord le jeu.")
        return

    try:
        subprocess.Popen([exe_path], cwd=os.path.dirname(exe_path) or DEFAULT_INSTALL_DIR)
    except Exception as exc:
        messagebox.showerror("Erreur", f"Impossible de lancer le jeu : {exc}")


def edit_config() -> None:
    try:
        ensure_url_config()
        subprocess.Popen(["notepad.exe", URL_CONFIG_PATH], shell=True)
    except Exception as exc:
        messagebox.showerror("Erreur", f"Impossible d'ouvrir le fichier de configuration : {exc}")


def choose_install_dir() -> None:
    chosen_dir = filedialog.askdirectory(title="Choisir le dossier d'installation", initialdir=DEFAULT_INSTALL_DIR)
    if chosen_dir:
        global DEFAULT_INSTALL_DIR, ZIP_PATH
        DEFAULT_INSTALL_DIR = chosen_dir
        ZIP_PATH = os.path.join(DEFAULT_INSTALL_DIR, ZIP_NAME)
        install_label.config(text=f"Dossier d'installation : {DEFAULT_INSTALL_DIR}")


def open_install_dir() -> None:
    if os.path.isdir(DEFAULT_INSTALL_DIR):
        os.startfile(DEFAULT_INSTALL_DIR)
    else:
        messagebox.showwarning("Dossier introuvable", "Le dossier d'installation n'existe pas encore.")


root = tk.Tk()
root.title(f"{DEFAULT_GAME_NAME} Launcher")
root.geometry("420x260")
root.resizable(False, False)

base_font = tkfont.Font(root=root, family=FONT_NAME, size=10)
small_font = tkfont.Font(root=root, family=FONT_NAME, size=9)

install_label = tk.Label(root, text=f"Dossier d'installation : {DEFAULT_INSTALL_DIR}", wraplength=400, justify="left", font=small_font)
install_label.pack(pady=(12, 4))

status_label = tk.Label(root, text="Prêt", fg="blue", font=small_font)
status_label.pack(pady=(0, 12))

button_frame = tk.Frame(root)
button_frame.pack(pady=4)

install_button = tk.Button(button_frame, text="Installer / Mettre à jour", width=24, command=install_or_update, font=base_font)
install_button.grid(row=0, column=0, padx=6, pady=4)

launch_button = tk.Button(button_frame, text="Lancer le jeu", width=24, command=launch_game, font=base_font)
launch_button.grid(row=1, column=0, padx=6, pady=4)

choose_dir_button = tk.Button(button_frame, text="Changer dossier", width=24, command=choose_install_dir, font=base_font)
choose_dir_button.grid(row=0, column=1, padx=6, pady=4)

open_dir_button = tk.Button(button_frame, text="Ouvrir dossier", width=24, command=open_install_dir, font=base_font)
open_dir_button.grid(row=1, column=1, padx=6, pady=4)

edit_config_button = tk.Button(button_frame, text="Modifier le zip URL", width=50, command=edit_config, font=base_font)
edit_config_button.grid(row=2, column=0, columnspan=2, padx=6, pady=4)

exit_button = tk.Button(root, text="Quitter", width=44, command=root.quit, font=base_font)
exit_button.pack(pady=(12, 6))

root.mainloop()
