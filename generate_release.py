import os
import zipfile
from pathlib import Path

os.chdir(os.path.dirname(os.path.abspath(__file__)))
PLUGINS_DIR: str = Path("../../BepInEx/plugins")
MODS_DIR: str = Path("../../user/mods")

#SCRIPT CONFIG:
MOD_NAME: str = "LeaveItThere"
ZIP_DIRS: list[str] = [
    Path("../../BepInEx/plugins/LeaveItThere"),
    Path("../../user/mods/LeaveItThere-Server"),
]
IGNORE_FILE_EXTENSIONS: list[str] = [
    ".js",
    ".js.map",
    ".zip"
]

def clean_path(path: str):
    parts = path.split(os.path.sep)
    clean_path = ""

    for part in parts:
        if part == "..": continue
        clean_path = Path(clean_path, part)

    return clean_path

def should_include_file(file):
    return not any(file.endswith(ext) for ext in IGNORE_FILE_EXTENSIONS)

def process_directory(zipf, dir: str):
    for root, dirs, files in os.walk(dir):
        for file in files:
            if should_include_file(file):
                file_path = os.path.join(root, file)
                arcname = clean_path(file_path)
                zipf.write(file_path, arcname)

def zip_directories(dirs: list[str], output_zip: str):
    with zipfile.ZipFile(output_zip, "w", zipfile.ZIP_DEFLATED) as zipf:
        for dir in dirs:
            process_directory(zipf, dir)

zip_directories(ZIP_DIRS, f"{MOD_NAME}-.zip")