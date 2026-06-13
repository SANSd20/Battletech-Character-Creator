# Rules Source

The authoritative rules source for this migration is:

`A Time of War: The BattleTech RPG - Corrected Third Printing`

The local reference copy is not committed to this repository. The original Qt
code may help locate data, but the corrected rulebook takes precedence when
values, prerequisites, choices, terminology, or stage behavior disagree.

## Character Creation Page Map

| Subject | PDF page | Printed page |
| --- | ---: | ---: |
| Character creation begins | 50 | 48 |
| Life Modules overview | 63 | 61 |
| Affiliation modules | 65 | 63 |
| Early childhood modules | 77 | 75 |
| Late childhood modules | 79 | 77 |
| Higher education rules | 82 | 80 |
| Master schools list | 84 | 82 |
| Opposed traits and XP cancellation | 98 | 96 |
| Additional XP options | 99 | 97 |

## Implementation Policy

1. Verify module costs, fixed XP, flexible XP, prerequisites, and choices
   against the corrected printing.
2. Preserve rulebook terminology in displayed names.
3. Treat old-source spelling, duplicated entries, commented rules, and UI
   workarounds as non-authoritative.
4. Add a regression test when a rulebook comparison changes migrated behavior.
5. Higher education follows the rulebook field limits: exactly one Basic Field,
   no more than three total Fields, and Special Training requires an Advanced
   Field.
6. Life-module cost is tracked separately from XP allocated to the character
   sheet. The universal module costs 850 XP, and education fields cost 30 XP
   for each skill in the field.
7. Stage 3 is optional. When selected, it requires one Basic Field; an optional
   Advanced Field may be followed by either a second Advanced Field or Special
   Training, for no more than three Fields total.
8. Field entries marked `Any` require a concrete subskill selection.
   `Protocol/Affiliation` and `Streetwise/Affiliation` resolve from the
   character's selected affiliation rather than remaining literal sheet names.
9. Flexible XP pools may be divided among multiple eligible attributes,
   traits, and skills, but the complete pool must be allocated.
10. Education Fields award 30 XP per Skill, while the XP Pool pays 30 XP per
    Skill normally, 24 with an active Fast Learner Trait, or 36 with an active
    Slow Learner Trait.
11. Repeating the same Stage 4 module awards only its Skill and Flexible XP;
    its Attribute and Trait XP apply only on the first selection.

## Stage 0 Audit

All 68 sub-affiliations in the corrected printing are represented by name and
their fixed and selectable XP awards have been audited against printed pages
64-73. Affiliation-wide flexible XP for Minor Periphery, Major Periphery State,
and Deep Periphery is also implemented. Restrictions described only in module
notes remain part of the ongoing prerequisite audit.

## Stage 4 Progress

The corrected Stage 4 catalog currently includes:

- Agitator
- Combat Correspondent
- Clan Watch Operative (Homeworld and Invading Clan)
- Clan Warrior Washout (four destination castes)
- Civilian Job
- Cloister Training
- ComStar/Word of Blake Service (both orders)
- Covert Operations (eleven affiliation variants)
- Dark Caste
- Explorer
- Goliath Scorpion Seeker
- Guerilla Insurgent (Free Rasalhague and general)
- Merchant (four career variants)
- Ne'er-Do-Well
- Organized Crime (standard and Clan Dark Caste paths)
- Postgraduate Studies
- ProtoMech Pilot Training
- Scientist Caste Service
- Solaris Insider
- Solaris VII Games
- Think Tank
- To Serve and Protect
- Tour of Duty (Periphery, Inner Sphere, and Clan)
- Travel

All 24 Stage 4 base modules from the corrected printing are represented.

Each implemented module includes its corrected cost, time, fixed awards,
specialty choices, flexible-XP restrictions, affiliation context, and currently
enforceable prerequisites.

ComStar and Word of Blake characters select a separate birth affiliation.
Unlike an ordinary affiliation change, both modules retain their full XP cost
and effects, as required by the corrected printing.

Selecting Dark Caste changes the final Clan caste to Dark Caste. The current
wizard treats that selection as the act of leaving Clan society. A second
career may follow it, including the Clan Dark Caste Organized Crime path.

Characters preserve an ordered Stage 4 career history in the legacy file
format. The rules engine applies only Skill and Flexible XP when a repeatable
module is selected again, rejects non-repeatable modules, and recognizes prior
Tour of Duty and Dark Caste careers for later prerequisites. The creation
wizard exposes two ordered Stage 4 career selectors, including repeated
careers where the module permits them.
