using Newtonsoft.Json;

namespace CRISPR.Models;

public class Results
{
    [JsonProperty("ref_name")]
    public string ReferenceName { get; set; } = string.Empty;

    [JsonProperty("ref")]
    public string ReferenceSequence { get; set; } = string.Empty;

    [JsonProperty("tar_name")]
    public string TargetName { get; set; } = string.Empty;

    [JsonProperty("tar")]
    public string TargetSequence { get; set; } = string.Empty;

    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("identity")]
    public double Identity { get; set; }

    [JsonProperty("similarity")]
    public double Similarity { get; set; }

    [JsonProperty("score")]
    public int Score { get; set; }

    [JsonProperty("variants")]
    public List<List<object>> Variants { get; set; } = new();

    [JsonProperty("sgrnas")]
    public List<List<string>> SGRNAs { get; set; } = new();


    public Results() { }
    public Results(string ReferenceName, string ReferenceSequence, string TargetName, string TargetSequence,
                   string Symbol, double Identity, double Similarity, int Score, List<List<object>> Variants, List<List<string>> SGRNAs) 
    {
        this.ReferenceName = ReferenceName;
        this.ReferenceSequence = ReferenceSequence;
        this.TargetName = TargetName;
        this.TargetSequence = TargetSequence;
        this.Symbol = Symbol;
        this.Identity = Identity;
        this.Similarity = Similarity;
        this.Score = Score;
        this.Variants = Variants;
        this.SGRNAs = SGRNAs;
    }
}
