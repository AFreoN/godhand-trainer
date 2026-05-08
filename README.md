# God Hand Trainer

A Windows Forms trainer for the PS2 game **God Hand**, running through the **PCSX2** emulator. Built in C# on .NET 10, it provides a clean, themed UI for modifying in-game values and patching gameplay code on the fly.

## Compatibility

- **PCSX2 1.6.0 only** (32-bit). Newer PCSX2 builds allocate the EE memory base differently and are not supported.
- **Windows** with .NET 10 desktop runtime.
- Must be run **as Administrator**. The trainer needs `OpenProcess` with write access against `pcsx2.exe`.

## How it works

The trainer attaches to the running `pcsx2.exe` process and operates in two ways:

1. **Memory modification**: directly writes values at known PS2 addresses (gold, meters, move slots, etc.). These cheats apply as soon as the game is loaded.
2. **Code injection**: locates instructions in PCSX2's recompiler cache via AOB (array-of-bytes) scanning and patches them with `JMP` trampolines or single-byte flips. Because PCSX2 emits PS2 code into its recompiler region only when the game actually executes it, **a code-injection cheat can only be installed after the relevant action has occurred at least once in-game**.

   For example:
   - **Hitbox Large**: perform any combat move first so the hitbox code is emitted, then enable the toggle.
   - **One Hit Kill**: let an enemy take damage at least once so the damage routine is emitted, then enable.

   The trainer pulses the toggle while the AOB scan runs so you know when the patch has installed.

A **Freeze** option is available for many memory-backed values. When enabled, the trainer continuously re-writes the chosen value so the game cannot overwrite it.

## Features

### Player

- **Gold**: set the player's gold amount.
- **God Mode**: player becomes invincible and enemies cannot guard.
- **God Hand Meter**: value of the heat gauge.
- **Player Speed** (code injection): modifies the player's overall speed (animations, movement, etc.) without affecting enemies.

### Unlocks

- **Unlock All Moves**: unlocks every move in the move list.
- **Unlock All Roulettes**: unlocks every roulette move.
- **Double God Hand**: by default the player has a single God Hand. This option enables the double God Hand with the costume of your choice. Freeze it to keep the selection locked. A reset is required for the game to load the appropriate assets.
- **Roulette Slots**: number of roulette slots the player has, from 2 to 6.

### Gameplay

- **Level Meter**: difficulty meter, range 0-5000.
- **Unlimited Keys**: keys remain available at all times. Notably useful in stage 5-4 *Rock Star Tour Boat*, which normally starts with 15 keys and ends the minigame as soon as a boat is hit.
- **Walk Through Walls**: walk through walls without restriction.

### Combat Hooks (all code injection)

- **Hitbox Large**: every move uses the same enlarged hitbox.
- **Quick Move Relief**: zero recovery time after performing a combat move.
- **One Hit Kill**: enemies die in one hit.
- **No Damage**: player health is unaffected by incoming damage.
- **Guard Breaker**: every move breaks enemy guard.
- **Damage Type**: controls which effect is applied to enemies on attack.

### Combat (slot assignment)

- **Presets**: load a preset move loadout into the input slots.
- **Per-slot mapping**: assign any move to any input slot (Triangle, Down + Triangle, Cross, etc.).

### Moves Damage

- Adjust the damage output of individual moves. The catalog covers the 73 moves with known damage addresses.

## Build & run

```
dotnet build
```

Run the produced executable **as Administrator**:

```
bin/Debug/net10.0-windows/GodHandTrainer.exe
```

The application manifest already requests elevation, so launching from Explorer triggers the standard UAC prompt. Running `dotnet run` from a non-elevated shell will also prompt for elevation.

## Usage

1. Start PCSX2 1.6.0 and load God Hand.
2. Launch `GodHandTrainer.exe` as Administrator. The trainer auto-attaches as soon as `pcsx2.exe` is detected.
3. Apply memory cheats at any time after the game is loaded.
4. For code-injection cheats, perform the relevant in-game action at least once before toggling the cheat on.

## License

See [LICENSE](LICENSE).
