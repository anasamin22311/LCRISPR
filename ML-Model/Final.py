import os
import re
import json
import glob
import shutil
import pickle
import argparse
import subprocess
import pandas as pd
from ast import literal_eval
from Bio import SeqIO 
from Bio.Seq import Seq
from Bio.SeqRecord import SeqRecord
from content.align.Aligner import align_str_sequences
from pycaret.classification import *
from concurrent.futures import ThreadPoolExecutor, as_completed

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
scorer = os.path.join(crispr, r'sgRNAScorer.2.0\Standalone\identifyAndScore.py')
fasta_input = os.path.join(base, fasta_input)
align_input = os.path.join(base, align_input)
predict_input = os.path.join(base, predict_input)
crispr_input = os.path.join(base, crispr_input)
fasta_output = os.path.join(base, fasta_output)
align_output = os.path.join(base, align_output)
predict_output = os.path.join(base, predict_output)
crispr_output = os.path.join(base, crispr_output)
results = os.path.join(base, results)

requirements = False
allow_user_input = False #True

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

def move(source, destination, skip=False):
    if os.path.isfile(source):
        create_folder(destination)
        shutil.move(source, destination)
    elif os.path.isdir(source):
        create_folder(destination)
        check = 'c'
        if skip:
            check = input("Source is a folder. Move folder (f) or contents (c)? ")
        if check.lower() == "f":
            shutil.move(source, destination)
        elif check.lower() == "c":
            for file in os.listdir(source):
                shutil.move(os.path.join(source, file), destination)

def read_csv(csv, start=0, end=0, sample=0, sep=',', converters=None):
    df = pd.read_csv(csv, sep=sep, converters=converters)
    if start > 0 and start < len(df):
        end -= start
        df = df[start:]
    if end > 0 and end < len(df):
        df = df[:end]
    if sample > 0 and sample < len(df):
        df = df.sample(sample)
    return df

def _read_json(json_file):
    with open(json_file) as f:
        return json.load(f)

def _json_to_dataframe(data):
    df = pd.DataFrame(columns=['GeneName', 'GeneID', 'CDSID', 'CDSNT', 'CDSAA'])
    rows = []
    for gene in data:
            gene_name = gene['GeneName']
            gene_id = gene['GeneID']
            if not 'CCDS' in gene:
                continue
            for CCD in gene["CCDS"]:
                CDS_ID = CCD['CDSID']
                CDS_NT = CCD['CDSNT']
                CDS_AA = CCD['CDSAA']
                rows.append({
                    'GeneName': gene_name,
                    'GeneID': gene_id,
                    'CDSID': CDS_ID,
                    'CDSNT': CDS_NT,
                    'CDSAA': CDS_AA,
                })
    df = pd.DataFrame(rows)
    return df

def df_to_json(df, file_path):
  json_string = df.to_json(orient="records")
  with open(file_path, "w") as file:
    file.write(json_string)

def _read_fasta_files(folder_path=fasta_input):
    fasta_files = glob.glob(folder_path + "/*.fasta")
    if len(fasta_files) == 0:
        print(f'No .fasta files found in "{folder_path}".')
        return None
    else:
        dfs = []
        for file in fasta_files:
            df = pd.read_fwf(file, colspecs="infer", header=None)
            name, details = df.iloc[0][0][1:].split(" ", 1)
            sequence = "".join(df.iloc[1:][0].tolist())
            new_df = pd.DataFrame({"Name": [name], "Details": [details], "Sequence": [sequence]})
            dfs.append(new_df)
        final_df = pd.concat(dfs, ignore_index=True)
        return final_df

def write_fasta_file(name, seq, description="", output=os.path.join(fasta_input, 'Seq.fasta')):
    if not name or not seq:
        return
    record = SeqRecord(Seq(seq), id=name, description=description)
    with open(output, "w") as output_handle:
        SeqIO.write(record, output_handle, "fasta")

def get_user_query(input_path=fasta_input, output_path=os.path.join(fasta_output, 'query.csv')):
    df = _read_fasta_files(input_path)
    if type(df) != pd.DataFrame:
        print(f'please add the user query in the "{fasta_output}" as "query.csv"')
        return None
    else:
        df.to_csv(output_path, index=False)
        return df

def _genes_to_csv(path, name):
    data = _read_json(path)
    df = _json_to_dataframe(data)
    df.to_csv(os.path.join(genes, name), index=False)
    return df

def prepare_genes(gene_json_path=os.path.join(genes, 'genes.json'), gene_csv_path=os.path.join(genes, 'genes.csv')):
    df = pd.DataFrame()
    if os.path.isfile(gene_json_path):
        if not os.path.isfile(gene_csv_path):
            df = _genes_to_csv(gene_json_path, 'genes.csv')
        else:
            create_genes_check = ''
            if allow_user_input:
                create_genes_check = input('Do tou want to recreate the genes file? (y/n) ')
            if requirements or create_genes_check.lower() == 'y':
                df = _genes_to_csv(os.path.join(genes, 'genes.json'), 'genes.csv')
            else:
                df = pd.read_csv(os.path.join(genes, 'genes.csv'))
        return df
    else:
        return False

def drive_download(url, name, destination):
    create_folder(destination)
    subprocess.run(["gdown", url, "-O", os.path.join(destination, name)])

def get_dataset(destination=dataset):
    check_dataset = ''
    if allow_user_input:
        check_dataset = input('Download the dataset (you can skip it and download the model directly)? (y/n) ')
    if requirements or check_dataset.lower() == 'y':
        drive_download('1wrx0CskTnatLS3PDZhdfb8WzwLRPiXup', 'dataset.csv', destination)
    return check_dataset

def get_model(destination=model):
    check_model = ''
    if allow_user_input:
        check_model = input('Download the model? (y/n) ')
    if requirements or check_model.lower() == 'y':
        drive_download('1-clGIJZlTxOv6sa0SFFQF6-LNI3jcQKw', 'model.pkl', destination)
    return check_model

def _split_variant_generic(df):
    new_df = pd.DataFrame(columns=['start', 'end', 'change'])
    for index, row in df.iterrows():
        variant = row['CDS_Variant']
        regex = r'([cgp]\.)?(\d+|\*\d+|-\d+)([+-]\d+)?(_(\d+|\*\d+|-\d+)([+-]\d+)?)?([a-zA-Z]+.*)'
        match = re.match(regex, variant)
        if match:
            prefix = match.group(1)
            base_position_before = match.group(2)
            offset_before = match.group(3)
            underscore_and_base_position_after = match.group(4)
            base_position_after = match.group(5)
            offset_after = match.group(6)
            change = match.group(7)
            if underscore_and_base_position_after:
                if offset_before:
                    start = base_position_before + offset_before
                else:
                    start = base_position_before
                if offset_after:
                    end = base_position_after + offset_after
                else:
                    end = base_position_after
            else:
                if offset_before:
                    start = base_position_before + offset_before
                    end = start
                else:
                    start = base_position_before
                    end = base_position_before
            new_df.loc[index] = [start, end, change]
        else:
            new_df.loc[index] = [None, None, None]
    new_df.drop("CDS_Variant", axis=1, inplace=True)
    return new_df

def _remove_spaces(df):
    for column in df.columns:
        if df[column].dtype == 'object':
            df[column] = df[column].str.strip()
    return df

def make_model(df, save_path=os.path.join(model, 'model')):
    df = _remove_spaces(df)
    if 'CDS_Variant' in df.columns:
        df = df[["GeneName", "CDNID", "CDS_Variant", "Consequence", "clinvar_clnsig"]]
        df['Consequence'] = df['Consequence'].str.replace('variant', '')
        df = _split_variant_generic(df)
    if 'start' in df.columns and 'end' in df.columns and 'change' in df.columns:
        print('Total dataset:', df.shape[0])
        if type(df['CDNID']) != str:
            df['CDNID'] = df['CDNID'].astype(str)
        clf = setup(data=df, target='clinvar_clnsig', session_id=963, fold=10, train_size=0.75, feature_selection=False, preprocess=True, fix_imbalance=True)
        best_model = compare_models()
        tuned_model = tune_model(best_model, early_stopping=True)
        ensembled_model = ensemble_model(tuned_model)
        final_model = finalize_model(tuned_model)
        evaluate_model(final_model)
        eda(display_format="bokeh")
        save_model(final_model, save_path)
        return final_model
    return None, None, None

def use_model(df, model_path=os.path.join(model, 'model')):
    df = _remove_spaces(df)
    crispr_model = load_model(model_path)
    df = df[['GeneName', 'CDNID', 'Consequence', 'start', 'end', 'change', 'clinvar_clnsig']]
    df['Consequence'] = df['Consequence'].str.replace('variant', '')
    predictions = predict_model(crispr_model, data=df)
    return predictions

def prepare_model(dataset_path=os.path.join(dataset, 'dataset.csv'), model_path=os.path.join(model, 'model')):
    get_dataset(dataset)
    get_model(model)
    check_dataset = True
    check_model = True
    final_model = None
    unseen = None
    prediction = None
    if not os.path.isfile(dataset_path):
        check_dataset = False
        print("The dataset folder does not have that dataset.csv you won't be able to create/test a new model")
    if not os.path.isfile(model_path + '.pkl'):
        check_model = False
        print("The model folder does not have that model.pkl you won't be able to make predictions")
    if check_dataset:
        create_model_check = ''
        if allow_user_input: 
            create_model_check = input('Do tou want to create the model? (y/n) ')
        if requirements or create_model_check.lower() == 'y':
            final_model, unseen, prediction = make_model(read_csv(dataset_path))
            check_model = True
    if check_dataset and check_model:
        create_model_check = ''
        if allow_user_input: 
            create_model_check = input('Do you want to test the model? (y/n) ')
        if requirements or create_model_check.lower() == 'y':
            df = read_csv(dataset_path, 0, 0, 10000)
            use_model(df, model_path)
    return final_model, unseen, prediction

def _run_binary(binary_path, args):
    result = subprocess.run([binary_path] + args, capture_output=True, text=True)
    print(result.stdout)
    print(result.stderr)

def select_sequences(sequences_df, selection, column='Name'):
    if selection:
        selection = list(map(str.lower, selection))
        sequences_df = sequences_df[sequences_df[column].str.lower().isin(selection)]
    return sequences_df

def align_sequences(user_query_path=os.path.join(fasta_output, 'query.csv'), genes_data_path=os.path.join(genes, 'genes.csv'),
                    select_user=[], select_genes=[], aligner_path=os.path.join(align, 'py'),
                    output_name=os.path.join(align_output, 'Alignment_output.csv')):
    user_query = select_sequences(read_csv(user_query_path).dropna(), select_user)
    genes_data = select_sequences(read_csv(genes_data_path).dropna(), select_genes, 'GeneName')
    for index_query, query_row in user_query.iterrows():
        for index_genes, genes_row in genes_data.iterrows():
            name1 = query_row['Name']
            seq1 = query_row['Sequence']
            name2 = f"{genes_row['GeneName']}-{genes_row['CDSID']}"
            seq2 = ''.join(eval(genes_row['CDSNT']))
            output = output_name
            align_str_sequences(name1=name1, seq1=seq1, name2=name2, seq2=seq2, output=output)

def get_top_from_alignment(df):
    return df.loc[df.groupby('ref_name')['similarity'].idxmax()]

def hits_to_csv(df, align, output_path=os.path.join(crispr_input, 'hits.csv')):
    hits = get_top_from_alignment(align)
    hits = df[df['Name'].isin(hits['ref_name'])]
    hits.to_csv(output_path, index=False)
    return hits

def prepare_alignment_output(df):
    df = get_top_from_alignment(df)
    return df

def prepare_variants(df):
    new_rows = []
    for _, row in df.iterrows():
        ref_name = row['ref_name']
        tar_name = row['tar_name']
        gene_name, cdnid = tar_name.split('-')
        variants = eval(row['variants'])
        for variant in variants:
            try:
                consequence = variant[1]
                start = variant[2]
                end = variant[3]
                change = variant[4]
                start_index = variant[5]
                end_index = variant[6]
                new_rows.append([ref_name, gene_name, cdnid, consequence, start, end, change, start_index, end_index, 0])
            except:
                print(f'error here {variant}')
    variants = pd.DataFrame(new_rows, columns=['ref_name', 'GeneName', 'CDNID', 'Consequence', 'start', 'end', 'change', 'start_index', 'end_index', 'clinvar_clnsig'])
    return variants

def extract_sgRNA(binary_path='python', script=os.path.join(crispr, 'crispr.py'),
                  input_csv=os.path.join(crispr_input, 'hits.csv'), output_csv=os.path.join(crispr_output, 'sgRNAs.csv'),
                  crispr_scorer=scorer, spacer_length=21, pam_orientation=3, pam_sequence='NGG'):
    binary_path = 'python'
    args = [
        script,
        '-i', input_csv,
        '-o', output_csv,
        '-c', crispr_scorer,
        '-s', str(spacer_length),
        '-p', str(pam_orientation),
        '-l', pam_sequence,
    ]
    _run_binary(binary_path, args)
    
def final_prepare():
    prepare_genes()
    final_model, unseen, unseen_prediction = prepare_model()
    return final_model, unseen, unseen_prediction

def user_input(input_path=fasta_input, output_path=os.path.join(fasta_output, 'query.csv')):
    user_query = get_user_query(input_path, output_path)
    if type(user_query) == type(None):
        return user_query, False
    return user_query, True

def make_alignment(user_query_path=os.path.join(fasta_output, 'query.csv'), genes_data_path=os.path.join(genes, 'genes.csv'),
                   select_user=[], select_genes=[], output_name=os.path.join(align_output, 'Alignment_output.csv')):
    align_sequences(select_user=select_user, select_genes=select_genes, output_name=output_name)
    align_results = read_csv(output_name, sep=';')
    align_results = prepare_alignment_output(align_results)
    user_query = read_csv(user_query_path)
    hits = hits_to_csv(user_query, align_results, output_path=os.path.join(crispr_input, 'hits.csv'))
    hits.to_csv(os.path.join(fasta_output, 'hits.csv'), index=False)
    variants = prepare_variants(align_results)
    variants = variants[['ref_name', 'GeneName', 'CDNID', 'Consequence', 'start', 'end', 'change', 'clinvar_clnsig']]
    variants.to_csv(os.path.join(predict_input, 'variants.csv'), index=False)
    variants.reset_index(inplace=True)
    return align_results, hits, variants

def create_prediction_output(df, output_path=os.path.join(predict_output, 'prediction.csv')):
    df = df.rename(columns={'name1': 'query'})
    df.reset_index(drop=True, inplace=True)
    df.to_csv(output_path, index=False)
    return df
    
def make_prediction(variants=os.path.join(predict_input, 'variants.csv')):
    variants_df = read_csv(variants)
    variants_df.reset_index(drop=True, inplace=True)
    model_df = variants_df[['GeneName', 'CDNID', 'Consequence', 'start', 'end', 'change', 'clinvar_clnsig']]
    model_df['Consequence'] = model_df['Consequence'].str.replace('variant', '')
    predict_df = use_model(model_df)
    variants_df[['clinvar_clnsig', 'prediction_score']] = predict_df[['prediction_label', 'prediction_score']]
    variants_df = create_prediction_output(variants_df, output_path=os.path.join(predict_output, 'prediction.csv'))
    return variants_df
    

def klsny(select_user=[], select_genes=[], final_result=os.path.join(results, 'output.json'),
        alignment=os.path.join(align_output, 'Alignment_output.csv'), sep=';',
        spacer_length=21, pam_orientation=3, pam_sequence='NGG'):
    align_sequences(select_user=select_user, select_genes=select_genes, output_name=alignment)
    Alignment_df = read_csv(alignment, sep=sep)
    Alignment_df = get_top_from_alignment(Alignment_df)
    Alignment_df = Alignment_df.reset_index(drop=True)
    variants_df = prepare_variants(Alignment_df)
    prediction_df = use_model(variants_df[['ref_name', 'GeneName', 'CDNID', 'Consequence', 'start', 'end', 'change', 'clinvar_clnsig']])
    variants_df[['clinvar_clnsig', 'prediction_score']] = prediction_df[['prediction_label', 'prediction_score']]
    groups = variants_df.groupby("ref_name").apply(lambda x: x.values.tolist())
    result = groups.tolist()
    ref_dict = dict(zip(groups.index, result))
    Alignment_df["variants"] = Alignment_df["ref_name"].map(ref_dict)
    hits = pd.DataFrame()
    hits['Name'] = Alignment_df['ref_name']
    hits['Sequence'] = Alignment_df['ref'].str.replace('-', '')
    hits.to_csv(os.path.join(crispr_input, 'hits.csv'), index=False)
    extract_sgRNA(spacer_length=spacer_length, pam_orientation=pam_orientation, pam_sequence=pam_sequence)
    gRNA_df = read_csv(os.path.join(crispr_output, 'sgRNAs.csv'), sep=';', converters={'sgRNAs': literal_eval})
    gRNA_df = gRNA_df.drop(columns="Sequence")
    Alignment_df = Alignment_df.merge(gRNA_df, left_on="ref_name", right_on="Name", how="left")
    Alignment_df = Alignment_df.drop(columns="Name")
    df_to_json(Alignment_df, final_result)
    return Alignment_df


def execute(select_user=[], select_genes=[], spacer_length=21, pam_orientation=3, pam_sequence='NGG'):
    remove_files(results)
    remove_files(align_input)
    remove_files(predict_input)
    remove_files(crispr_input)
    remove_files(fasta_output)
    remove_files(align_output)
    remove_files(predict_output)
    remove_files(crispr_output)
    final_prepare()
    Alignment_df = None
    user_query, query_exists = user_input()
    if query_exists:
        Alignment_df = klsny(select_user=select_user, select_genes=select_genes, spacer_length=spacer_length, 
                             pam_orientation=pam_orientation, pam_sequence=pam_sequence)
    remove_files(fasta_input)
    remove_files(align_input)
    remove_files(predict_input)
    remove_files(crispr_input)
    remove_files(fasta_output)
    remove_files(align_output)
    remove_files(predict_output)
    remove_files(crispr_output)
    return Alignment_df

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('-i', "--allow_user_input", help="Allow user input? (y/any) ", default='',required=False)
    parser.add_argument('-r', "--requirements", help="Download requirements? (y/any) ", default='',required=False)
    parser.add_argument('-u', "--select_from_user", help="Select a list of the user query sequences seperated by ',' to process on (leave empty for all)",required=False)
    parser.add_argument('-g', "--select_genes", help="Select a list of the target genes sequences seperated by ',' to process on (leave empty for all)", required=False)
    parser.add_argument('-s', "--spacer_length", default=21, help="Spacer length", required=False)
    parser.add_argument('-p', "--pam_orientation", default=3, help="PAM orientation", required=False)
    parser.add_argument('-l', "--pam_sequence", default="NGG", help="PAM sequence", required=False)
    args = parser.parse_args()
    
    global requirements
    global allow_user_input
    
    if args.allow_user_input == 'y':
        allow_user_input = True

    if args.requirements == 'y':
        requirements = True
    
    select_user = args.select_from_user
    select_genes = args.select_genes
    if select_user:
        select_user = str(select_user).split(',')
    if select_genes:
        select_genes = str(select_genes).split(',')
    spacer_length = args.spacer_length
    pam_orientation = args.pam_orientation
    pam_sequence = args.pam_sequence
    
    execute(select_user=select_user, select_genes=select_genes,
            spacer_length=spacer_length, pam_orientation=pam_orientation, pam_sequence=pam_sequence)
    
if __name__ == "__main__":
    main()
