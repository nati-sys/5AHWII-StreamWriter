namespace StreamWriter.Web
{
    public interface IDailyChallengeTextGenerator
    {
        Task<string> GetText();
    }
}
