# Veil Game Context

- **Goal**: Explore the 2D rooms, find the single key hidden in a drawer, then interact with the exit door to win. Getting caught by the enemy triggers a jump-scare and a lose screen.
- **Scenes/flow**: Start screen (`StartButtonPlay*`, `StartScreenUI`) loads the instructions (`InstructionsFlow` or `InstructionsScreen`), which then loads the main game scene. `GameManager` tracks win/lose state, key ownership, and restart (`R` after end).

## Player
- Movement via Input System (`PlayerController` or `PlayerMover2D`); aim toward mouse. Facing visuals handled by `PlayerFacing2D` (drives Animator with `Speed` float and `Dir4` int: 0=Right, 1=Left, 2=Up, 3=Down; if Animator assigned, it no longer flips/swaps sprites, leaving visuals to the Animator; falls back to sprite swapping if no Animator). `PivotAimByVelocity2D` can rotate child art.
- Interactions: `PlayerInteractor` scans a radius for `IInteractable` objects and calls `Interact` (mapped to the Interact action / E).
- Inventory: `PlayerInventory` proxies key state to `GameManager`.

## Items and UI
- **Flashlight** (`Flashlight`): Has limited charges (HUD in `BatteryHUD`), directional flash stuns enemies in a forward box (`enemyMask`, `wallMask` for line-of-sight). Uses UI overlays: `filmImage`, `whiteFlash`, `usedFilm` directional sprites. Cooldown between uses; charges can be refilled.
- **Batteries** (`BatteryPickup`): Interactable if player flashlight is not full; refills to max and can spawn VFX/SFX.
- **Drawers** (`Drawer`): Interactable containers that open UI via `DrawerUIManager`. Loot types: `Key`, `Battery`, or `None`. SFX optional.
  - `DrawerUIManager` plays open animation, pauses time once open, can trigger a non-lethal scare overlay (random `scareChance`) before showing contents. Supports optional Animators on the scare image (auto-restarts on scare) and the item icon (e.g., looping key animation). Take/Close buttons: taking key updates `PlayerInventory`, taking battery refills flashlight.
  - `DrawerLootManager` assigns exactly one key to a random drawer at start; others get battery or empty based on `batteryChance`.
- **Door** (`Door`): Requires key; on success disables colliders, plays open SFX, and calls `GameManager.Win()`. Without key, plays locked SFX.
- **HUD**: `KeyHUD` listens to `GameManager.KeyChanged`; `BatteryHUD` shows current flashlight charges.

## Enemies and hazards
- **Enemy** (`Enemy`): Patrol/chase AI with distance thresholds; flips sprite facing. Can be stunned (`IStunnable`) which pauses movement and optionally swaps sprite/tint. If close enough or colliding after grace period, triggers `JumpScareManager.Trigger()`.
- **EnemyStunnable**: Simple wrapper to disable an AI script for a duration.
- **JumpScareManager**: Full-screen image + optional audio; fades in, pauses time, holds, optional fade-out, then calls `GameManager.Lose()` (keeps time paused until restart). Supports an optional Animator on the jump-scare Image to play a frame animation (auto-restarts the animator on trigger).
- **ThunderManager**: Periodic lightning flash on UI, temporarily disables the dark film from the flashlight and plays optional thunder audio.
- **StartSceneLightning**: Randomly triggers a non-looping lightning animation on the start screen (Animator on lightning Image; uses unscaled time); optional audio (AudioSource + thunder clip).
- **LetterboxCamera**: Keeps a fixed aspect ratio (e.g., 701x588) by adding black bars (camera.rect) instead of stretching; attach to the main camera (useful for WebGL fullscreen).

## Audio hooks
- Player footsteps: `PlayerMover2D` supports optional looping footstep AudioSource/Clip (plays while moving above min speed; configurable base pitch + optional pitch jitter).
- Enemy footsteps: `Enemy` supports optional looping footstep AudioSource/Clip (can be chase-only or also patrol via `footstepsInPatrol`; separate patrol/chase pitch, min speed, optional jitter). Footsteps stop when the game ends (Win/Lose).

## World/Spawning
- **RoomSpawner**: Uses a BoxCollider2D volume to spawn content. After `enemySpawnDelay`, optionally spawns an enemy at a random valid point not too close to the player (`blockedMask` avoids walls). Immediately spawns 0..N batteries. Drawer spawning is currently commented out here.

## Input highlights (Unity Input System)
- Movement: Vector2 action (`OnMove`). Aim: mouse-based in `PlayerController`/`PlayerFacing2D`.
- Interact: `PlayerInteractor.OnInteract` methods (Send Messages binding).
- Flashlight: `Flashlight.OnFlashlight` methods.
- Pause: `GameState.OnPause` via `PauseForwarder`.
- Restart after end: `R` key handled in `GameManager.Update()`.

## File map (key scripts)
- Core state: `Assets/Scripts/GameManager.cs`, `Assets/Scripts/GameState.cs`.
- Player: `Assets/Scripts/PlayerController.cs`, `Assets/Scripts/PlayerMover2D.cs`, `Assets/Scripts/PlayerFacing2D.cs`, `Assets/Scripts/PlayerInteractor.cs`, `Assets/Scripts/PlayerInventory.cs`, `Assets/Scripts/PivotAimByVelocity2D.cs`.
- Items/UI: `Assets/Scripts/Flashlight.cs`, `Assets/Scripts/BatteryHUD.cs`, `Assets/Scripts/BatteryPickup.cs`, `Assets/Scripts/Drawer.cs`, `Assets/Scripts/DrawerUIManager.cs`, `Assets/Scripts/DrawerAnimEvents.cs`, `Assets/Scripts/DrawerLootManager.cs`, `Assets/Scripts/KeyHUD.cs`, `Assets/Scripts/KeyPickup.cs`, `Assets/Scripts/Door.cs`.
- Enemies/FX: `Assets/Scripts/Enemy.cs`, `Assets/Scripts/EnemyStunnable.cs`, `Assets/Scripts/JumpScareManager.cs`, `Assets/Scripts/ThunderManager.cs`, `Assets/Scripts/RoomSpawner.cs`.
- UI flow: `Assets/Scripts/StartScreenUI.cs`, `Assets/Scripts/StartButtonPlay.cs`, `Assets/Scripts/StartButtonPlaySimple.cs`, `Assets/Scripts/InstructionsFlow.cs`, `Assets/Scripts/InstructionsScreen.cs`, `Assets/Scripts/PauseForwarder.cs`, `Assets/Scripts/InstructionScreen.cs`, `Assets/Scripts/Interactor.cs`.
