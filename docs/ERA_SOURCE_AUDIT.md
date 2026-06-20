# Era Source Audit

These PDFs are local reference sources only and are not committed to the
repository. They can inform future era-aware defaults, affiliation availability,
campaign context, equipment availability, and template work, but they do not
override the core character-creation rules in the corrected third printing.

## Local Sources

| Local file | PDF title | Pages | Initial use |
| --- | --- | ---: | --- |
| `d1 Era Digest Age of War [2398 - 2571] (Sourcebook).pdf` | `BattleTech: Era Digest: Age of War` | 36 | Era timeline and early-era context |
| `d2 Era Digest Golden Century [2830 - 2930] (Sourcebook).pdf` | `BattleTech: Era Digest: Golden Century` | 39 | Clan Golden Century context |
| `d3 Era Digest Dark Age [3128 - 3134] (Sourcebook).pdf` | `Battletech: Era Digest: Dark Age` | 51 | Dark Age context and faction state |
| `r1 Era Report 2750 [Star League era] (Sourcebook, Senario Pack).pdf` | `BattleTech: Era Report: 2750` | 162 | Star League era context |
| `r2 Era Report 3052 [3050-3052] (Sourcebook, Senario Pack).pdf` | `BattleTech: Era Report: 3052` | 170 | Clan Invasion era context |
| `r3 Era Report 3062 [3053-3062] (Sourcebook, Senario Pack).pdf` | `BATTLETECH ERA REPORT 3062` | 162 | Civil War era context |
| `r4 Era Report 3145 [3132-3145] (Sourcebook, Senario Pack).pdf` | `BattleTech: Era Report 3145` | 202 | Late Dark Age context |

## Implementation Policy

1. Treat these books as era and campaign context, not as replacements for
   *A Time of War* character-creation rules.
2. Do not enable era-specific options silently. Any future era filter, default
   game year, faction availability, or equipment availability rule should be
   visible to the user.
3. Add focused tests before an era source changes available affiliations,
   equipment, years, or templates.
4. Keep exact page references in future implementation notes when an era book
   changes behavior.

## Candidate Features

- Game-year presets for common eras.
- Era-aware affiliation and sub-affiliation availability filters.
- Era notes on the basic information or Stage 0 screen.
- Era-aware equipment availability and warnings.
- Character templates or quick-start examples for specific eras.
