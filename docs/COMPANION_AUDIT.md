# A Time of War Companion Audit

The optional expansion source for this migration is:

`A Time of War Companion - First Printing Corrected with Errata v1.1`

The local reference PDF is not committed to this repository. Companion material
should be treated as optional expansion content, not as a replacement for the
core corrected-printing character creation rules.

## Import Policy

1. Keep the corrected third printing of the core rulebook authoritative for the
   default Life Module character wizard.
2. Label Companion material clearly in data, tests, and the user interface.
3. Do not let Companion options silently change core-rulebook defaults.
4. Prefer opt-in rules, filters, or presets for Companion character options.
5. Add focused tests before enabling any Companion option that changes XP,
   traits, skills, prerequisites, equipment totals, or character export.

## Companion Page Map

| Subject | PDF page |
| --- | ---: |
| Advanced trait rules begin | 29 |
| Expanded Rank trait | 29 |
| Expanded Title/Bloodname trait | 42 |
| Rank-to-title equivalency | 47 |
| Expanded vehicle and custom vehicle traits | 49 |
| Expanded implants and prosthetics traits | 51 |
| New Mutation trait | 55 |
| Advanced character creation begins | 78 |
| Optional life events | 78 |
| Character archetypes | 78 |
| Character templates begin | 87 |
| Hot Shot template | 88 |
| Grizzled Veteran template | 89 |
| Chopper Pilot template | 90 |
| Communications Specialist template | 91 |
| Sniper template | 92 |
| Canine Soldier template | 93 |
| Battle Armor Specialist template | 94 |
| Information Broker and Martial Artist templates | 95 |
| Previous-edition conversion guidance | 96 |
| Equipment expansion begins | 160 |
| Additional personal equipment begins | 174 |
| Advanced implants and prosthetics equipment | 180 |
| Light support vehicles | 193 |
| Mech-less warriors character guidance | 218 |
| Power player and high-power NPC guidance begins | 236 |
| Complete power character creation system | 242 |

## Suggested Import Order

1. Add source labels and settings for optional Companion material. Done: the
   editor catalog now has an opt-in Companion toggle, and catalog entries carry
   source labels.
2. Import additional personal equipment, weapons, armor, implants, and
   prosthetics where the current catalog model already has matching fields.
3. Extend equipment data only where needed for Companion-only fields such as
   maintenance, modification, legality, or conversion notes.
4. Add expanded trait metadata after the trait model can represent richer
   source notes, costs, restrictions, and linked equipment effects.
5. Add character templates as explicit presets, not as ordinary Life Module
   steps, so players can choose between guided creation and ready-made starts.
6. Defer creatures, world-building, campaign systems, and high-power NPC rules
   until the core player-character workflow is stable.

## First Import Candidates

The most practical first Companion import is the equipment expansion. It is
mostly catalog data, it fits the existing editor direction, and it has a smaller
blast radius than changing character creation math.

Expanded traits should follow after the editor supports source-tagged rule notes
and richer trait effects. Character templates should wait until the start screen
and wizard can offer presets without confusing them with standard Life Module
choices.

## Known Risks

Companion rules are optional and sometimes alter assumptions from the core book.
Treat each import as a feature with its own source label and regression tests.

Equipment expansion may require new fields before every item can be represented
cleanly. Trait expansion may require more than the current name-and-XP model.
Templates may need a preset pipeline that can create a complete character
without passing through every normal wizard screen.
