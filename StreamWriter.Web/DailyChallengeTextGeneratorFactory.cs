namespace StreamWriter.Web
{
    public static class DailyChallengeTextGeneratorFactory
    {
        public static IDailyChallengeTextGenerator Create(
            DailyChallengeTextGeneratorConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return configuration.Type.Trim().ToLowerInvariant() switch
            {
                "static" => new StaticDailyChallengeTextGenerator(
                    configuration.Text ?? "This is a static typing text from StaticDailyChallengeTextGenerator."),
                "rotating" => CreateRotating(configuration),
                "delay" => new DelayedDailyChallengeTextGenerator(
                    TimeSpan.FromSeconds(ValidateDelay(configuration.DelaySeconds)),
                    CreateInner(configuration)),
                "randomspecialcharacters" => new DailyChallengeWithRandomSpecialCharactersTextGenerator(
                    CreateInner(configuration),
                    ValidateRatio(configuration.SpecialCharacterRatio)),
                _ => throw new InvalidOperationException(
                    $"Unknown daily challenge text generator type '{configuration.Type}'.")
            };
        }

        private static IDailyChallengeTextGenerator CreateRotating(
            DailyChallengeTextGeneratorConfiguration configuration)
        {
            if (configuration.Texts.Length == 0)
            {
                throw new InvalidOperationException("A rotating generator requires at least one text.");
            }

            return new RotatingDailyChallengeTextGenerator(configuration.Texts);
        }

        private static IDailyChallengeTextGenerator CreateInner(
            DailyChallengeTextGeneratorConfiguration configuration)
        {
            if (configuration.Inner is null)
            {
                throw new InvalidOperationException(
                    $"The '{configuration.Type}' generator requires an Inner generator configuration.");
            }

            return Create(configuration.Inner);
        }

        private static int ValidateDelay(int delaySeconds)
        {
            if (delaySeconds < 0)
            {
                throw new InvalidOperationException("DelaySeconds must be non-negative.");
            }

            return delaySeconds;
        }

        private static double ValidateRatio(double ratio)
        {
            if (double.IsNaN(ratio) || double.IsInfinity(ratio) || ratio < 0)
            {
                throw new InvalidOperationException("SpecialCharacterRatio must be a finite, non-negative value.");
            }

            return ratio;
        }
    }
}