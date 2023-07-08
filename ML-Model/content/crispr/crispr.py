import os
import re
import argparse
import subprocess
import pandas as pd

base = os.getcwd()
base = os.path.join(base, 'content\crispr')
def normalize_string(s):
    return re.sub(r'[^a-zA-Z0-9]+', '_', s)
def run_sgrna_scorer(sequence, pam_orientation, spacer_length, pam_sequence, output_file='Temp'):
    with open(os.path.join(base, "sgRNAScorer.2.0\Standalone\Temp", output_file+".fasta"), "w") as f:
        f.write(">{}\n{}".format(output_file, sequence))
    args = [
        "python", 
        os.path.join(base, "sgRNAScorer.2.0\Standalone\identifyAndScore.py"),
        "-i", os.path.join(base, "sgRNAScorer.2.0\Standalone\Temp", output_file+".fasta"),       
        "-o", os.path.join(base, "sgRNAScorer.2.0\Standalone\Temp", output_file+".txt"),
        "-p", pam_orientation,
        "-s", spacer_length,    
        "-l", pam_sequence
    ]  
    subprocess.call(args)
    print()
    with open(os.path.join(base, "sgRNAScorer.2.0\Standalone\Temp", output_file+".txt")) as f:
        lines = f.read().splitlines()
    columns = ["SeqID", "Sequence", "Score"]
    df = pd.DataFrame(columns=columns)
    for line in lines:
        row = line.split("\t")
        df.loc[len(df)] = row
    os.remove(os.path.join(base, "sgRNAScorer.2.0\Standalone\Temp", output_file+".fasta"))
    os.remove(os.path.join(base, "sgRNAScorer.2.0\Standalone\Temp", output_file+".txt"))
    return df

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('-i', "--input", help="Input CSV file",type=argparse.FileType('r'),required=True)
    parser.add_argument('-o', "--output", help="Output CSV file", default="Output.csv", required=False)
    parser.add_argument('-c', "--crispr_program", default=os.path.join(base, "sgRNAScorer.2.0\Standalone\identifyAndScore.py"), help="sgRNA scorer path (identifyAndScore.py)", required=False)
    parser.add_argument('-s', "--spacer_length", default=21, help="Spacer length", required=False)
    parser.add_argument('-p', "--pam_orientation", default=3, help="PAM orientation", required=False)
    parser.add_argument('-l', "--pam_sequence", default="NGG", help="PAM sequence", required=False)
    args = parser.parse_args()
    df = pd.read_csv(args.input)  
    df = df.reset_index(drop=True)
    df["sgRNAs"] = None
    for index, row in df.iterrows():  
        sequence = row["Sequence"]
        name = row["Name"]
        name = normalize_string(name)
        output_file = name + "_sgrnas"   
        df_sgrna = run_sgrna_scorer(sequence, args.pam_orientation, args.spacer_length, args.pam_sequence, output_file)
        # sgrna_data = {
        #     "SeqID": "|".join(df_sgrna["SeqID"].tolist()),  
        #     "sgRNA": "|".join(df_sgrna["Sequence"].tolist()),
        #     "Score": "|".join(df_sgrna["Score"].tolist()) 
        # }
        # for col in ["SeqID", "sgRNA", "Score"]:
        #     if col not in df.columns: 
        #         df[col] = None
        df["sgRNAs"][index] = df_sgrna.values.tolist()[1:]
    df = df.reset_index(drop=True)
    df.to_csv(args.output, index=False, sep=';')
if __name__ == "__main__":
    main()
