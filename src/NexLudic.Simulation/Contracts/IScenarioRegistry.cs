namespace NexLudic.Simulation.Contracts;

/// <summary>
/// Registry for discovering and managing scenarios.
/// </summary>
public interface IScenarioRegistry
{
    void Register(IScenario scenario);
    bool TryGet(string scenarioId, out IScenario? scenario);
    IReadOnlyList<IScenario> GetAll();
    IReadOnlyList<IScenario> GetByLayer(string layer);
}
