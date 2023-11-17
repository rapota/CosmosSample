using CosmosSample;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosmosSampleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly TranslationContext _context;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, TranslationContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<WeatherForecast[]> Get()
        {
            await _context.Database.EnsureCreatedAsync();

            TranslationRuleFactory ruleFactory = new TranslationRuleFactory(10, 10);
            TranslationRule translationRule = ruleFactory.Create();

            await _context.TranslationRules.AddAsync(translationRule);
            await _context.SaveChangesAsync();

            List<TranslationRule> rules = await _context.TranslationRules.ToListAsync();








            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
