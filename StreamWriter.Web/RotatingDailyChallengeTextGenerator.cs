namespace StreamWriter.Web
{
    public class RotatingDailyChallengeTextGenerator : IDailyChallengeTextGenerator
    {
        private readonly string[] _texts;
        private int _textIndex;

        public RotatingDailyChallengeTextGenerator(string[] texts)
        {
            _texts = texts;
            _textIndex = 0;
        }
        private int _currentIndex = 0;
        public Task<string> GetText()
        {
            string result = _texts[_textIndex];
            _textIndex = (_textIndex + 1) % _texts.Length;

            //if (_textIndex == _texts.Length - 1)
            //{
            //    _textIndex = 0;
            //}
            //else
            //{
            //    _textIndex++;
            //}
            return Task.FromResult(result);
        }
    }
}
