namespace StreamWriter.Web
{
    public class DailyChallengeGeneratorOptions
    {
        public string Type { get; set; } = "static";
        public string Text { get; set; } = string.Empty;
        public int DelaySeconds { get; set; }
        public double SpecialCharacterRatio { get; set; }
        public string[] Texts { get; set; } = [];
        public DailyChallengeGeneratorOptions? Inner { get; set; }
    }
}
