# Ashfall — Technical Design Document
**Engine:** Unity (C#) | **Target:** WebGL build for itch.io

---

## 1. Project Structure

```
Assets/
  _Project/
    Scripts/
      Player/
        PlayerController.cs
        PlayerCombat.cs
        PlayerHealth.cs
        PlayerStateMachine.cs
      Enemies/
        EnemyBase.cs
        AshWretch.cs
        CinderArcher.cs
        Boss/
          AshenWarden.cs
          BossPhaseController.cs
      Combat/
        IDamageable.cs
        HitboxController.cs
        Projectile.cs
      Systems/
        GameManager.cs
        UIManager.cs
        CameraShake.cs
        HitStop.cs
    Prefabs/
    Sprites/
    Animations/
    Audio/
    Scenes/
      Level_Main.unity
```

Keep everything under `_Project` so it sorts above package folders — small thing, saves friction.

## 2. Core Systems

### 2.1 Player Movement (`PlayerController.cs`)
- `Rigidbody2D`-based horizontal movement, `velocity.x` set directly from input each `FixedUpdate`
- Jump: apply upward velocity impulse on button press if grounded OR within coyote timer
- **Coyote time:** float timer reset to ~0.1s when grounded, counts down when airborne; jump allowed if timer > 0
- **Jump buffering:** float timer set to ~0.1s on jump input; if grounded before timer expires, jump fires
- Ground check: `Physics2D.OverlapBox` at feet, against a `Ground` layer mask
- Dash: short burst of velocity in facing direction, ~0.15s duration, sets `isInvincible = true` during window, small cooldown after

### 2.2 State Machine (`PlayerStateMachine.cs` / `EnemyBase.cs`)
Simple enum-driven FSM, not a full class-per-state system (overkill for this scope):

```csharp
public enum PlayerState { Idle, Run, Jump, Fall, Attack, Dash, Hurt, Dead }
```
- One `currentState` field, a `switch` in `Update()` handling state-specific logic, and explicit transition rules (e.g., `Attack` can't be interrupted by movement input, `Hurt` briefly locks control)
- Enemies use the same pattern: `Patrol, Chase, Attack, Hurt, Dead`

### 2.3 Combat (`IDamageable.cs`, `HitboxController.cs`)
```csharp
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 knockbackDir);
}
```
- Player and all enemies implement `IDamageable`
- Attacks spawn a short-lived hitbox (`OverlapCircle`/`OverlapBox`) on an `Enemies` or `Player` layer mask, calling `TakeDamage` on anything hit
- Knockback: apply an `AddForce(Impulse)` in the hit direction on the receiving `Rigidbody2D`
- I-frames: bool flag + timer on `TakeDamage`, ignore further damage calls while active

### 2.4 Enemy AI
- **AshWretch:** `Patrol` (move between two points) → `Raycast` detects player in range → `Chase` (move toward player) → `Attack` when in melee range, cooldown between attacks
- **CinderArcher:** stationary/short patrol → detects player via distance check → `Attack` state instantiates a `Projectile` prefab (simple `Rigidbody2D` moving in a straight line, destroys on hit or lifetime expiry)
- **AshenWarden (boss):** `BossPhaseController.cs` checks HP percentage each frame; below 50% swaps an `attackPattern` enum/state set to the phase-2 behavior (faster attack cooldowns + new ranged attack state)

### 2.5 Game Feel Systems
- **CameraShake.cs:** coroutine that offsets the camera's local position randomly for a short duration, decaying over time. Trigger on hits and boss phase transitions.
- **HitStop.cs:** briefly sets `Time.timeScale` to a low value (e.g., 0.05) for a few frames, then restores — call on significant hits (not every jab, or it gets exhausting)
- Particle systems on hit (simple burst, reuse one prefab with color/size tweaks per context)

### 2.6 Game Flow (`GameManager.cs`)
- States: `Playing`, `Dead`, `Won`
- On player death: reload scene (no checkpoint system — matches GDD scope)
- On boss defeat: trigger end text card UI, stop enemy spawning/updates

### 2.7 UI (`UIManager.cs`)
- Health display (5 hearts or simple bar) bound to `PlayerHealth` events
- Restart button/key on death screen
- Text card system: simple `CanvasGroup` fade in/out for intro and outro text

## 3. Build Order (maps to GDD, sequenced for 3-day solo build)

**Day 1 — Core feel**
1. Player movement + coyote/buffer + dash
2. Basic combat: hitboxes, `IDamageable`, knockback, i-frames
3. Placeholder box-sprite testing scene

**Day 2 — Content**
4. AshWretch + CinderArcher AI
5. Boss with 2-phase logic
6. Block out the level geometry/corridor

**Day 3 — Polish + ship**
7. Camera shake, hit-stop, particles, UI (health, restart, text cards)
8. Art pass (swap placeholder sprites), audio pass (SFX + ambient loop)
9. WebGL build, upload + itch.io page setup, playtest pass for bugs

## 4. Performance/Technical Notes

- Object pool projectiles if the archer/boss fire frequently — avoids `Instantiate`/`Destroy` GC spikes even at this small scale, and it's a good habit
- Use `FixedUpdate` for all physics-affecting movement, `Update` for input reading and timers
- Keep all tunable values (speeds, damage, cooldowns) as `[SerializeField]` fields, not hardcoded — you will be tuning combat feel constantly on day 1

## 5. itch.io Publishing Checklist

- Player Settings → WebGL → compress build (Brotli or gzip)
- Test the actual WebGL build locally before upload (behavior can differ from editor)
- itch.io page: set "This file will be played in the browser," correct viewport dimensions, add a cover image + 2-3 screenshots/gif
- Add short description using the GDD logline
