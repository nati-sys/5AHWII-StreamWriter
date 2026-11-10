namespace StreamWriter.Web
{
    public class DelayedDailyChallengeTextGenerator : IDailyChallengeTextGenerator
    {
        private readonly IDailyChallengeTextGenerator _innerGenerator;
        private readonly TimeSpan _delayMilliseconds;
        public DelayedDailyChallengeTextGenerator(TimeSpan delayMilliseconds, IDailyChallengeTextGenerator innerGenerator)
        {
            _innerGenerator = innerGenerator;
            _delayMilliseconds = delayMilliseconds;
        }
        public async Task<string> GetText()
        {
            await Task.Delay(_delayMilliseconds);
            return await _innerGenerator.GetText();
        }
    }
}
