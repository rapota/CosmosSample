using AutoFixture;

namespace CosmosSample;

public class TranslationRuleFactory
{
    private readonly Random _random;
    private readonly Fixture _fixture;
    private readonly List<string> _sourceSystems;
    private readonly List<string> _targetSystems;

    public TranslationRuleFactory(int sourceSystemCount, int targetSystemsCount)
    {
        _random = new Random();
        _fixture = new Fixture();

        _sourceSystems = Enumerable.Range(1, sourceSystemCount)
            .Select(x => "SourceSystem-" + x)
            .ToList();

        _targetSystems = Enumerable.Range(1, targetSystemsCount)
            .Select(x => "TargetSystem-" + x)
            .ToList();
    }

    public TranslationRule Create()
    {
        string sourceSystem = _sourceSystems[_random.Next(_sourceSystems.Count - 1)];
        string targetSystem = _targetSystems[_random.Next(_targetSystems.Count - 1)];

        return _fixture.Build<TranslationRule>()
            .With(x => x.SourceSystem, sourceSystem)
            .With(x => x.TargetSystem, targetSystem)
            .Create();
    }
}