using Microsoft.Extensions.Configuration;

namespace StreamWriter.Web
{
    public static class DailyChallengeGeneratorFactory
    {
        public static IDailyChallengeTextGenerator Create(IConfiguration configuration)
        {
            var options = configuration
                .GetSection("DailyChallenge")
                .Get<DailyChallengeGeneratorOptions>()
                ?? throw new InvalidOperationException("The DailyChallenge configuration is missing.");

            return Create(options);
        }

        private static IDailyChallengeTextGenerator Create(DailyChallengeGeneratorOptions options)
        {
            IDailyChallengeTextGenerator generator = options.Type.ToLowerInvariant() switch
            {
                "static" => new StaticDailyChallengeTextGenerator(options.Text),
                "rotating" => new RotatingDailyChallengeTextGenerator(options.Texts),
                "delay" => new DelayedDailyChallengeTextGenerator(
                    TimeSpan.FromSeconds(options.DelaySeconds),
                    CreateInner(options)),
                "random" => new DailyChallengeWithRandomSpecialCharactersTextGenerator(
                    CreateInner(options),
                    options.SpecialCharacterRatio),
                _ => throw new InvalidOperationException(
                    $"Unknown daily challenge generator type '{options.Type}'.")
            };

            return generator;
        }

        private static IDailyChallengeTextGenerator CreateInner(DailyChallengeGeneratorOptions options)
        {
            if (options.Inner is null)
            {
                throw new InvalidOperationException(
                    $"Generator type '{options.Type}' requires an Inner generator.");
            }

            return Create(options.Inner);
        }
    }
}
