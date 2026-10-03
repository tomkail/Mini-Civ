# Animal Fruit Picker — Design Doc

> **Status:** Draft v0.4. Sections marked **[Proposal]** are new suggestions that build on the original notes. Everything else comes straight from them. Open questions are collected at the end.

## 1. Pitch

A tiny turn-based tactics game on a hex grid. You play a small animal crossing a land hidden by fog, hunting rare fruit to sell in villages, year after year, and never getting caught out when winter comes.

Every fruit grows by readable rules: certain terrain, certain cluster sizes, certain neighbours. Uncovering the land is therefore also a logic puzzle. Each hex you reveal narrows down where the fruit must be. Each fruit you spot tells you where its siblings are hiding.

**One-line hook:** Minesweeper deduction meets a seasonal travel route. Map the land, work out where the fruit grows, and get home before the snow.

## 2. Design pillars

1. **Information is the real resource.** Moves and energy matter, but the interesting choices are about what you know and what you're guessing. Each run should move from "where am I?" to "I know exactly where the Skyapples are, but can I reach them in time?"
2. **Rules you can read.** Fruit placement, terrain costs and seasons follow simple, consistent rules that a player can learn and use. The generator must never break a rule the player has been told.
3. **Small and cosy, with a real deadline.** Short runs on a small map. Winter is the pressure: soft and charming, but it can trap you.
4. **Explore first, then claim.** A run has two halves: revealing the map, then using what you learned to harvest efficiently. The best moments come when the two overlap.

## 3. Core loop

```
Start in a village (one other village marked on the map)
  └─ Plan a route → Travel (reveal terrain) → Spot fruit / deduce clusters
       └─ Harvest → Reach a village → Sell, rest, buy, hear rumours
            └─ Repeat until winter… survive it… spring comes again
```

### Run structure

- **Turns are fast and a run spans many seasons.** A season is 4 turns and should take under a minute to play, so a year is a few minutes and a run lasts several years.
- **4 seasons × 4 turns = 16 turns per year.** Start at the beginning of spring. Winter arrives on turn 13 of each year.
- You start in a village with a blank map. The location of one other village is marked.
- **Villages are always connected by paths (roads).** Once you know where two villages are, you can make a decent guess at how many turns the trip takes.
- **Short and replayable.** Each run is self-contained, on a fresh map. There's no campaign between runs.

### 3.1 The yearly rhythm [Proposal]

Because the map carries over from year to year, each year plays differently from the last:

| | What persists across years | What resets each spring |
|---|---|---|
| **Map** | **Terrain you've revealed** stays revealed. | — |
| **Fruit** | The rules, your field guide, and **most groves**: many fruits come back in the same spots. | **Some fruit moves**, depending on its type (see §9, Regrowth). Growth fog returns over everything you aren't standing next to, so even fixed groves need checking. |
| **World** | Bridges built, forests cleared, items bought. | — |
| **You** | Coins, upgrades, village relationships. | Energy refills; winter's fruit is eaten. |

This produces a natural arc within a run:

- **Year 1: exploration.** Most of the map is unknown. You're finding villages, roads and mountains, and the fruit you find is mostly luck along the way.
- **Years 2+: routes and deduction.** You know the terrain and where the fixed groves are, so you start each spring with a dependable "orchard route". The fruit that moves is that year's puzzle: *"the Flufffruit has blown east; which meadow is it in this year?"* Routes get tighter and more ambitious.
- **Investments pay off over time.** A bridge built in year 1 saves turns every year after. Services become real long-term decisions, not one-off costs.
- **[Proposal]** The map is bigger than one year can uncover. Distant villages and rarer biomes (caves, snowfields) sit beyond the first year's reach, giving later years new places to push into.

### 3.2 Ending a run [Open, options]

With many seasons per run, the ending needs to give years a purpose and stop the game becoming a solved map. Options:

| Option | How it ends | Strengths | Weaknesses |
|---|---|---|---|
| **A. Rising winter rent** | Each winter you must pay for a burrow in a village, and the price goes up every year. Can't pay, or caught outside with no fruit? You hibernate and the run ends. Score = years survived (+ coins). | Clear stakes every year. Naturally escalates, so the run ends on its own. Fits the cosy theme. | Needs tuning so rent outpaces a fully-mapped island. |
| **B. Fixed length** | Fixed number of years (e.g. 3, about 10–15 minutes). Score = coins at the end. | Predictable length; good for daily-seed leaderboards. | Less tension, because winter only costs you, it never ends you. |
| **C. Goal** | Run ends when you complete a goal, e.g. fill the field guide, or earn enough to buy a home orchard. Score = years taken. | A satisfying "I did it" ending; encourages exploring every biome. | Weaker pressure in the early years. |

**Leaning towards A.** It turns each winter into a checkpoint, and the escalating cost guarantees runs stay short. C's "fill the field guide" could be a secondary goal or achievement. Fruit in your basket when the run ends is worth nothing.

### Turn structure [Proposal]

Each turn you get **movement points (MP)**: 3 in spring, summer and autumn, and 1 in winter (from the notes: "you can only travel 1 tile per turn"). You spend MP to enter hexes, according to terrain cost. When you finish moving, fog updates and the turn ends.

Turns are the calendar. MP is how far you get within one turn. Because there are only 16 turns, every single turn counts.

## 4. Revealing: the information model

This is the gap the notes flag: *"Revealing should give you enough information to start claiming strategically, but there's still a bit of revealing left to add some uncertainty."*

### [Proposal] Two layers of fog

Hide the map in two separate layers:

| Layer | What it hides | How it's revealed |
|---|---|---|
| **Terrain fog** | What a hex *is* (grass, forest, river…) | Line of sight, mountain views, plains chunks, village maps |
| **Growth fog** | Whether a hex *has fruit* on it | Only up close: standing on the hex or next to it (or anywhere in line of sight within 1–2 hexes) |

Far-reaching reveals (climbing a mountain, being told about the area by a villager) only lift **terrain fog**. They show you where fruit *could* grow, not where it *does*. You fill in the rest by deduction:

- *"That's a cave pocket of exactly three hexes, and Glowberries only grow in clusters of three. If there are any Glowberries there, they fill the whole pocket."*
- *"I've found one Flufffruit, and they always grow in pairs. Its partner has to be one of these two unseen grassland hexes."*
- *"Dustdate never grows next to water, so the desert edge along the river is empty."*

This answers the design problem directly. The broad picture comes cheap and lets you plan. The exact layout stays uncertain until you commit to going there.

### Reveal sources

| Source | Reveals |
|---|---|
| Moving | Terrain + growth in the adjacent ring (forest blocks this) |
| **Mountain** | Terrain within a 2-hex radius (costs energy to climb) |
| **Plains** | Entering a plains region reveals the terrain of the whole connected plains area (a "chunk") |
| **Village** | Villagers reveal parts of the map: nearby terrain, the location of the next village, or a **rumour** |
| **[Proposal] Rumours** | Vague growth hints, e.g. *"I hear Skyapples were spotted on the northern peaks."* They point you at a region without giving exact hexes. |
| **Field guide** | Not a reveal, but the key that makes deduction possible. It lists each fruit's growing rules. |

**Rules aren't secret, but you learn them as you play.** The field guide is always open, and nothing is locked or bought. The game introduces fruits gradually instead of explaining them all up front:

- **[Proposal]** Each map grows only a subset of fruits (say 3–5), so a run is never a wall of ten rules at once.
- The first time you spot a fruit, the "new fruit found" interstitial shows its rule. That moment is when the rule becomes useful, because you can immediately apply it to find the rest of the cluster.
- Villagers mention fruit and rules in passing (*"Flufffruit always comes in pairs, you know"*), which doubles as a rumour about what grows nearby.
- New players naturally see the simpler fruits first (fixed group sizes on common terrain). The stricter ones appear on rarer biomes.

## 5. Claiming: harvesting

"Claiming" means harvesting. **Picking is free:** moving onto a fruit hex picks it automatically, at no extra MP. The cost of harvesting is the route itself, and how far out of your way the fruit takes you. Picked fruit goes into your **basket**.

- **Basket capacity** (e.g. 6 fruit to start) forces trips back to a village to sell and stops you from vacuuming up the whole map. Villages can sell bigger baskets.
- **Eating fruit** keeps you alive in winter, but every fruit eaten is a fruit you can't sell. That trade-off is what makes winter tense.

## 6. Resources

| Resource | What it does | Restored by |
|---|---|---|
| **Turns** | The calendar (16 per run) | Never |
| **MP** | Movement within a turn | Refills each turn |
| **Energy** | Stamina for climbing mountains and crossing rough terrain | Resting in a village |
| **Coins** | Score; spent on items and services | Selling fruit |
| **Basket** | Carrying capacity | Upgrades |

**Energy is its own resource**, separate from fruit. The notes suggest *"travel from village to village to restore your energy."* Energy is the leash that keeps you moving between villages, and it decides how far into rough country you can push before you have to turn back.

- **[Proposal]** Start with a small pool (e.g. 5). Mountains cost 2; swamp, snow and jungle cost 1 once added; everything else costs 0.
- **[Proposal]** Running out of energy doesn't end the run: you can still walk on cheap terrain, but you can't climb or enter rough ground until you rest.
- Keep fruit and energy separate. Fruit is money and winter food; energy is stamina. That keeps the two decisions readable: *"can I sell it?"* and *"can I reach it?"*
- It shouldn't be so tight that it becomes a second timer.

## 7. Seasons

| Season | Turns in year | Effects |
|---|---|---|
| Spring | 1–4 | Normal travel. **[Proposal]** Rivers swell: fords are impassable. |
| Summer | 5–8 | Normal travel. **[Proposal]** Most fruits ripe. |
| Autumn | 9–12 | Normal travel. **[Proposal]** Last chance for most fruits; prices rise as villages stock up for winter. |
| Winter | 13–16 | Snow: 1 hex per turn. Each turn spent outside a village costs 1 fruit from your basket. Villages are shelter. **Don't get trapped out.** |

Winter comes round every year, so the seasonal cycle is the game's heartbeat: forage in the warm months, sell up in autumn, then get through the snow.

**[Proposal] Seasonal fruit** gives you a reason to go out in winter at all. Frostfig is only harvestable in winter and sells for a lot, so a well-planned winter route close to a village becomes a high-risk, high-reward endgame.

**[Proposal] Trapped in winter:** if you're outside a village with no fruit to eat, your animal curls up and hibernates, and the run ends. It's soft and in keeping with the animal theme, but it hurts. Under ending option A, failing to pay winter rent ends the run the same way.

## 8. Terrain

From the notes, with costs added as a **[Proposal]** starting point:

| Tile | Move cost | Vision | Notes |
|---|---|---|---|
| **Road** | ½ (2 hexes per MP) | Normal | Cheap to travel. Always connects villages. |
| **Plains / Grassland** | 1 | Normal | Revealed in a chunk when entered. |
| **Forest** | 2 | **Blocks vision** | Can be cleared by a village service. |
| **Mountain** | 2 + energy | **2-hex radius reveal** | The main scouting tool. |
| **River** | Impassable | Normal | Must be crossed at a bridge. |
| **Bridge** | 1 | Normal | Crosses rivers. Can be built by a village service. |
| **Village** | 1 | Normal | Winter shelter. Map info. Sell goods. Shop. |
| Desert *(later)* | 1 | **[Proposal]** Extended vision (+1) | Open sands. |
| Swamp *(later)* | 2 | Normal | |
| Cave *(later)* | 1 | **[Proposal]** Only visible from inside or next to it | Hidden pockets, good for clue puzzles. |
| Snow / Tundra *(later)* | 2 | Normal | |
| Jungle *(later)* | 2 | Blocks vision | Like forest, but in a warmer climate. |
| Rocky *(later)* | 1 | Normal | |

## 9. Fruit

### Rule grammar [Proposal]

Every fruit is described by four parts. This keeps rules readable for players, simple to enforce in the generator, and easy to show in the field guide:

- **Habitat:** which terrain it grows on.
- **Group size:** how many grow together in one connected cluster (1, 2, 3, 2–4…).
- **Neighbour constraint:** e.g. *next to water*, *never next to water*, *never next to its own kind*.
- **Regrowth:** how predictable the fruit is from one year to the next.

### Regrowth

Many groves are fixed, some change, and some fruit types are more predictable than others. Each fruit belongs to one of three regrowth types:

| Type | Each spring… | Feels like | Typical value |
|---|---|---|---|
| **Rooted** | Grows back in exactly the same hexes. The group size can vary within its range. | Trees and bushes. Part of your known route once found. | Low–medium: reliable income |
| **Drifting** | Moves a short distance (about 1–3 hexes), staying in the same habitat and still obeying its rules. | Seeds blowing in the wind, plants creeping along a riverbank. | Medium: you know roughly where to look, and deduction finds the exact spot |
| **Scattered** | Fully re-placed anywhere its habitat and rules allow. | Mushrooms, desert blooms. | High: a fresh puzzle every year |

**[Proposal] Some changes even to fixed groves**, so a familiar map never becomes fully solved:

- **Fallow years:** a rooted grove occasionally skips a year. You only find out when you get there, so a backup plan matters.
- **New groves:** each spring there's a chance of a new rooted grove appearing, more likely in parts of the map you haven't explored much.
- **Lost groves:** clearing a forest or building a bridge can destroy the grove that grew there. Building has a cost beyond coins.

The field guide shows each fruit's regrowth type alongside its rules, so players know what to rely on.

### Catalogue

| # | Fruit | Habitat | Group | Neighbour rule | Regrowth [Proposal] |
|---|---|---|---|---|---|
| 1 | **Glowberry** | Cave | 3 | — | Rooted |
| 2 | **Aqualime** | Any, near water | 1 | Adjacent to water | Drifting (along the water's edge) |
| 3 | **Spikeroot** | Desert | 1 | Never adjacent to another Spikeroot | Scattered |
| 4 | **Flufffruit** | Grassland | 2 | — | Drifting |
| 5 | **Skyapple** | Mountain top | 2–4 | — | Rooted |
| 6 | **Mudmango** | Swamp | 1 | Never adjacent to another Mudmango | Scattered |
| 7 | **Rockpear** | Rocky | 2–3 | — | Rooted |
| 8 | **Frostfig** | Snow | 2–4 | — | Scattered (winter only) |
| 9 | **Vineplum** | Jungle | any | Adjacent to a river | Drifting (along the river) |
| 10 | **Dustdate** | Desert | 3 | Never adjacent to water | Rooted (palm groves) |

Notes on the list:
- **Spikeroot and Mudmango** have the same rule shape on different terrain. That's fine, and it even helps players learn the rules. Spikeroot and Dustdate share the desert, which makes the desert a good "two-rule interaction" biome to save for later in the game.
- **Group size is the strongest deduction tool.** Fruits with fixed sizes (Glowberry 3, Flufffruit 2, Dustdate 3) give firmer clues than ranged ones (2–4). Introduce fixed-size fruits first.
- **Desert pairing:** Spikeroot (scattered) and Dustdate (rooted) share a biome but differ in predictability, so a desert has a dependable palm grove plus a yearly hunt.
- **Value [Proposal]:** rarer habitats, stricter rules and less predictable regrowth should sell for more. Each village sets its own prices (see §10).

### MVP fruit set [Proposal]

The prototype already has grass, forest, mountain, river and road. Starting with fruits that fit those tiles avoids needing new biomes:

1. **Flufffruit** (grassland, pairs, drifting): teaches group size.
2. **Aqualime** (near water, single, drifting): teaches neighbour rules.
3. **Skyapple** (mountain, 2–4, rooted): rewards climbing, and once found it becomes a reliable stop on your route.
4. A forest fruit (new, e.g. *Shadeplum*: forest, clusters of 3, scattered). It grows where you can't see and moves every year, which makes deduction matter most.

This set covers all three regrowth types.

## 10. Villages

- **Shelter** in winter.
- **Information:** villagers reveal map regions, point to the next village, and share rumours.
- **Market:** sell fruit. **You don't know what's valuable until you've explored.** **[Proposal]** Each village prices fruit by local scarcity: fruit that grows nearby sells cheap, and fruit from far away sells high. That gives you a reason to carry fruit *across* the map instead of selling it at the closest village.
- **Shop (items):** snow shoes (2 MP in winter), bigger basket, **[Proposal]** climbing gear (mountains cost no energy), lantern (see into caves).
- **Services:** forest clearing (turns a forest hex into plains), bridge building (puts a bridge over a river hex). **[Proposal]** Services cost coins and finish at the start of the next season, so you have to plan ahead. They're **permanent for the run**, so a bridge built early pays off every year after.

## 11. The animal [Proposal, optional]

Choosing an animal could change the rules a little and add replay variety without much extra content:

- **Squirrel:** forests cost 1, sees from treetops (+1 vision in forest).
- **Mole:** caves are revealed in a wider radius, can tunnel once per season.
- **Goat:** mountains cost no energy.
- **Otter:** can swim across a river once per season.

Leave this out until the core loop is fun.

## 12. Map generation

- A small hex island (the current prototype's procedural island is a good base), sized so a full run uncovers it gradually: about a year's worth of reachable land at first, with more beyond.
- **[Proposal]** 3–5 villages, all linked by a connected road network. The starting village and one marked village are always connected within about 4 turns of travel.
- Terrain regions sized to support fruit groups (e.g. cave pockets of at least 3 hexes).
- **Fruit regrows each spring by type:** rooted groves stay put (with fallow-year and new-grove rolls), drifting fruit moves a short distance, and scattered fruit is fully re-placed. Every placement must still satisfy every rule.
- **Fruit placement must satisfy every rule exactly.** Place fruit after terrain, then validate. Also make sure no rule is ever ambiguous in a way the player can't resolve. **[Proposal]** Aim for "deducible if you scout well" rather than full Minesweeper-style uniqueness.
- An optional fixed seed for daily runs and testing (the settings asset already supports a fixed seed).

## 13. Presentation and polish

- **Seasonal music:** the song cross-fades between instruments as the season changes, in the style of Banjo-Kazooie's dynamic music. Same melody, new arrangement for each season.
- **Interstitial art** when:
  - the season changes,
  - you find a new fruit (which also unlocks its field guide entry),
  - you reach a new village,
  - you talk to a villager.
- **[Proposal]** The map visibly changes season by season: blossom → lush → orange → snow. Snow in winter also shows at a glance where travel is slow.

## 14. Current prototype status

What exists in this repo (Mini-Civ) that the game can build on:

- Hex grid maths (`Assets/Scripts/Grid/Hex/`): axial, offset and cube coordinates, rings, corners.
- A procedural island generator with grass, forest, mountain, river and road (`TomsLevelGenerator`).
- Fog of war with outline rendering (`FogRenderer`, using UnityX island detection, which can also find connected plains "chunks").
- Click-to-move along an A* path (`PathFinder`).

Not built yet: turns/seasons, movement-point costs, growth fog, fruit, villages, economy, UI.

## 15. Roadmap [Proposal]

1. **Turn and season clock:** 16 turns, MP per turn, terrain costs, winter slowdown.
2. **Reveal rules:** mountain radius, plains chunks, forest blocking.
3. **Villages and roads** in the generator; rest and shelter.
4. **Growth fog and the 4 MVP fruits** with rule-validated placement; harvesting; basket.
5. **Selling and prices.** Yearly cycle and end-of-run condition (see §3.1–3.2). *First playable: is it fun to deduce and plan a route?*
6. Field guide, first-sighting interstitials and rumours.
7. Shop items and services.
8. New biomes and the rest of the fruit list.
9. Polish: seasonal music, interstitials, seasonal map art.

## 16. Open questions

1. **How does a run end?** Rising winter rent, fixed length, or a goal (§3.2). Leaning towards rising winter rent.
2. **How much do rules reveal?** Should the generator guarantee clusters can be fully deduced, or is some luck fine?
3. **Seasonal fruit availability:** which fruits are harvestable in which seasons?
4. **Energy tuning:** pool size, and which terrain costs energy.
5. **Combat or hazards:** none (pure puzzle and travel), or light hazards such as bears or storms?

### Decided

- **Format:** short, replayable, self-contained runs. No campaign.
- **Run length:** many seasons per run. Turns are fast, and a season (4 turns) should take under a minute.
- **Harvesting:** free. Moving onto a fruit hex picks it.
- **Energy:** its own resource, restored by resting in villages. Fruit is not used for energy.
- **Fruit rules:** not secret (the field guide is always open), but introduced gradually through play: a few fruits per map, and each rule shown on first sighting.
- **Regrowth:** many fixed groves, some change. Each fruit type has its own predictability (rooted, drifting, scattered).
