import os
import re
import uuid
import Final
from flask import Flask, request, jsonify, send_file
from werkzeug.utils import secure_filename


############################################################################
#                                                                          #
#   Run http://127.0.0.1:5000/fasta and pass the fasta file                # 
#   Run http://127.0.0.1:5000/dna and pass the Name and Sequence as JSON   # 
#                                                                          #
############################################################################


base = os.getcwd()
fasta_input = r'content\user\input\fasta'
results = r'content\user\results\output.json'
fasta_input = os.path.join(base, fasta_input)
results = os.path.join(base, results)

def remove_files(folder_path):
    for filename in os.listdir(folder_path):
        file_path = os.path.join(folder_path, filename)
        try:
            os.remove(file_path)
        except OSError as e:
            print("Error: %s - %s." % (e.filename, e.strerror))

app = Flask(__name__)

def validate_fasta(file):
    if file.read() == b'':
        return False, "Empty file"
    file.seek(0)
    first_line = file.readline().decode()
    if not first_line.startswith('>'):
        return False, "Invalid header"
    sequence = ""
    for line in file:
        line = line.decode().strip()
        if line == "" or line.startswith('>'):
            if sequence:
                continue
            return False, "Invalid format"
        if not re.match("^[ATCGatcg]+$", line):
            if sequence:
                continue
            return False, "Invalid characters"
        sequence += line
    return True, sequence

def validate_dna(sequence):
    if sequence == "":
        return False, "Empty sequence"
    if not re.match("^[ATCGatcg]+$", sequence):
        return False, "Invalid characters"
    return True, sequence

@app.route('/fasta', methods=['POST'])
def fasta():
    if 'file' not in request.files:
        return jsonify({"error": "No file provided"}), 400
    file = request.files['file']
    valid, result = validate_fasta(file)
    if valid:
        remove_files(fasta_input)
        filename = str(uuid.uuid4())
        if file.filename:
            filename = secure_filename(file.filename)
        filename = f'{str(uuid.uuid4())} - {filename}'
        path = os.path.join(fasta_input, filename)
        file.seek(0)
        file.save(path)
        try:
            Final.execute(select_genes=[])
            return send_file(results)
        except:
            remove_files(path)
    return jsonify({"error": result}), 400

@app.route('/dna', methods=['POST'])
def dna():
    if not request.is_json:
        return jsonify({"error": "No json data provided"}), 400
    data = request.get_json()
    if 'sequence' not in data:
        return jsonify({"error": "No sequence provided"}), 400
    name = data['name']
    if name:
        name = secure_filename(name)
    else:
        name = 'Query'
    sequence = data['sequence']
    valid, result = validate_dna(sequence)
    if valid:
        path = os.path.join(fasta_input, f'{str(uuid.uuid4())} - {name}.fasta')
        try:
            remove_files(fasta_input)
            Final.write_fasta_file(name, sequence, 'Reference', output=path)
            Final.execute(select_genes=[])
            return send_file(results)
        except:
            remove_files(path)
    return jsonify({"error": result}), 400

if __name__ == '__main__':
    app.run(port=5000)
