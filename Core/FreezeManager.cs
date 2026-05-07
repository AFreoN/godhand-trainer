using System.Collections.Concurrent;

namespace GodHandTrainer.Core;

public sealed class FreezeManager : IDisposable
{
    private readonly MemoryManager _memory;
    private readonly ConcurrentDictionary<string, Action> _frozen = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    public FreezeManager(MemoryManager memory)
    {
        _memory = memory;
        _loop = Task.Run(LoopAsync);
    }

    public void Set(string id, Action writeAction) => _frozen[id] = writeAction;

    public void Clear(string id) => _frozen.TryRemove(id, out _);

    public bool IsFrozen(string id) => _frozen.ContainsKey(id);

    public void ClearAll() => _frozen.Clear();

    private async Task LoopAsync()
    {
        var token = _cts.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_memory.IsAttached)
                {
                    foreach (var kv in _frozen)
                    {
                        try { kv.Value(); }
                        catch { /* ignore single-write failures */ }
                    }
                }
                await Task.Delay(100, token);
            }
            catch (TaskCanceledException) { break; }
            catch { /* keep looping */ }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _loop.Wait(500); } catch { }
        _cts.Dispose();
    }
}
