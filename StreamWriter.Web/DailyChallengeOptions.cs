namespace StreamWriter.Web
{
    public class DailyChallengeOptions
    {
        public int DelaySeconds { get; set; } = 5;
        public double SpecialCharacterRatio { get; set; } = 0.1;
        public string[] Texts { get; set; } = [];
    }
}
