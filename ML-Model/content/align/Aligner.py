import os
import uuid
import argparse
import pandas as pd
import subprocess as sb
from Bio import SeqIO 
from Bio.Seq import Seq
from Bio.SeqRecord import SeqRecord

base = os.getcwd()
base = os.path.join(base, r'content\align')

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

def align(input='Sequence.fasta', output='align_output.fasta', binary=os.path.join(base, 'muscle.exe'),
          match_award=10, similar_mismatch_penalty=5, mismatch_penalty=-5, gap_penalty=-10):
    
    bases = ['A', 'T', 'G', 'C']
    args = ['-align', input, '-output', output]

    def run_binary(binary, args):
        sb.run([binary] + args)

    def get_sequences(alignment=output, ref_index=0):
        ref_seq = None
        seqs = []
        index = 0
        for seq_record in SeqIO.parse(alignment, "fasta"):
            if index == ref_index:
                ref_seq = seq_record
            else:
                seqs.append(seq_record)
            index += 1
        return ref_seq, seqs

    def get_codon(index, sequence):
        try:
            if len(sequence) < 3 or index < 0 or index >= len(sequence):
                return "-"
            codon_start = (index // 3) * 3
            if codon_start + 3 > len(sequence):
                return "-"
            return sequence[codon_start:codon_start+3]
        except:
            return "-"

    def compare_codons(codon1, codon2):
        if any(base not in bases for base in codon1) or any(base not in bases for base in codon2):
            return False
        genetic_code = {
            "ATA": 'I', "ATC": 'I', "ATT": 'I', "ATG": 'M',
            "ACA": 'T', "ACC": 'T', "ACG": 'T', "ACT": 'T',
            "AAC": 'N', "AAT": 'N', "AAA": 'K', "AAG": 'K',
            "AGC": 'S', "AGT": 'S', "AGA": 'R', "AGG": 'R',
            "CTA": 'L', "CTC": 'L', "CTG": 'L', "CTT": 'L',
            "CCA": 'P', "CCC": 'P', "CCG": 'P', "CCT": 'P',
            "CAC": 'H', "CAT": 'H', "CAA": 'Q', "CAG": 'Q',
            "CGA": 'R', "CGC": 'R', "CGG": 'R', "CGT": 'R',
            "GTA": 'V', "GTC": 'V', "GTG": 'V', "GTT": 'V',
            "GCA": 'A', "GCC": 'A', "GCG": 'A', "GCT": 'A',
            "GAC": 'D', "GAT": 'D', "GAA": 'E', "GAG": 'E',
            "GGA": 'G', "GGC": 'G', "GGG": 'G', "GGT": 'G',
            "TCA": 'S', "TCC": 'S', "TCG": 'S', "TCT": 'S',
            "TTC": 'F', "TTT": 'F', "TTA": 'L', "TTG": 'L',
            "TAC": 'Y', "TAT": 'Y', "TAA": '_', "TAG": '_',
            "TGC": 'C', "TGT": 'C', "TGA": '_', "TGG": 'W'
        }
        aa1 = genetic_code.get(codon1.upper(), '')
        aa2 = genetic_code.get(codon2.upper(), '')
        if aa1 == '' or aa2 == '':
            return False
        return aa1 == aa2

    def is_similar(index1, sequence1, index2, sequence2):
        codon1 = get_codon(index1, sequence1)
        codon2 = get_codon(index2, sequence2)
        return compare_codons(codon1, codon2)

    def match_score(index1, sequence1, index2, sequence2):
        if sequence1[index1] == sequence2[index2]: return match_award, '|'
        elif sequence1[index1] == '-' or sequence2[index2] == '-': return gap_penalty, ' '
        elif is_similar(index1, sequence1, index2, sequence2): return similar_mismatch_penalty, ':'
        else: return mismatch_penalty, '.'

    def get_variant_type(start_index, end_index, seq1, seq2, sybmol):
        variant_type = ''
        if sybmol[start_index:end_index] == len(sybmol[start_index:end_index]) * sybmol[start_index]:
            if seq1[start_index] in bases and seq2[start_index] in bases:
                if sybmol[start_index] == '|' and seq1[start_index] == seq2[start_index]:
                    variant_type = ''
                elif sybmol[start_index] == ':' and is_similar(start_index, seq1, start_index, seq2):
                    variant_type = 'S'
                elif sybmol[start_index] == '.':
                    variant_type = 'M'
            elif sybmol[start_index] == ' ':
                if seq1[start_index:end_index] == len(seq1[start_index:end_index]) * '-' and all(base in bases for base in seq2[start_index:end_index]):
                    variant_type = 'D'
                elif seq2[start_index:end_index] == len(seq2[start_index:end_index]) * '-' and all(base in bases for base in seq1[start_index:end_index]):
                    variant_type = 'I'
        return variant_type

    def format_variant(start_position, index, ref_seq, tar_seq, variant_type):
        if variant_type != '':
            if variant_type == 'D':
                return make_variant(start_position+1, index, start_position, index, ref_seq, tar_seq, variant_type)
            elif variant_type == 'I':
                return make_variant(start_position, start_position+1, start_position, index, ref_seq, tar_seq, variant_type)
            elif variant_type == 'S' or variant_type == 'M':
                return make_variant(start_position, start_position, index-1, index, ref_seq, tar_seq, variant_type)
        return ''

    def make_variant(start_pos, end_pos, start_index, end_index, seq1, seq2, variant_type):
        start_pos -= seq2[:start_index].count('-')
        end_pos -= seq2[:start_index].count('-')
        seq1 = seq1[start_index:end_index]
        seq2 = seq2[start_index:end_index]
        if variant_type.upper() == 'S':
            return [f'{start_pos}{seq2}>{seq1}','synonym variant', start_pos, start_pos, f'{seq2}>{seq1}', start_index, start_index]
        elif variant_type.upper() == 'M':
            return [f'{start_pos}{seq2}>{seq1}', 'missens variant', start_pos, start_pos, f'{seq2}>{seq1}', start_index, start_index]
        elif variant_type.upper() == 'D':
            return [f'{start_pos}_{end_pos}del', 'frameshift variant', start_pos, end_pos, 'del', start_index, end_index]
        elif variant_type.upper() == 'I':
            return [f'{start_pos}_{end_pos}ins{seq1}', 'frameshift variant', start_pos, end_pos, f'ins{seq1}', start_index, end_index]
        return None

    def get_HGVSish_variants(ref_seq, tar_seq):
        symbol, identity, similarity, score, variants, start_position, variant_type = '', 0, 0, 0, [], 0, ''
        for j in range(len(ref_seq)):
            current_score, current_symbol = match_score(j, ref_seq, j, tar_seq)
            symbol += current_symbol
            score += current_score
            if variant_type != '' and variant_type != 'D' and variant_type != 'I':
                variant = format_variant(start_position, j, ref_seq, tar_seq, variant_type)
                if variant:
                    variants.append(variant)
                variant_type = ''
            if current_symbol == ' ' or variant_type == 'D' or variant_type == 'I':
                temp = get_variant_type(start_position, j+1, ref_seq, tar_seq, symbol)
                if variant_type != '' and variant_type != temp:
                    variant = format_variant(start_position, j, ref_seq, tar_seq, variant_type)
                    if variant:
                        variants.append(variant)
                    start_position = j + 1
                variant_type = temp
            if current_symbol == '|':
                identity += 1
                similarity += 1
                start_position = j + 1
            elif current_symbol == ':':
                variant_type = get_variant_type(j, j+1, ref_seq, tar_seq, symbol)
                similarity += 1
                start_position = j + 1
            elif current_symbol == '.':
                variant_type = get_variant_type(j, j+1, ref_seq, tar_seq, symbol)
                start_position = j + 1
        if variant_type != '':
            variant = format_variant(start_position, len(ref_seq), ref_seq, tar_seq, variant_type)
            if variant:
                variants.append(variant)
            variant_type = ''
        if len(ref_seq) > 0:
            identity = identity / len(ref_seq) * 100
            similarity = similarity / len(ref_seq) * 100
        return {'ref': ref_seq,'tar': tar_seq,'symbol': symbol, 'variants': variants, 'identity': identity, 'similarity': similarity, 'score': score}

    def get_multi_HGVSish_variants(ref_seq, seqs):
        multi_HGVSish_variants = [] 
        for seq in seqs:
            results = get_HGVSish_variants(ref_seq.seq, seq.seq)
            results.update({'ref_name': ref_seq.id, 'tar_name': seq.id})
            multi_HGVSish_variants.append(results)
        return multi_HGVSish_variants

    run_binary(binary=binary, args=args)
    ref_seq, seqs = get_sequences()
    multi_HGVSish_variants = get_multi_HGVSish_variants(ref_seq, seqs)
    return multi_HGVSish_variants

def align_str_sequences(name1="reference", seq1='', name2="target", seq2='', output=os.path.join(base, 'variants.csv'), temp_path=os.path.join(base, 'Temp')):
    # print(temp_path)
    create_folder(temp_path)
    
    if seq1 == '' or seq2 == '':
        return None
    name1 += f' - {str(uuid.uuid4())}'
    name2 += f' - {str(uuid.uuid4())}'

    record1 = SeqRecord(Seq(seq1), id=name1, description="Reference")
    record2 = SeqRecord(Seq(seq2), id=name2, description="Target")

    fasta_path = os.path.join(temp_path, f'{name1}_{name2}.fasta')

    with open(fasta_path, "w") as output_handle:
        SeqIO.write([record1, record2], output_handle, "fasta")
    
    multi_HGVSish_variants = align(fasta_path, output=os.path.join(temp_path, f'aligned_{name1}_{name2}.fasta'), binary=os.path.join(base, 'muscle.exe'),
                                   match_award=10, similar_mismatch_penalty=0, mismatch_penalty=-5, gap_penalty=-10)
    
    remove_files(temp_path)

    column_names = ['ref_name', 'ref', 'tar_name', 'tar', 'symbol', 'variants', 'identity', 'similarity', 'score']
    df = pd.DataFrame(multi_HGVSish_variants, columns=column_names)
    df.to_csv(output, mode='a', index=False, header=not os.path.exists(output), sep=';')
    return df

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('-r', "--reference", help="The reference sequence (the query's sequence)", required=True)
    parser.add_argument('-q', "--reference_name", default="Reference", help="The name of the reference sequence (the name of the query's sequence)", required=False)
    parser.add_argument('-t', "--target", help="The target sequence (the gene's sequence)", required=True)
    parser.add_argument('-g', "--target_name", default="Target", help="PAM orientation", required=False)
    parser.add_argument('-o', "--output", default=os.path.join(base, 'variants.csv'), help="Output file", required=False)
    parser.add_argument('-m', "--temp", default=os.path.join(base, 'Temp'), help="Temp folder path", required=False)
    args = parser.parse_args()
    name1 = args.reference_name
    seq1 = args.reference
    name2 = args.target_name
    seq2 = args.target
    output_path = args.output
    temp_path = args.temp
    align_str_sequences(name1, seq1, name2, seq2, output_path, temp_path)

if __name__ == "__main__":
    main()
