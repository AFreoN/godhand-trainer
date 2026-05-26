using GodHandTrainer.Cheats;
using GodHandTrainer.Data;

namespace GodHandTrainer.UI;

public sealed class MainForm : Form
{
    private readonly TrainerService _trainer;

    private Label _statusLabel = null!;
    private Panel _statusDot = null!;

    private NumericUpDown _goldInput = null!;
    private Button _goldApply = null!;

    private CheckBox _godModeToggle = null!;

    private NumericUpDown _godHandMeterInput = null!;
    private Button _godHandMeterApply = null!;
    private CheckBox _godHandMeterFreeze = null!;
    private NumericUpDown _levelMeterInput = null!;
    private Button _levelMeterApply = null!;
    private CheckBox _levelMeterFreeze = null!;

    private TrackBar _healthMaxSlider = null!;
    private Label _healthMaxValueLabel = null!;
    private CheckBox _healthMaxFreeze = null!;
    private TrackBar _heatGaugeMaxSlider = null!;
    private Label _heatGaugeMaxValueLabel = null!;
    private CheckBox _heatGaugeMaxFreeze = null!;

    private CheckBox _unlimitedKeysToggle = null!;

    private Button _movesUnlockButton = null!;
    private Button _rouletteUnlockButton = null!;
    private ComboBox _costumeCombo = null!;
    private NumericUpDown _rouletteSlotsInput = null!;
    private Button _rouletteSlotsApply = null!;
    private CheckBox _costumeFreeze = null!;
    private CheckBox _rouletteSlotsFreeze = null!;
    private NumericUpDown _rouletteAvailableInput = null!;
    private Button _rouletteAvailableApply = null!;
    private CheckBox _rouletteAvailableFreeze = null!;
    private CancellationTokenSource? _unlocksSyncCts;

    private CheckBox _timeScaleEnable = null!;
    private TrackBar _timeScaleSlider = null!;
    private Label _timeScaleValueLabel = null!;

    private readonly List<ComboBox> _combatCombos = new();
    private readonly List<CheckBox> _combatFreezes = new();
    private readonly List<CombatSlot> _combatSlots = new();
    private readonly List<Button> _combatPresetButtons = new();
    private CancellationTokenSource? _combatSyncCts;

    private CheckBox _hitboxToggle = null!;
    private CheckBox _damageTypeToggle = null!;
    private ComboBox _moveEffectCombo = null!;
    private CheckBox _noLagToggle = null!;
    private CheckBox _oneHitKillToggle = null!;
    private CheckBox _guardBreakerToggle = null!;
    private CheckBox _noDamageToggle = null!;
    private CheckBox _walkThroughWallsToggle = null!;

    private CollapsibleSection _moveDamageSection = null!;
    private readonly Dictionary<uint, NumericUpDown> _damageInputs = new();
    private readonly List<Button> _damageApplyButtons = new();

    private bool _suppressEvents;

    public MainForm(TrainerService trainer)
    {
        _trainer = trainer;
        BuildUi();

        _trainer.AttachStateChanged += (_, _) => BeginInvoke(new Action(RefreshAttachState));
        FormClosed += (_, _) =>
        {
            StopCombatSyncLoop();
            StopUnlocksSyncLoop();
            _trainer.Dispose();
        };

        RefreshAttachState();
    }

    private void BuildUi()
    {
        Text = "God Hand Trainer";
        ClientSize = new Size(1100, 1020);
        AutoScroll = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
        BackColor = Color.FromArgb(28, 28, 32);
        ForeColor = Color.Gainsboro;

        const int leftX = 12;
        const int rightX = 558;
        const int colW = 530;
        const int topY = 76;

        var status = BuildStatusPanel();
        status.Location = new Point(leftX, 12);
        status.Size = new Size(1076, 56);
        Controls.Add(status);

        var player = BuildPlayerGroup();
        player.Location = new Point(leftX, topY);
        player.Size = new Size(colW, 360);
        Controls.Add(player);

        var unlocks = BuildUnlocksGroup();
        unlocks.Location = new Point(leftX, topY + 368);
        unlocks.Size = new Size(colW, 200);
        Controls.Add(unlocks);

        var gameplay = BuildGameplayGroup();
        gameplay.Location = new Point(leftX, topY + 576);
        gameplay.Size = new Size(colW, 128);
        Controls.Add(gameplay);

        var hooks = BuildCombatHooksGroup();
        hooks.Location = new Point(rightX, topY);
        hooks.Size = new Size(colW, 170);
        Controls.Add(hooks);

        var combat = BuildCombatGroup();
        combat.Location = new Point(rightX, topY + 178);
        combat.Size = new Size(colW, 405);
        Controls.Add(combat);

        var movesDamageY = topY + 704 + 10;
        _moveDamageSection = BuildMoveDamageSection();
        _moveDamageSection.Location = new Point(leftX, movesDamageY);
        _moveDamageSection.Width = 1076;
        _moveDamageSection.ExpandedContentHeight = 330;
        Controls.Add(_moveDamageSection);

        var footer = new Label
        {
            Text = "PCSX2 must be running. Run trainer as administrator.",
            ForeColor = Color.DimGray,
            AutoSize = true,
            Location = new Point(leftX + 2, 0),
            Tag = "footer",
        };
        Controls.Add(footer);

        _moveDamageSection.StateChanged += (_, _) => RelayoutAfterMoveDamage();
        RelayoutAfterMoveDamage();
    }

    private Panel BuildStatusPanel()
    {
        var panel = new Panel { BackColor = Color.FromArgb(38, 38, 44) };

        _statusDot = new Panel
        {
            Size = new Size(14, 14),
            Location = new Point(14, 21),
            BackColor = Color.IndianRed,
        };
        panel.Controls.Add(_statusDot);

        _statusLabel = new Label
        {
            AutoSize = true,
            Location = new Point(36, 19),
            ForeColor = Color.Gainsboro,
            Text = "Searching for pcsx2.exe…",
        };
        panel.Controls.Add(_statusLabel);

        return panel;
    }

    private GroupBox BuildPlayerGroup()
    {
        var box = NewGroup("Player");

        var goldLabel = new Label
        {
            Text = "Gold",
            AutoSize = true,
            Location = new Point(14, 28),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(goldLabel);

        _goldInput = new DarkNumericUpDown
        {
            Location = new Point(60, 24),
            Size = new Size(120, 30),
            Minimum = 0,
            Maximum = 999_999,
            Value = 99_999,
        };
        box.Controls.Add(_goldInput);

        _goldApply = new DarkButton
        {
            Text = "Apply",
            Location = new Point(190, 24),
            Size = new Size(80, 30),
            Style = DarkButtonStyle.Primary,
        };
        _goldApply.Click += async (_, _) => await ApplyGoldAsync();
        box.Controls.Add(_goldApply);

        _godModeToggle = new DarkCheckBox
        {
            Text = "God Mode",
            Location = new Point(14, 60),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _godModeToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            var on = _godModeToggle.Checked;
            await Task.Run(() => _trainer.Player.SetGodMode(on));
            if (on) _trainer.Freeze.Set("GodMode", () => _trainer.Player.SetGodMode(true));
            else _trainer.Freeze.Clear("GodMode");
        };
        box.Controls.Add(_godModeToggle);

        var ghmLabel = new Label
        {
            Text = "God Hand Meter",
            AutoSize = true,
            Location = new Point(14, 96),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(ghmLabel);

        _godHandMeterInput = new DarkNumericUpDown
        {
            Location = new Point(110, 90),
            Size = new Size(80, 30),
            Minimum = Offsets.GodHandMeterMin,
            Maximum = Offsets.GodHandMeterMax,
            Value = 0,
        };
        box.Controls.Add(_godHandMeterInput);

        _godHandMeterApply = new DarkButton
        {
            Text = "Apply",
            Location = new Point(196, 90),
            Size = new Size(60, 30),
            Style = DarkButtonStyle.Primary,
        };
        _godHandMeterApply.Click += async (_, _) =>
        {
            var v = (int)_godHandMeterInput.Value;
            await Task.Run(() => _trainer.Player.SetGodHandMeter(v));
            if (_godHandMeterFreeze.Checked) RegisterGodHandMeterFreeze();
        };
        box.Controls.Add(_godHandMeterApply);

        _godHandMeterFreeze = new DarkCheckBox
        {
            Text = "Freeze",
            Location = new Point(266, 94),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _godHandMeterFreeze.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_godHandMeterFreeze.Checked) RegisterGodHandMeterFreeze();
            else _trainer.Freeze.Clear("GodHandMeter");
        };
        box.Controls.Add(_godHandMeterFreeze);

        _timeScaleEnable = new DarkCheckBox
        {
            Text = "Player Speed",
            Location = new Point(14, 128),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _timeScaleEnable.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleTimeScaleAsync(_timeScaleEnable.Checked);
        };
        box.Controls.Add(_timeScaleEnable);

        _timeScaleSlider = new TrackBar
        {
            Location = new Point(60, 155),
            Size = new Size(350, 45),
            Minimum = 5,
            Maximum = 100,
            Value = 10,
            TickFrequency = 5,
            SmallChange = 1,
            LargeChange = 5,
        };
        _timeScaleSlider.ValueChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            var v = _timeScaleSlider.Value / 10f;
            UpdateTimeScale(v, sourceIsSlider: true);
        };
        box.Controls.Add(_timeScaleSlider);

        _timeScaleValueLabel = new Label
        {
            Text = "1.0×",
            AutoSize = true,
            Location = new Point(420, 162),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(_timeScaleValueLabel);

        var healthMaxLabel = new Label
        {
            Text = "Health (Max)",
            AutoSize = true,
            Location = new Point(14, 215),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(healthMaxLabel);

        _healthMaxSlider = new TrackBar
        {
            Location = new Point(14, 235),
            Size = new Size(396, 45),
            Minimum = Offsets.HealthMaxMin,
            Maximum = Offsets.HealthMaxMax,
            Value = Offsets.HealthMaxMin,
            TickFrequency = 1,
            SmallChange = 1,
            LargeChange = 1,
        };
        _healthMaxSlider.ValueChanged += async (_, _) =>
        {
            _healthMaxValueLabel.Text = _healthMaxSlider.Value.ToString();
            if (_suppressEvents) return;
            var v = _healthMaxSlider.Value;
            await Task.Run(() => _trainer.Player.SetHealthMax(v));
            if (_healthMaxFreeze.Checked) RegisterHealthMaxFreeze();
        };
        box.Controls.Add(_healthMaxSlider);

        _healthMaxValueLabel = new Label
        {
            Text = Offsets.HealthMaxMin.ToString(),
            AutoSize = true,
            Location = new Point(420, 242),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(_healthMaxValueLabel);

        _healthMaxFreeze = new DarkCheckBox
        {
            Text = "Freeze",
            Location = new Point(450, 240),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _healthMaxFreeze.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_healthMaxFreeze.Checked) RegisterHealthMaxFreeze();
            else _trainer.Freeze.Clear("HealthMax");
        };
        box.Controls.Add(_healthMaxFreeze);

        var heatGaugeMaxLabel = new Label
        {
            Text = "Heat Gauge (Max)",
            AutoSize = true,
            Location = new Point(14, 280),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(heatGaugeMaxLabel);

        _heatGaugeMaxSlider = new TrackBar
        {
            Location = new Point(14, 300),
            Size = new Size(396, 45),
            Minimum = Offsets.HeatGaugeMaxMin,
            Maximum = Offsets.HeatGaugeMaxMax,
            Value = Offsets.HeatGaugeMaxMin,
            TickFrequency = 1,
            SmallChange = 1,
            LargeChange = 1,
        };
        _heatGaugeMaxSlider.ValueChanged += async (_, _) =>
        {
            _heatGaugeMaxValueLabel.Text = _heatGaugeMaxSlider.Value.ToString();
            if (_suppressEvents) return;
            var v = _heatGaugeMaxSlider.Value;
            await Task.Run(() => _trainer.Player.SetHeatGaugeMax(v));
            if (_heatGaugeMaxFreeze.Checked) RegisterHeatGaugeMaxFreeze();
        };
        box.Controls.Add(_heatGaugeMaxSlider);

        _heatGaugeMaxValueLabel = new Label
        {
            Text = Offsets.HeatGaugeMaxMin.ToString(),
            AutoSize = true,
            Location = new Point(420, 307),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(_heatGaugeMaxValueLabel);

        _heatGaugeMaxFreeze = new DarkCheckBox
        {
            Text = "Freeze",
            Location = new Point(450, 305),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _heatGaugeMaxFreeze.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_heatGaugeMaxFreeze.Checked) RegisterHeatGaugeMaxFreeze();
            else _trainer.Freeze.Clear("HeatGaugeMax");
        };
        box.Controls.Add(_heatGaugeMaxFreeze);

        return box;
    }

    private void RegisterHealthMaxFreeze()
    {
        var v = _healthMaxSlider.Value;
        _trainer.Freeze.Set("HealthMax", () => _trainer.Player.SetHealthMax(v));
    }

    private void RegisterHeatGaugeMaxFreeze()
    {
        var v = _heatGaugeMaxSlider.Value;
        _trainer.Freeze.Set("HeatGaugeMax", () => _trainer.Player.SetHeatGaugeMax(v));
    }

    private void RegisterGodHandMeterFreeze()
    {
        var v = (int)_godHandMeterInput.Value;
        _trainer.Freeze.Set("GodHandMeter", () => _trainer.Player.SetGodHandMeter(v));
    }

    private void RegisterLevelMeterFreeze()
    {
        var v = (int)_levelMeterInput.Value;
        _trainer.Freeze.Set("LevelMeter", () => _trainer.Player.SetLevelMeter(v));
    }

    private GroupBox BuildUnlocksGroup()
    {
        var box = NewGroup("Unlocks");

        _movesUnlockButton = new DarkButton
        {
            Text = "Unlock All Moves",
            Location = new Point(14, 30),
            Size = new Size(220, 34),
            Style = DarkButtonStyle.Success,
        };
        _movesUnlockButton.Click += async (_, _) => await Task.Run(() => _trainer.Unlocks.UnlockAllMoves());
        box.Controls.Add(_movesUnlockButton);

        _rouletteUnlockButton = new DarkButton
        {
            Text = "Unlock All Roulettes",
            Location = new Point(244, 30),
            Size = new Size(220, 34),
            Style = DarkButtonStyle.Warning,
        };
        _rouletteUnlockButton.Click += async (_, _) => await Task.Run(() => _trainer.Unlocks.UnlockAllRoulettes());
        box.Controls.Add(_rouletteUnlockButton);

        var costumeLabel = new Label
        {
            Text = "Costume",
            AutoSize = true,
            Location = new Point(14, 80),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(costumeLabel);

        _costumeCombo = new DarkComboBox
        {
            Location = new Point(140, 74),
            Size = new Size(180, 30),
        };
        _costumeCombo.Items.AddRange(new object[]
        {
            "Original",
            "Original Double",
            "Devil",
            "Devil Double",
            "Karate",
            "Karate Double",
            "Carnival",
            "Carnival Double",
        });
        _costumeCombo.SelectedIndex = 0;
        _costumeCombo.SelectedIndexChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            var costume = (Costume)_costumeCombo.SelectedIndex;
            await Task.Run(() => _trainer.Unlocks.SetCostume(costume));
            if (_costumeFreeze.Checked) RegisterCostumeFreeze();
        };
        box.Controls.Add(_costumeCombo);

        _costumeFreeze = new DarkCheckBox
        {
            Text = "Freeze",
            Location = new Point(330, 78),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _costumeFreeze.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_costumeFreeze.Checked) RegisterCostumeFreeze();
            else _trainer.Freeze.Clear("Costume");
        };
        box.Controls.Add(_costumeFreeze);

        var slotsLabel = new Label
        {
            Text = "Roulette Slots",
            AutoSize = true,
            Location = new Point(14, 122),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(slotsLabel);

        _rouletteSlotsInput = new DarkNumericUpDown
        {
            Location = new Point(140, 116),
            Size = new Size(60, 30),
            Minimum = Offsets.RouletteSlotsMin,
            Maximum = Offsets.RouletteSlotsMax,
            Value = 6,
        };
        box.Controls.Add(_rouletteSlotsInput);

        _rouletteSlotsApply = new DarkButton
        {
            Text = "Apply",
            Location = new Point(210, 116),
            Size = new Size(80, 30),
            Style = DarkButtonStyle.Primary,
        };
        _rouletteSlotsApply.Click += async (_, _) =>
        {
            var v = (byte)_rouletteSlotsInput.Value;
            await Task.Run(() => _trainer.Unlocks.SetRouletteSlots(v));
            if (_rouletteSlotsFreeze.Checked) RegisterRouletteSlotsFreeze();
        };
        box.Controls.Add(_rouletteSlotsApply);

        _rouletteSlotsFreeze = new DarkCheckBox
        {
            Text = "Freeze",
            Location = new Point(300, 120),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _rouletteSlotsFreeze.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_rouletteSlotsFreeze.Checked) RegisterRouletteSlotsFreeze();
            else _trainer.Freeze.Clear("RouletteSlots");
        };
        box.Controls.Add(_rouletteSlotsFreeze);

        var availableLabel = new Label
        {
            Text = "Roulette Available",
            AutoSize = true,
            Location = new Point(14, 164),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(availableLabel);

        _rouletteAvailableInput = new DarkNumericUpDown
        {
            Location = new Point(140, 158),
            Size = new Size(60, 30),
            Minimum = 0,
            Maximum = Offsets.RouletteAvailableMax,
            Value = 0,
        };
        box.Controls.Add(_rouletteAvailableInput);

        _rouletteAvailableApply = new DarkButton
        {
            Text = "Apply",
            Location = new Point(210, 158),
            Size = new Size(80, 30),
            Style = DarkButtonStyle.Primary,
        };
        _rouletteAvailableApply.Click += async (_, _) =>
        {
            var v = (byte)_rouletteAvailableInput.Value;
            if (v < Offsets.RouletteAvailableMin) return;
            await Task.Run(() => _trainer.Unlocks.SetRouletteAvailable(v));
            if (_rouletteAvailableFreeze.Checked) RegisterRouletteAvailableFreeze();
        };
        box.Controls.Add(_rouletteAvailableApply);

        _rouletteAvailableFreeze = new DarkCheckBox
        {
            Text = "Freeze",
            Location = new Point(300, 162),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _rouletteAvailableFreeze.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_rouletteAvailableFreeze.Checked) RegisterRouletteAvailableFreeze();
            else _trainer.Freeze.Clear("RouletteAvailable");
        };
        box.Controls.Add(_rouletteAvailableFreeze);

        return box;
    }

    private void RegisterCostumeFreeze()
    {
        var costume = (Costume)_costumeCombo.SelectedIndex;
        _trainer.Freeze.Set("Costume", () => _trainer.Unlocks.SetCostume(costume));
    }

    private void RegisterRouletteSlotsFreeze()
    {
        var v = (byte)_rouletteSlotsInput.Value;
        _trainer.Freeze.Set("RouletteSlots", () => _trainer.Unlocks.SetRouletteSlots(v));
    }

    private void RegisterRouletteAvailableFreeze()
    {
        var v = (byte)_rouletteAvailableInput.Value;
        if (v < Offsets.RouletteAvailableMin) return;
        _trainer.Freeze.Set("RouletteAvailable", () => _trainer.Unlocks.SetRouletteAvailable(v));
    }

    private GroupBox BuildGameplayGroup()
    {
        var box = NewGroup("Gameplay");

        var lvlLabel = new Label
        {
            Text = "Level Meter",
            AutoSize = true,
            Location = new Point(14, 28),
            ForeColor = Color.Gainsboro,
        };
        box.Controls.Add(lvlLabel);

        _levelMeterInput = new DarkNumericUpDown
        {
            Location = new Point(110, 22),
            Size = new Size(80, 30),
            Minimum = Offsets.LevelMeterMin,
            Maximum = Offsets.LevelMeterMax,
            Value = 0,
        };
        box.Controls.Add(_levelMeterInput);

        _levelMeterApply = new DarkButton
        {
            Text = "Apply",
            Location = new Point(196, 22),
            Size = new Size(60, 30),
            Style = DarkButtonStyle.Primary,
        };
        _levelMeterApply.Click += async (_, _) =>
        {
            var v = (int)_levelMeterInput.Value;
            await Task.Run(() => _trainer.Player.SetLevelMeter(v));
            if (_levelMeterFreeze.Checked) RegisterLevelMeterFreeze();
        };
        box.Controls.Add(_levelMeterApply);

        _levelMeterFreeze = new DarkCheckBox
        {
            Text = "Freeze",
            Location = new Point(266, 26),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _levelMeterFreeze.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_levelMeterFreeze.Checked) RegisterLevelMeterFreeze();
            else _trainer.Freeze.Clear("LevelMeter");
        };
        box.Controls.Add(_levelMeterFreeze);

        _unlimitedKeysToggle = new DarkCheckBox
        {
            Text = "Unlimited Keys",
            Location = new Point(14, 60),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _unlimitedKeysToggle.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents) return;
            if (_unlimitedKeysToggle.Checked) RegisterUnlimitedKeysFreeze();
            else _trainer.Freeze.Clear("UnlimitedKeys");
        };
        box.Controls.Add(_unlimitedKeysToggle);

        _walkThroughWallsToggle = new DarkCheckBox
        {
            Text = "Walk Through Walls",
            Location = new Point(14, 92),
            AutoSize = true,
            ForeColor = Color.Gainsboro,
        };
        _walkThroughWallsToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleHookAsync(_walkThroughWallsToggle,
                () => _trainer.CombatHooks.InstallWalkThroughWalls(),
                () => { _trainer.CombatHooks.UninstallWalkThroughWalls(); return true; },
                "Walk Through Walls");
        };
        box.Controls.Add(_walkThroughWallsToggle);

        return box;
    }

    private void RegisterUnlimitedKeysFreeze()
    {
        _trainer.Freeze.Set("UnlimitedKeys", () =>
        {
            _trainer.Player.SetUnlimitedKeys();
        });
    }


    private GroupBox BuildCombatHooksGroup()
    {
        var box = NewGroup("Combat Hooks");

        _hitboxToggle = new DarkCheckBox
        {
            Text = "Hitbox Large",
            AutoSize = true,
            Location = new Point(14, 28),
            ForeColor = Color.Gainsboro,
        };
        _hitboxToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleHookAsync(_hitboxToggle,
                () => _trainer.CombatHooks.InstallHitBox(),
                () => { _trainer.CombatHooks.UninstallHitBox(); return true; },
                "Hitbox");
        };
        box.Controls.Add(_hitboxToggle);

        _noLagToggle = new DarkCheckBox
        {
            Text = "Quick Move Relief (no lag)",
            AutoSize = true,
            Location = new Point(260, 28),
            ForeColor = Color.Gainsboro,
        };
        _noLagToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleHookAsync(_noLagToggle,
                () => _trainer.CombatHooks.InstallNoLag(),
                () => { _trainer.CombatHooks.UninstallNoLag(); return true; },
                "Quick Move Relief");
        };
        box.Controls.Add(_noLagToggle);

        _oneHitKillToggle = new DarkCheckBox
        {
            Text = "One Hit Kill",
            AutoSize = true,
            Location = new Point(14, 64),
            ForeColor = Color.Gainsboro,
        };
        _oneHitKillToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleHookAsync(_oneHitKillToggle,
                () => _trainer.CombatHooks.InstallOneHitKill(),
                () => { _trainer.CombatHooks.UninstallOneHitKill(); return true; },
                "One Hit Kill");
        };
        box.Controls.Add(_oneHitKillToggle);

        _noDamageToggle = new DarkCheckBox
        {
            Text = "No Damage",
            AutoSize = true,
            Location = new Point(140, 64),
            ForeColor = Color.Gainsboro,
        };
        _noDamageToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleHookAsync(_noDamageToggle,
                () => _trainer.CombatHooks.InstallNoDamage(),
                () => { _trainer.CombatHooks.UninstallNoDamage(); return true; },
                "No Damage");
        };
        box.Controls.Add(_noDamageToggle);

        _guardBreakerToggle = new DarkCheckBox
        {
            Text = "Guard Breaker",
            AutoSize = true,
            Location = new Point(260, 64),
            ForeColor = Color.Gainsboro,
        };
        _guardBreakerToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleHookAsync(_guardBreakerToggle,
                () => _trainer.CombatHooks.InstallUnBlocker(),
                () => { _trainer.CombatHooks.UninstallUnBlocker(); return true; },
                "Guard Breaker");
        };
        box.Controls.Add(_guardBreakerToggle);

        _damageTypeToggle = new DarkCheckBox
        {
            Text = "Damage Type",
            AutoSize = true,
            Location = new Point(14, 100),
            ForeColor = Color.Gainsboro,
        };
        _damageTypeToggle.CheckedChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            await ToggleHookAsync(_damageTypeToggle,
                () =>
                {
                    if (!_trainer.CombatHooks.InstallDamageType()) return false;
                    if (_moveEffectCombo.SelectedItem is MoveEffectDefinition def)
                        _trainer.CombatHooks.SetMoveEffect(def.Id);
                    return true;
                },
                () => { _trainer.CombatHooks.UninstallDamageType(); return true; },
                "Damage Type");
        };
        box.Controls.Add(_damageTypeToggle);

        _moveEffectCombo = new DarkComboBox
        {
            Location = new Point(140, 110),
            Size = new Size(330, 30),
            DropDownHeight = 200,
        };
        _moveEffectCombo.Items.AddRange(MoveEffectCatalog.Effects.Cast<object>().ToArray());
        var defaultIdx = MoveEffectCatalog.Effects.ToList().FindIndex(e => e.Id == Patterns.MoveEffectDefault);
        _moveEffectCombo.SelectedIndex = defaultIdx >= 0 ? defaultIdx : 0;
        _moveEffectCombo.SelectedIndexChanged += async (_, _) =>
        {
            if (_suppressEvents) return;
            if (_moveEffectCombo.SelectedItem is not MoveEffectDefinition def) return;
            await Task.Run(() => _trainer.CombatHooks.SetMoveEffect(def.Id));
        };
        box.Controls.Add(_moveEffectCombo);

        return box;
    }

    private async Task ToggleHookAsync(CheckBox toggle, Func<bool> install, Func<bool> uninstall, string name)
    {
        toggle.Enabled = false;
        var pulsing = toggle as DarkCheckBox;
        pulsing?.BeginPulse();
        try
        {
            if (toggle.Checked)
            {
                var ok = await Task.Run(install);
                if (!ok)
                {
                    _suppressEvents = true;
                    toggle.Checked = false;
                    _suppressEvents = false;
                    MessageBox.Show(this,
                        $"Could not locate {name} pattern. Load the game and try again.",
                        name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                await Task.Run(uninstall);
            }
        }
        finally
        {
            pulsing?.EndPulse();
            toggle.Enabled = true;
        }
    }

    private GroupBox BuildCombatGroup()
    {
        var box = NewGroup("Combat — assign moves to slots");

        var moveItems = MoveCatalog.Moves.Cast<object>().ToArray();

        const int presetCount = 4;
        const int presetBtnWidth = 110;
        const int presetBtnGap = 8;
        var presetRowY = 28;
        for (var i = 0; i < presetCount; i++)
        {
            var index = i;
            var btn = new DarkButton
            {
                Text = $"Preset {index + 1}",
                Location = new Point(14 + i * (presetBtnWidth + presetBtnGap), presetRowY),
                Size = new Size(presetBtnWidth, 30),
                Style = DarkButtonStyle.Primary,
            };
            btn.Click += async (_, _) => await ApplyCombatPresetAsync(index);
            box.Controls.Add(btn);
            _combatPresetButtons.Add(btn);
        }

        var y = presetRowY + 38;
        foreach (var slot in MoveCatalog.Slots)
        {
            var label = new Label
            {
                Text = slot.Label,
                AutoSize = false,
                Size = new Size(120, 23),
                Location = new Point(14, y + 3),
                ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            box.Controls.Add(label);

            var combo = new DarkComboBox
            {
                Location = new Point(140, y),
                Size = new Size(300, 30),
                Tag = slot,
                IntegralHeight = false,
                DropDownHeight = 320,
            };
            combo.Items.AddRange(moveItems);
            combo.SelectedIndex = 0;

            var freeze = new DarkCheckBox
            {
                Text = "Freeze",
                Location = new Point(450, y + 1),
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Tag = slot,
            };
            var freezeId = $"CombatSlot:{slot.Address:X8}";
            combo.SelectedIndexChanged += async (s, _) =>
            {
                if (_suppressEvents) return;
                if (s is not ComboBox cb) return;
                if (cb.Tag is not CombatSlot cs) return;
                if (cb.SelectedItem is not MoveDefinition move) return;
                await Task.Run(() => _trainer.Combat.SetSlotMove(cs.Address, move.Id));
                if (freeze.Checked)
                    _trainer.Freeze.Set(freezeId, () => _trainer.Combat.SetSlotMove(cs.Address, move.Id));
            };
            freeze.CheckedChanged += (_, _) =>
            {
                if (_suppressEvents) return;
                if (freeze.Checked)
                {
                    if (combo.SelectedItem is MoveDefinition mv)
                        _trainer.Freeze.Set(freezeId, () => _trainer.Combat.SetSlotMove(slot.Address, mv.Id));
                }
                else
                {
                    _trainer.Freeze.Clear(freezeId);
                }
            };
            box.Controls.Add(combo);
            box.Controls.Add(freeze);
            _combatCombos.Add(combo);
            _combatFreezes.Add(freeze);
            _combatSlots.Add(slot);

            y += 30;
        }

        return box;
    }

    private static readonly IReadOnlyDictionary<uint, int> CombatPreset1 = new Dictionary<uint, int>
    {
        [Offsets.MoveTriangle]     = 99,
        [Offsets.MoveDownTriangle] = 77,
        [Offsets.MoveCross]        = 101,
        [Offsets.MoveDownCross]    = 57,
        [Offsets.MoveDownSquare]   = 98,
        [Offsets.MoveSquare1]      = 95,
        [Offsets.MoveSquare2]      = 94,
        [Offsets.MoveSquare3]      = 96,
        [Offsets.MoveSquare4]      = 97,
    };

    private static readonly IReadOnlyDictionary<uint, int> CombatPreset2 = new Dictionary<uint, int>
    {
        [Offsets.MoveTriangle]     = 99,
        [Offsets.MoveDownTriangle] = 77,
        [Offsets.MoveCross]        = 101,
        [Offsets.MoveDownCross]    = 57,
        [Offsets.MoveDownSquare]   = 98,
        [Offsets.MoveSquare1]      = 95,
        [Offsets.MoveSquare2]      = 94,
        [Offsets.MoveSquare3]      = 96,
        [Offsets.MoveSquare4]      = 97,
    };

    private static readonly IReadOnlyDictionary<uint, int> CombatPreset3 = new Dictionary<uint, int>
    {
        [Offsets.MoveTriangle]     = 99,
        [Offsets.MoveDownTriangle] = 77,
        [Offsets.MoveCross]        = 101,
        [Offsets.MoveDownCross]    = 57,
        [Offsets.MoveDownSquare]   = 98,
        [Offsets.MoveSquare1]      = 95,
        [Offsets.MoveSquare2]      = 94,
        [Offsets.MoveSquare3]      = 96,
        [Offsets.MoveSquare4]      = 97,
    };

    private static readonly IReadOnlyDictionary<uint, int> CombatPreset4 = new Dictionary<uint, int>
    {
        [Offsets.MoveTriangle]     = 99,
        [Offsets.MoveDownTriangle] = 77,
        [Offsets.MoveCross]        = 101,
        [Offsets.MoveDownCross]    = 57,
        [Offsets.MoveDownSquare]   = 98,
        [Offsets.MoveSquare1]      = 95,
        [Offsets.MoveSquare2]      = 94,
        [Offsets.MoveSquare3]      = 96,
        [Offsets.MoveSquare4]      = 97,
    };

    private static IReadOnlyDictionary<uint, int> GetCombatPreset(int index) => index switch
    {
        0 => CombatPreset1,
        1 => CombatPreset2,
        2 => CombatPreset3,
        3 => CombatPreset4,
        _ => CombatPreset1,
    };

    private async Task ApplyCombatPresetAsync(int index)
    {
        if (!_trainer.Memory.IsAttached) return;
        var preset = GetCombatPreset(index);
        var movesById = MoveCatalog.Moves.ToDictionary(m => m.Id);

        await Task.Run(() =>
        {
            foreach (var (addr, moveId) in preset)
                _trainer.Combat.SetSlotMove(addr, moveId);
        });

        _suppressEvents = true;
        try
        {
            for (var i = 0; i < _combatSlots.Count; i++)
            {
                if (!preset.TryGetValue(_combatSlots[i].Address, out var moveId)) continue;
                if (!movesById.TryGetValue(moveId, out var move)) continue;
                _combatCombos[i].SelectedItem = move;
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        for (var i = 0; i < _combatSlots.Count; i++)
        {
            if (!preset.TryGetValue(_combatSlots[i].Address, out var moveId)) continue;
            if (!_combatFreezes[i].Checked) continue;
            var addr = _combatSlots[i].Address;
            var freezeId = $"CombatSlot:{addr:X8}";
            _trainer.Freeze.Set(freezeId, () => _trainer.Combat.SetSlotMove(addr, moveId));
        }
    }

    private CollapsibleSection BuildMoveDamageSection()
    {
        var section = new CollapsibleSection
        {
            Title = "Moves Damage",
            Expanded = false,
        };

        var content = section.Content;
        content.Padding = new Padding(0, 0, 8, 0);
        content.HorizontalScroll.Enabled = false;
        content.HorizontalScroll.Visible = false;
        content.HorizontalScroll.Maximum = 0;
        const int columns = 3;
        const int colWidth = 330;
        const int rowHeight = 30;
        const int colGap = 8;

        var entries = MoveDamageCatalog.Entries;
        var rowsPerColumn = (entries.Count + columns - 1) / columns;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var col = i / rowsPerColumn;
            var row = i % rowsPerColumn;
            var x = 8 + col * (colWidth + colGap);
            var y = 6 + row * rowHeight;

            var idLabel = new Label
            {
                Text = $"{entry.MoveId:D3}",
                AutoSize = false,
                Size = new Size(34, 23),
                Location = new Point(x, y + 3),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            content.Controls.Add(idLabel);

            var nameLabel = new Label
            {
                Text = entry.Name,
                AutoSize = false,
                Size = new Size(150, 23),
                Location = new Point(x + 36, y + 3),
                ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            content.Controls.Add(nameLabel);

            var input = new DarkNumericUpDown
            {
                Location = new Point(x + 188, y - 3),
                Size = new Size(80, 30),
                Minimum = 0,
                Maximum = 999_999,
                Value = 100,
                Tag = entry,
            };
            content.Controls.Add(input);
            _damageInputs[entry.Address] = input;

            var apply = new DarkButton
            {
                Text = "Apply",
                Location = new Point(x + 272, y - 3),
                Size = new Size(56, 28),
                Style = DarkButtonStyle.Primary,
                Tag = entry,
            };
            apply.Click += async (s, _) =>
            {
                if (s is not Button b || b.Tag is not MoveDamageEntry e) return;
                var v = (int)input.Value;
                await Task.Run(() => _trainer.MoveDamage.SetDamage(e.Address, v));
            };
            content.Controls.Add(apply);
            _damageApplyButtons.Add(apply);
        }

        return section;
    }

    private void RelayoutAfterMoveDamage()
    {
        var sectionBottom = _moveDamageSection.Location.Y + _moveDamageSection.Height;
        foreach (Control c in Controls)
        {
            if (c.Tag is "footer")
                c.Location = new Point(14, sectionBottom + 12);
        }
        var footerBottom = sectionBottom + 36;
        if (ClientSize.Height < footerBottom)
            ClientSize = new Size(ClientSize.Width, footerBottom);
    }

    private GroupBox NewGroup(string title) => new()
    {
        Text = title,
        ForeColor = Color.Gainsboro,
        BackColor = Color.FromArgb(38, 38, 44),
    };

    private void StartCombatSyncLoop()
    {
        _combatSyncCts = new CancellationTokenSource();
        var token = _combatSyncCts.Token;
        _ = Task.Run(async () =>
        {
            var movesList = MoveCatalog.Moves.ToList();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!_trainer.Memory.IsAttached)
                    {
                        await Task.Delay(100, token);
                        continue;
                    }

                    var values = await Task.Run(() =>
                    {
                        var result = new int?[_combatCombos.Count];
                        for (var i = 0; i < _combatCombos.Count; i++)
                        {
                            if (_combatCombos[i].Tag is CombatSlot slot && !_combatFreezes[i].Checked)
                                result[i] = _trainer.Combat.GetSlotMove(slot.Address);
                        }
                        return result;
                    }, token);

                    BeginInvoke(new Action(() =>
                    {
                        _suppressEvents = true;
                        try
                        {
                            for (var i = 0; i < _combatCombos.Count; i++)
                            {
                                var v = values[i];
                                if (v is null) continue;
                                var idx = movesList.FindIndex(m => m.Id == v.Value);
                                if (idx >= 0 && _combatCombos[i].SelectedIndex != idx)
                                    _combatCombos[i].SelectedIndex = idx;
                            }
                        }
                        finally { _suppressEvents = false; }
                    }));

                    await Task.Delay(100, token);
                }
                catch (TaskCanceledException) { break; }
                catch { await Task.Delay(100, token); }
            }
        });
    }

    private void StopCombatSyncLoop()
    {
        if (_combatSyncCts is not null)
        {
            _combatSyncCts.Cancel();
            try { _combatSyncCts.Dispose(); } catch { }
            _combatSyncCts = null;
        }
    }

    private void StartUnlocksSyncLoop()
    {
        StopUnlocksSyncLoop();
        _unlocksSyncCts = new CancellationTokenSource();
        var token = _unlocksSyncCts.Token;
        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!_trainer.Memory.IsAttached)
                    {
                        await Task.Delay(100, token);
                        continue;
                    }

                    var skip = (bool)Invoke(new Func<bool>(() => _rouletteAvailableFreeze.Checked));
                    if (!skip)
                    {
                        var read = await Task.Run(() => _trainer.Unlocks.GetRouletteAvailable(), token);
                        BeginInvoke(new Action(() =>
                        {
                            _suppressEvents = true;
                            try
                            {
                                var display = (decimal)(read ?? 0);
                                if (_rouletteAvailableInput.Value != display)
                                    _rouletteAvailableInput.Value = display;
                            }
                            finally { _suppressEvents = false; }
                        }));
                    }

                    await Task.Delay(250, token);
                }
                catch (TaskCanceledException) { break; }
                catch { await Task.Delay(250, token); }
            }
        });
    }

    private void StopUnlocksSyncLoop()
    {
        if (_unlocksSyncCts is not null)
        {
            _unlocksSyncCts.Cancel();
            try { _unlocksSyncCts.Dispose(); } catch { }
            _unlocksSyncCts = null;
        }
    }

    private async Task SyncPlayerMetersFromMemoryAsync()
    {
        var (gh, lvl, hp, heat) = await Task.Run(() =>
            (_trainer.Player.GetGodHandMeter(), _trainer.Player.GetLevelMeter(),
             _trainer.Player.GetHealthMax(), _trainer.Player.GetHeatGaugeMax()));

        _suppressEvents = true;
        try
        {
            if (gh is int g)
                _godHandMeterInput.Value = Math.Clamp(g, (int)_godHandMeterInput.Minimum, (int)_godHandMeterInput.Maximum);
            if (lvl is int l)
                _levelMeterInput.Value = Math.Clamp(l, (int)_levelMeterInput.Minimum, (int)_levelMeterInput.Maximum);
            if (hp is int h)
            {
                var clamped = Math.Clamp(h, _healthMaxSlider.Minimum, _healthMaxSlider.Maximum);
                _healthMaxSlider.Value = clamped;
                _healthMaxValueLabel.Text = clamped.ToString();
            }
            if (heat is int ht)
            {
                var clamped = Math.Clamp(ht, _heatGaugeMaxSlider.Minimum, _heatGaugeMaxSlider.Maximum);
                _heatGaugeMaxSlider.Value = clamped;
                _heatGaugeMaxValueLabel.Text = clamped.ToString();
            }
        }
        finally { _suppressEvents = false; }
    }

    private async Task SyncMoveDamageFromMemoryAsync()
    {
        var snapshot = _damageInputs.ToArray();
        var values = await Task.Run(() =>
        {
            var result = new Dictionary<uint, int?>();
            foreach (var (addr, _) in snapshot)
                result[addr] = _trainer.MoveDamage.GetDamage(addr);
            return result;
        });

        _suppressEvents = true;
        try
        {
            foreach (var (addr, input) in snapshot)
            {
                if (!values.TryGetValue(addr, out var v) || v is null) continue;
                var clamped = Math.Clamp(v.Value, (int)input.Minimum, (int)input.Maximum);
                input.Value = clamped;
            }
        }
        finally { _suppressEvents = false; }
    }

    private async Task ApplyGoldAsync()
    {
        var amount = (int)_goldInput.Value;
        await Task.Run(() => _trainer.Player.SetGold(amount));
    }

    private async Task ToggleTimeScaleAsync(bool enable)
    {
        _timeScaleEnable.Enabled = false;
        var pulsing = _timeScaleEnable as DarkCheckBox;
        pulsing?.BeginPulse();
        try
        {
            if (enable)
            {
                var ok = await Task.Run(() => _trainer.Gameplay.InstallTimeScale());
                if (!ok)
                {
                    _suppressEvents = true;
                    _timeScaleEnable.Checked = false;
                    _suppressEvents = false;
                    MessageBox.Show(this,
                        "Could not locate the time scale instruction. Load the game and try again.",
                        "Time Scale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    await Task.Run(() => _trainer.Gameplay.SetTimeScale(_timeScaleSlider.Value / 10f));
                }
            }
            else
            {
                await Task.Run(() => _trainer.Gameplay.UninstallTimeScale());
            }
        }
        finally
        {
            pulsing?.EndPulse();
            _timeScaleEnable.Enabled = true;
        }
    }

    private void UpdateTimeScale(float value, bool sourceIsSlider)
    {
        var clamped = Math.Clamp(value, GameplayCheats.MinTimeScale, GameplayCheats.MaxTimeScale);

        _suppressEvents = true;
        _timeScaleSlider.Value = (int)Math.Round(clamped * 10f);
        _suppressEvents = false;

        _timeScaleValueLabel.Text = $"{clamped:0.0}×";
        _ = Task.Run(() => _trainer.Gameplay.SetTimeScale(clamped));
    }

    private void RefreshAttachState()
    {
        var attached = _trainer.Memory.IsAttached;
        _statusDot.BackColor = attached ? Color.MediumSeaGreen : Color.IndianRed;
        _statusLabel.Text = attached
            ? $"Attached to pcsx2.exe (PID {_trainer.Memory.Process!.Id})"
            : "Searching for pcsx2.exe…";

        _goldApply.Enabled = attached;
        _goldInput.Enabled = attached;
        _godModeToggle.Enabled = attached;
        _godHandMeterInput.Enabled = attached;
        _godHandMeterApply.Enabled = attached;
        _godHandMeterFreeze.Enabled = attached;
        _levelMeterInput.Enabled = attached;
        _levelMeterApply.Enabled = attached;
        _levelMeterFreeze.Enabled = attached;
        _movesUnlockButton.Enabled = attached;
        _rouletteUnlockButton.Enabled = attached;
        _costumeCombo.Enabled = attached;
        _rouletteSlotsInput.Enabled = attached;
        _rouletteSlotsApply.Enabled = attached;
        _costumeFreeze.Enabled = attached;
        _rouletteSlotsFreeze.Enabled = attached;
        _rouletteAvailableInput.Enabled = attached;
        _rouletteAvailableApply.Enabled = attached;
        _rouletteAvailableFreeze.Enabled = attached;
        _levelMeterInput.Enabled = attached;
        _levelMeterApply.Enabled = attached;
        _levelMeterFreeze.Enabled = attached;
        _unlimitedKeysToggle.Enabled = attached;
        _healthMaxSlider.Enabled = attached;
        _healthMaxFreeze.Enabled = attached;
        _heatGaugeMaxSlider.Enabled = attached;
        _heatGaugeMaxFreeze.Enabled = attached;

        if (!attached)
        {
            _suppressEvents = true;
            _godModeToggle.Checked = false;
            _costumeFreeze.Checked = false;
            _rouletteSlotsFreeze.Checked = false;
            _rouletteAvailableFreeze.Checked = false;
            _rouletteAvailableInput.Value = 0;
            _godHandMeterFreeze.Checked = false;
            _levelMeterFreeze.Checked = false;
            _unlimitedKeysToggle.Checked = false;
            _healthMaxFreeze.Checked = false;
            _heatGaugeMaxFreeze.Checked = false;
            foreach (var f in _combatFreezes) f.Checked = false;
            _suppressEvents = false;
            _trainer.Freeze.ClearAll();
        }
        foreach (var c in _combatCombos) c.Enabled = attached;
        foreach (var f in _combatFreezes) f.Enabled = attached;
        foreach (var b in _combatPresetButtons) b.Enabled = attached;
        _hitboxToggle.Enabled = attached;
        _noLagToggle.Enabled = attached;
        _damageTypeToggle.Enabled = attached;
        _moveEffectCombo.Enabled = attached;
        _oneHitKillToggle.Enabled = attached;
        _guardBreakerToggle.Enabled = attached;
        _noDamageToggle.Enabled = attached;
        _walkThroughWallsToggle.Enabled = attached;

        if (!attached)
        {
            _suppressEvents = true;
            _hitboxToggle.Checked = false;
            _noLagToggle.Checked = false;
            _damageTypeToggle.Checked = false;
            _oneHitKillToggle.Checked = false;
            _guardBreakerToggle.Checked = false;
            _noDamageToggle.Checked = false;
            _walkThroughWallsToggle.Checked = false;
            _suppressEvents = false;
        }

        foreach (var input in _damageInputs.Values) input.Enabled = attached;
        foreach (var btn in _damageApplyButtons) btn.Enabled = attached;

        if (attached)
        {
            StartCombatSyncLoop();
            StartUnlocksSyncLoop();
            _ = SyncMoveDamageFromMemoryAsync();
            _ = SyncPlayerMetersFromMemoryAsync();
        }
        else
        {
            StopCombatSyncLoop();
            StopUnlocksSyncLoop();
        }
        _timeScaleEnable.Enabled = attached;
        _timeScaleSlider.Enabled = attached;

        if (!attached && _timeScaleEnable.Checked)
        {
            _suppressEvents = true;
            _timeScaleEnable.Checked = false;
            _suppressEvents = false;
        }
    }
}
