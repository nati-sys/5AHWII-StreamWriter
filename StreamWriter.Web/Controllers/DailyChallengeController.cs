using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StreamWriter.Web.Controllers
{
    [Route("daily-challenge")]
    [ApiController]
    public class DailyChallengeController : ControllerBase
    {
        private readonly IDailyChallengeTextGenerator _dailyChallengeTextGenerator;

        public DailyChallengeController(IDailyChallengeTextGenerator dailyChallengeTextGenerator)
        {
            _dailyChallengeTextGenerator = dailyChallengeTextGenerator;
        }


        [HttpGet("text")]
        public async Task<string> GetDailyChallenge()
        {
            // await Task.Delay(500);
            string text = await _dailyChallengeTextGenerator.GetText();
            return text;
        }
    }
}
