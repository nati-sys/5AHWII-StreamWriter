namespace StreamWriter.Web
{
    public class DailyChallengeWithRandomSpecialCharactersTextGenerator : IDailyChallengeTextGenerator
    {
        private const string SpecialCharacters = "!@#$%^&*()-_=+[]{};:'\",.<>/?\\|`~";
        private readonly IDailyChallengeTextGenerator _inner;
        private readonly double _specialCharacterRatio;

        public DailyChallengeWithRandomSpecialCharactersTextGenerator(
            IDailyChallengeTextGenerator inner,
            double specialCharacterRatio)
        {
            ArgumentNullException.ThrowIfNull(inner);

            if (specialCharacterRatio < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(specialCharacterRatio));
            }

            _inner = inner;
            _specialCharacterRatio = specialCharacterRatio;
        }

        public async Task<string> GetText()
        {
            string text = await _inner.GetText();
            int specialCharacterCount = (int)Math.Round(
                text.Length * _specialCharacterRatio,
                MidpointRounding.AwayFromZero);

            var result = new System.Text.StringBuilder(text);

            for (int i = 0; i < specialCharacterCount; i++)
            {
                int insertionIndex = Random.Shared.Next(result.Length + 1);
                char specialCharacter = SpecialCharacters[Random.Shared.Next(SpecialCharacters.Length)];
                result.Insert(insertionIndex, specialCharacter);
            }

            return result.ToString();
        }
    }
}