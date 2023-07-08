import os
# import json
# import glob
import shutil
import pickle
import requests
import argparse
import subprocess
from zipfile import ZipFile

base = os.getcwd()
dataset = r'content\dataset'
model = r'content\model'
genes = r'content\genes'
align = r'content\align'
crispr = r'content\crispr'
fasta_input = r'content\user\input\fasta'
align_input = r'content\user\input\align'
predict_input = r'content\user\input\predict'
crispr_input = r'content\user\input\crispr'
fasta_output = r'content\user\output\fasta'
align_output = r'content\user\output\align'
predict_output = r'content\user\output\predict'
crispr_output = r'content\user\output\crispr'
results = r'content\user\results'

dataset = os.path.join(base, dataset)
genes = os.path.join(base, genes)
model = os.path.join(base, model)
align = os.path.join(base, align)
crispr = os.path.join(base, crispr)
fasta_input = os.path.join(base, fasta_input)
align_input = os.path.join(base, align_input)
predict_input = os.path.join(base, predict_input)
crispr_input = os.path.join(base, crispr_input)
fasta_output = os.path.join(base, fasta_output)
align_output = os.path.join(base, align_output)
predict_output = os.path.join(base, predict_output)
crispr_output = os.path.join(base, crispr_output)
results = os.path.join(base, results)

folders_list = [dataset, genes, model, align, crispr, fasta_input, align_input, predict_input, crispr_input, fasta_output, align_output, predict_output, crispr_output, results]

def save_data(name, *args):
    with open(f"{name}.pkl", "wb") as f:
        pickle.dump(args, f)

def load_data(name):
    with open(f"{name}.pkl", "rb") as f:
        data = pickle.load(f)
    return data

def create_folder(folders):
    if type(folders) == str:
        if not os.path.isdir(folders):
            try:
                os.makedirs(folders)
            except FileExistsError:
                print(f"{folders} already exists.")
    elif type(folders) == list:
        for folder in folders:
            if type(folder) == str and not os.path.isdir(folder):
                try:
                  os.makedirs(folder)
                except FileExistsError:
                  print(f"{folder} already exists.")

def remove_files(folder_path):
    for filename in os.listdir(folder_path):
        file_path = os.path.join(folder_path, filename)
        try:
            os.remove(file_path)
        except OSError as e:
            print("Error: %s - %s." % (e.filename, e.strerror))

def move(source, destination):
    if os.path.isfile(source):
        create_folder(destination)
        shutil.move(source, destination)
    elif os.path.isdir(source):
        create_folder(destination)
        check = input("Source is a folder. Move folder (f) or contents (c)? ")
        if check.lower() == "f":
            shutil.move(source, destination)
        elif check.lower() == "c":
            for file in os.listdir(source):
                shutil.move(os.path.join(source, file), destination)

def run_binary(binary_path, args):
    subprocess.run([binary_path] + args)

def pip_install():
    run_binary('pip', ['install', 'gdown'])
    run_binary('pip', ['install', 'pandas'])
    run_binary('pip', ['install', 'numpy'])
    run_binary('pip', ['install', 'scikit-learn'])
    run_binary('pip', ['install', 'bio'])
    run_binary('pip', ['install', 'pycaret'])

def drive_download(url, name, destination):
    create_folder(destination)
    run_binary('gdown', [url, "-O", os.path.join(destination, name)])

def unzip_file(source, destination):
    with ZipFile(source, 'r') as zip:
        zip.extractall(destination)

def download_and_unzip(url, name, destination):
    create_folder(destination)
    r = requests.get(url, stream=True)
    with open(os.path.join(destination, name), 'wb') as f:
        for chunk in r.iter_content(chunk_size=1024):
            f.write(chunk)
    unzip_file(os.path.join(destination, name), destination)

def prepare_env(install_pip='y'):
    install_packages = input('Do you want to install the required packages? (y/n) ')
    if str(install_pip).lower() == 'y' or install_packages.lower() == 'y':
        pip_install()
    create_folder(folders_list)
    drive_download('1dHCjsggAjkzkJRMvSt2omRKWnAa5bRWs', 'genes.json', genes)
    drive_download('1DWtSNyjrjzK6RdWMjlmnNlQwwOkEv-nI', 'Alignment.exe', align)
    drive_download('1fiPncYHi5DykH286nIUlOjGfUXpbDQTf', 'sgRNAScorer.2.0.zip', crispr)
    unzip_file(os.path.join(crispr, 'sgRNAScorer.2.0.zip'), crispr)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('-i', "--install_pip", default='', help="Install requirements? (y/any) ",required=False)
    args = parser.parse_args()
    
    install_pip = args.install_pip
    prepare_env(install_pip=install_pip)
    
if __name__ == "__main__":
    main()
