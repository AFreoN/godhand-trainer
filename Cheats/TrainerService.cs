using GodHandTrainer.Core;

namespace GodHandTrainer.Cheats;

public sealed class TrainerService : IDisposable
{
    public MemoryManager Memory { get; }
    public AOBScanner Scanner { get; }
    public InjectionManager Injection { get; }
    public FreezeManager Freeze { get; }

    public PlayerCheats Player { get; }
    public UnlockCheats Unlocks { get; }
    public GameplayCheats Gameplay { get; }
    public CombatCheats Combat { get; }
    public CombatHooksCheats CombatHooks { get; }
    public MoveDamageCheats MoveDamage { get; }

    private readonly System.Threading.Timer _watchdog;
    private bool _disposed;

    public event EventHandler? AttachStateChanged;

    public TrainerService()
    {
        Memory = MemoryManager.Instance;
        Scanner = new AOBScanner(Memory);
        Injection = new InjectionManager(Memory);
        Freeze = new FreezeManager(Memory);
        Player = new PlayerCheats(Memory);
        Unlocks = new UnlockCheats(Memory);
        Gameplay = new GameplayCheats(Memory, Scanner, Injection);
        Combat = new CombatCheats(Memory);
        CombatHooks = new CombatHooksCheats(Memory, Scanner, Injection);
        MoveDamage = new MoveDamageCheats(Memory);

        Memory.AttachStateChanged += (s, e) => AttachStateChanged?.Invoke(this, EventArgs.Empty);

        _watchdog = new System.Threading.Timer(_ => CheckProcess(), null, 1000, 2000);
    }

    public Task<bool> AttachAsync() => Task.Run(() => Memory.TryAttach());

    public void Detach()
    {
        Injection.UninstallAll();
        Memory.Detach();
    }

    private void CheckProcess()
    {
        try
        {
            if (Memory.IsAttached) return;
            if (Memory.Process is not null)
            {
                Injection.UninstallAll();
                Freeze.ClearAll();
                Memory.Detach();
                return;
            }
            Memory.TryAttach();
        }
        catch { /* swallow watchdog errors */ }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _watchdog.Dispose();
        Freeze.Dispose();
        Detach();
    }
}
