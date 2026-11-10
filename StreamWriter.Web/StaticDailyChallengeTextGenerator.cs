namespace StreamWriter.Web
{
    public class StaticDailyChallengeTextGenerator : IDailyChallengeTextGenerator
    {
        private readonly string _text;

        public StaticDailyChallengeTextGenerator(
            string text = "This is a static typing text from StaticDailyChallengeTextGenerator.")
        {
            _text = text;
        }

        public Task<string> GetText()
        {
            return Task.FromResult(_text);
        }
    }
}
