# Imported GitHub Issue Audit

Generated: 2026-07-17 20:55:56 -05:00

Source: `issues\issues.json`

This file summarizes the locally imported GitHub issue export without
modifying the exported `issues.json` snapshot. Use it to decide what
still needs manual testing or GitHub-side closure.

## Summary

| Issue | GitHub state | Local status | Next step |
| --- | --- | --- | --- |
| #7: Add a new Free XP section before final review. | OPEN | Implemented locally; ready for manual confirmation and GitHub closure | Confirm the workflow in the packaged app, then close the GitHub issue. |

## Details

### #7 Add a new Free XP section before final review.

- GitHub state: OPEN
- URL: https://github.com/SANSd20/Battletech-Character-Creator/issues/7
- Local status: Implemented locally; ready for manual confirmation and GitHub closure
- Evidence:
  - The wizard has a dedicated Free XP step before final review.
  - The Free XP step can spend remaining XP from one grouped target dropdown.
  - Free XP allocation rows can be removed individually without resetting all spending.
  - Free XP targets include current character Traits and Skills even when they come from generated, imported, or life-module-specific names.
  - Rule-check fixes moved to the Free XP step; Attribute, Trait, and Skill gaps can spend Free XP directly.
  - Non-XP rule-check issues navigate back to the relevant wizard stage, including Education issues returning to Stage 3.
- Next step: Confirm the workflow in the packaged app, then close the GitHub issue.

