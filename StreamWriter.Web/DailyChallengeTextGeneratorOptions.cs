namespace StreamWriter.Web
{
    public class DailyChallengeTextGeneratorOptions
    {
        public const string SectionName = "DailyChallengeTextGenerator";

        public DailyChallengeTextGeneratorConfiguration Generator { get; set; } = new();
    }

    public class DailyChallengeTextGeneratorConfiguration
    {
        public string Type { get; set; } = "Static";
        public string? Text { get; set; }
        public string[] Texts { get; set; } = [];
        public int DelaySeconds { get; set; }
        public double SpecialCharacterRatio { get; set; }
        public DailyChallengeTextGeneratorConfiguration? Inner { get; set; }
    }
}