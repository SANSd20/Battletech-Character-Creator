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
| Advanced combat practice equipment | 192 |
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
   Started: vintage personal armor, First SLDF armor kit entries, the
   straightforward archaic, vintage, and advanced weapon rows from PDF pages
   176 and 178, fixed-cost advanced implant rows from PDF pages 182, 184, 188,
   prosthetic enhancements from PDF page 189, cosmetic implants and adaptation
   kits from PDF page 191, advanced combat practice equipment from PDF page
   192, and light support vehicles from PDF page 196 are imported behind the
   Companion catalog toggle.
3. Extend equipment data only where needed for Companion-only fields such as
   maintenance, modification, legality, or conversion notes.
4. Add expanded trait metadata after the trait model can represent richer
   source notes, costs, restrictions, and linked equipment effects. Started:
   expanded Rank, Title/Bloodname, Vehicle, Custom Vehicle,
   Implant/Prosthetic, and Mutation reference entries are imported behind the
   Companion catalog toggle.
5. Add character templates as explicit presets, not as ordinary Life Module
   steps, so players can choose between guided creation and ready-made starts.
6. Defer creatures, world-building, campaign systems, and high-power NPC rules
   until the core player-character workflow is stable.

## First Import Candidates

The most practical first Companion import is the equipment expansion. It is
mostly catalog data, it fits the existing editor direction, and it has a smaller
blast radius than changing character creation math.

The first implemented batch covers PDF pages 176 and 178: vintage personal
armor, the First SLDF armor kit, archaic and vintage weapons, and advanced
personal weapons. These entries live in separate `resource/companion_*.dat`
files and are hidden unless Companion content is enabled. Specialty munitions
and ordnance remain deferred because the current catalog does not yet model
ammunition modifiers as first-class inventory rules.

The second equipment batch covers fixed-cost implant rows from PDF pages 182
and 184: Hostile Environment implants, Black Ops cybernetics, communication
implants, pheromone and toxin effusers, VDNI systems, pain shunts, secondary
power supplies, and the triple-core processor. These are represented as
Companion equipment entries with their equipment rating in the existing
armor/rating field. Wildcard-cost multimodal implants, percentage-cost dermal
systems, prosthetic enhancements, and cosmetic adaptation kits remain deferred
until the editor can represent their special cost and rule dependencies more
clearly.

The third equipment batch covers the fixed and base-cost extreme cybernetics
and prosthetics from PDF page 188: prosthetic jaws and fangs, prosthetic tails,
additional prosthetic limbs, glide wings, and flight wings. Entries whose table
costs are listed as base cost plus limb cost retain the base cost and call out
the extra limb cost requirement in notes.

The fourth equipment batch covers prosthetic enhancements from PDF page 189.
Because these are add-ons to prosthetic limbs rather than standalone weapons,
they are represented as Companion equipment entries. The editor warns when a
prosthetic enhancement is purchased without a prosthetic or implant host. Their
attack profile, power use, reload notes, and utility modifiers are preserved in
notes until the editor has a dedicated prosthetic enhancement model.

The fifth equipment batch covers exotic cosmetic implants and prosthetics from
PDF page 191: beauty and horror enhancements, cosmetic tail and wing
prosthetics, cosmetic claws, and centaur, mermaid, and naga adaptation kits.
These are represented as Companion equipment entries; the cosmetic claws row
uses the numeric additional cost and records the add-on nature in notes.

The sixth equipment batch covers advanced combat practice equipment from PDF
page 192: field simulation systems, null-network equipment, simulator pods, and
their command servers. These are represented as Companion equipment entries
with unit limits and special behavior preserved in notes.

The seventh equipment batch covers light support vehicles from PDF page 196.
Because the editor does not yet have a dedicated vehicle model, these are
represented as Companion equipment entries with their vehicle ratings in the
armor/rating field and vehicle armor, fuel, range, speed, crew, passenger, and
cargo details preserved in notes.

Expanded trait references now have a first source-tagged import behind the
Companion catalog toggle. Full expanded trait mechanics should follow after the
editor can represent source-tagged rule notes, restrictions, and linked
equipment effects. Character templates should wait until the start screen and
wizard can offer presets without confusing them with standard Life Module
choices.

The MW3 Skill Conversions table on PDF page 102 / printed page 100 has also
been audited. Every distinct AToW Skill column value is now present in the app's
skill catalog, including `/Any` conversion targets that remain placeholders
until a player chooses the concrete subskill. See `docs/MW3_CONVERSION_AUDIT.md`.

## Known Risks

Companion rules are optional and sometimes alter assumptions from the core book.
Treat each import as a feature with its own source label and regression tests.

Equipment expansion may require new fields before every item can be represented
cleanly. Current inventory totals count base purchase prices from slash, comma,
and wildcard cost formats, include explicitly purchased armor patches and ammo
packs, report pure wildcard prices as unresolved, and warn when prosthetic
enhancements are missing a prosthetic or implant host, but reload, patch repair
rules, ammunition modifier, and vehicle purchasing still need richer modeling.
Trait expansion may require more than the current name-and-XP model. Templates
may need a preset pipeline that can create a complete character without passing
through every normal wizard screen.
