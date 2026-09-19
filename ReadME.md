# Ashfall

A short, polished 2D combat platformer built in Unity (C#) as a solo portfolio project. Cut through a collapsing fortress, fight through two enemy types, and take down a 2-phase boss before the throne room falls.

**[itch.io →](https://mandox.itch.io)**

![Gameplay Screenshot](./banner.png)

---

## About

Ashfall was built in ~3 days with a deliberately small scope, prioritizing game feel and polish over content length. No dialogue, no branching paths — just tight movement, weighty combat, and a short, complete experience.

## Features

- Responsive movement with coyote time and jump buffering
- Dash with brief invincibility frames
- Melee combat system with knockback and hit-stop feedback
- Two enemy types (melee chaser, ranged shooter) driven by finite state machines
- Boss fight with two distinct attack phases
- Screen shake, hit particles, and other game-feel polish
- WebGL build, playable directly in-browser

## Controls

| Action | Key                     |
| ------ | ----------------------- |
| Move   | A/D or Left/Right Arrow |
| Jump   | Space                   |
| Dash   | Shift                   |
| Attack | Left Mouse / J          |

<!-- Update this table to match your actual final input bindings -->

## Tech Stack

- **Engine:** Unity (version X.X here)
- **Language:** C#
- **Physics:** Unity 2D physics (Rigidbody2D)
- **Platform:** WebGL (itch.io)

## Project Structure

```
Assets/_Project/
  Scripts/
    Player/       — movement, combat, health, state machine
    Enemies/       — enemy AI (Ash Wretch, Cinder Archer, Ashen Warden boss)
    Combat/         — shared damage interface, hitboxes, projectiles
    Systems/       — game manager, UI, camera shake, hit-stop
  Prefabs/
  Sprites/
  Animations/
  Audio/
  Scenes/
```

## Running Locally

1. Clone the repo:
   ```
   git clone <your-repo-url>
   ```
2. Open the project folder in Unity Hub (version X.X or later).
3. Open `Assets/_Project/Scenes/Level_Main.unity`.
4. Press Play.

## Design & Technical Docs

Full design and architecture decisions are documented in:

- [`Ashfall_GDD.md`](./Ashfall_GDD.md) — Game Design Document
- [`Ashfall_TDD.md`](./Ashfall_TDD.md) — Technical Design Document

## Credits & Asset Attribution

- Sprites/tilesets: <!-- e.g. "Kenney.nl (CC0)" — list each pack used -->
- Sound effects: <!-- e.g. "Freesound.org contributors — see individual credits below" -->
- Ambient music: <!-- source/track name -->
- Fonts: <!-- if applicable -->

<!-- List individual CC-BY attributions here if any assets require it -->

## License

<!-- e.g. "Code: MIT License. Assets: see Credits section above for individual licenses." -->

---

Built solo by [@Macdox] as a portfolio project. Feedback welcome!
