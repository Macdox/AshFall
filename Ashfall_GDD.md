# Ashfall — Game Design Document
**Version 1.0 | Scope: 2-3 day solo build | Platform: itch.io (WebGL)**

---

## 1. Concept

**Genre:** 2D side-scrolling combat platformer
**Logline:** A lone knight cuts through a collapsing, ash-choked fortress to reach the throne room before it falls.
**Pillars** (every decision gets checked against these):
1. **Movement feels tight** — jumping and dashing must feel responsive, not floaty.
2. **Combat feels weighty** — hits land with feedback (shake, stop, particles).
3. **Short and finishable** — 3-5 minutes of gameplay, fully polished, beats 20 minutes half-done.

## 2. Story (minimal, environmental)

No dialogue, no cutscenes. Story is told through:
- Opening text card: *"The fortress is falling. So is everything you swore to protect. One corridor left."*
- Environmental ash/fire particles increasing in intensity as you progress
- Ending text card on boss defeat: *"Too late. But not for nothing."*

This costs near-zero dev time and gives the game emotional weight it wouldn't otherwise have.

## 3. Core Gameplay Loop

Run/jump through a linear corridor → fight 2 enemy types → reach boss arena → defeat boss → end card.

No branching paths. No inventory. No leveling. One life-path, designed to be replayed for score/time if you want to add that later.

## 4. Player Character — "The Knight"

**Movement:**
- Run (standard horizontal movement)
- Jump with coyote time (~0.1s) + jump buffering (~0.1s)
- Short dash (i-frames during dash — doubles as a dodge)

**Combat:**
- Melee attack: 2-hit combo, short-range arc (OverlapCircle/Box hit detection)
- Optional stretch goal if time allows: charged heavy attack

**Health:**
- 5 hit points, simple heart or bar UI
- Brief invincibility frames after taking damage
- Death = instant restart from level start (no checkpoints — keeps scope small)

## 5. Enemies

**Enemy A — "Ash Wretch" (melee, common)**
- Patrols a fixed range → detects player via raycast → chases → attacks in melee range
- 2 hit points, dies in 2 hits
- Placed 3-4 times through the level

**Enemy B — "Cinder Archer" (ranged, less common)**
- Stationary or short patrol → detects player → fires a slow projectile → retreats slightly
- 2 hit points
- Placed 1-2 times, positioned to force player to close distance or dodge

**Boss — "The Ashen Warden"**
- End-of-level encounter, 2 phases by HP threshold:
  - Phase 1 (100-50% HP): melee slam attack (telegraphed), player dodges/punishes
  - Phase 2 (50-0% HP): faster attacks + one ranged ash-burst attack
- 15-20 hit points total
- On defeat: triggers end text card

## 6. Level Design

- **One linear corridor**, roughly 60-90 seconds of traversal if played perfectly, 3-5 min realistic first playthrough
- Structure: intro platforming beat → Ash Wretch encounter → mixed platforming/combat → Cinder Archer encounter → short breather → boss arena
- Visual escalation: environment gets more ash/fire/crumbling as you approach the throne room (reinforces urgency without extra text)

## 7. Art & Audio Direction (scoped for solo/fast execution)

- **Art:** Dark silhouette-style sprites or simple pixel art; ash particle effects do a lot of visual heavy lifting cheaply
- **Palette:** Near-black backgrounds, warm orange/red accents (fire, ash, UI highlights)
- **Audio:** Free/CC0 SFX packs (itch.io, freesound.org) for hits/jumps/footsteps; one looping ambient track, no need for original composition
- **Juice (do not skip):** screen shake on hits, hit-stop (2-4 frames freeze), hit particles, camera punch on boss phase transitions

## 8. Explicit Out-of-Scope (do not build these — you will run out of time)

- No save system
- No checkpoints
- No multiple levels
- No inventory/upgrades/leveling
- No dialogue system or voice
- No settings menu beyond restart/quit
- No cutscenes — text cards only

## 9. Success Criteria

Game is "done" when: player can complete start-to-finish without bugs, combat feels responsive, boss fight has visible phase change, and it's playable in-browser on itch.io.
