using System.Collections.Concurrent;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;

namespace NexLudic.Simulation.Contracts;

/// <summary>
/// Execution context for an individual scenario, providing state isolation, virtual time,
/// diagnostics, reproducible randomness, and tracked disposable cleanup.
/// </summary>
public sealed class ScenarioContext : IAsyncDisposable
{
    private readonly ConcurrentBag<IDisposable> _disposables = new();
    private readonly ConcurrentBag<IAsyncDisposable> _asyncDisposables = new();
    private readonly ConcurrentDictionary<string, object> _state = new();

    public string ScenarioId { get; }
    public string RunId { get; }
    public int Seed { get; }
    public IDeterministicClock Clock { get; }
    public IDiagnosticsCollector Diagnostics { get; }
    public string WorkingDirectory { get; }
    public bool CleanupAttempted { get; private set; }
    public bool CleanupSucceeded { get; private set; }
    public Exception? CleanupError { get; private set; }

    public ScenarioContext(
        string scenarioId,
        string runId,
        int seed,
        IDeterministicClock clock,
        IDiagnosticsCollector diagnostics,
        string workingDirectory)
    {
        ScenarioId = scenarioId ?? throw new ArgumentNullException(nameof(scenarioId));
        RunId = runId ?? throw new ArgumentNullException(nameof(runId));
        Seed = seed;
        Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
    }

    public void SetState<T>(string key, T value) where T : notnull
    {
        _state[key] = value;
    }

    public bool TryGetState<T>(string key, out T? value)
    {
        if (_state.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    public T GetRequiredState<T>(string key)
    {
        if (TryGetState<T>(key, out var val) && val != null)
            return val;
        throw new KeyNotFoundException($"Required state key '{key}' of type {typeof(T).Name} not found in scenario context.");
    }

    public void TrackDisposable(IDisposable disposable)
    {
        if (disposable != null)
            _disposables.Add(disposable);
    }

    public void TrackAsyncDisposable(IAsyncDisposable asyncDisposable)
    {
        if (asyncDisposable != null)
            _asyncDisposables.Add(asyncDisposable);
    }

    public async Task CleanupAsync()
    {
        CleanupAttempted = true;
        List<Exception> errors = new();

        foreach (var asyncDisp in _asyncDisposables)
        {
            try
            {
                await asyncDisp.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
                Diagnostics.LogError("ContextCleanup", $"Async disposable cleanup error: {ex.Message}", ex);
            }
        }

        foreach (var disp in _disposables)
        {
            try
            {
                disp.Dispose();
            }
            catch (Exception ex)
            {
                errors.Add(ex);
                Diagnostics.LogError("ContextCleanup", $"Disposable cleanup error: {ex.Message}", ex);
            }
        }

        // Clean temporary working directory if it exists and is under temp
        if (Directory.Exists(WorkingDirectory))
        {
            try
            {
                // Only clean if it is a designated scenario scratch dir
                if (WorkingDirectory.Contains("NexLudicSim_"))
                {
                    Directory.Delete(WorkingDirectory, recursive: true);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex);
                Diagnostics.LogError("ContextCleanup", $"Directory cleanup error for {WorkingDirectory}: {ex.Message}", ex);
            }
        }

        if (errors.Count > 0)
        {
            CleanupSucceeded = false;
            CleanupError = new AggregateException("One or more errors occurred during scenario context cleanup.", errors);
        }
        else
        {
            CleanupSucceeded = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!CleanupAttempted)
        {
            await CleanupAsync().ConfigureAwait(false);
        }
    }
}
