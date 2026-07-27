# Daily Cycle System — Design Reference

## Purpose

The Daily Cycle System represents the core gameplay progression of Flipit. This is NOT a real-time day/night lighting system. It is a gameplay loop where each day begins at school and ends when the player returns home. The purpose is to create routine, progression, and replayability while keeping exploration enjoyable.

The Daily Cycle must integrate with existing systems without replacing or modifying validated implementations.

## Design Philosophy

Flipit is a relaxing exploration RPG. Players should enjoy walking through the city every day. The city should always feel alive. Every new day should feel familiar but slightly different.

Daily Events exist to create small gameplay variations that make the experience more enjoyable. The player should feel "Something different happened today" instead of "The game is making my life difficult."

The Daily Cycle should encourage the player to continue playing "just one more day."

## Persistent World Rules

The city is persistent. Critical constraints:

- Never regenerate the city after Day 1
- Never destroy existing GameObjects
- Never recreate the city
- Never duplicate NPCs
- All NPCs already exist from the beginning (Vendors, Challengers, Forced Encounter NPCs, Story NPCs, Mother NPC)
- Buildings, roads, props, decorations, and NPCs remain exactly where they are between days
- Only gameplay variables change

## Daily Gameplay Loop

```
School Spawn
  ↓
Player explores the city (Optional interactions, Combat, Shopping, Story progression)
  ↓
Player reaches home → Interacts with Mother NPC
  ↓
End of Day Transition → Random Daily Event → Next Day
  ↓
Player respawns at School
```

## Ending the Day

The day cannot end automatically. The player must interact with the Mother NPC (located outside the player's house) using the existing Dialogue System.

After dialogue finishes:
1. Disable player movement
2. Fade screen to black
3. Display "End of Day X" (X = current day number)
4. Wait a few seconds
5. Execute one Random Daily Event
6. Apply event effects
7. Increase day counter
8. Respawn player at School Spawn Point
9. Fade back in
10. Enable player movement

## Daily Events

Small gameplay variations that make each day unique without interrupting gameplay.

### Events must NEVER:
- Remove NPCs
- Spawn duplicate NPCs
- Regenerate the city
- Disable vendors or challengers
- Block game progression
- Force long detours
- Punish the player excessively

### Event Categories

**Positive (30%)**: Gain coins, gain Flipits, vendor discounts, better combat rewards
**Neutral (50%)**: Immersion changes (music, decorations, NPC behavior variations)
**Negative (20%)**: Minor losses (small coins, slight movement reduction for 30s, lose inexpensive consumable)

Negative events should never prevent progression. Positive events should be slightly more common than negative.

Probabilities must be configurable — never hardcoded.

## Event Architecture

- Base class: `DailyEvent` (all events inherit from it)
- Each event contains: Title, Description, Category, Probability, Execute()
- `DailyCycleManager` selects one event every day
- Adding new events must NOT require modifying existing code
- Prefer ScriptableObjects for event definitions

## DailyCycleManager API

Responsibilities: Current Day, End Day, Next Day, Random Event Selection, Player Respawn, Transition Sequence

```csharp
public void EndCurrentDay()
public void NextDay()
public void TriggerRandomEvent()
public void RespawnPlayer()
public int GetCurrentDay()
```

## Player Respawn

- Player always begins at school
- Never hardcode coordinates — use configurable School Spawn Point
- Respawn only changes player position; the world remains unchanged

## Mother NPC

- Reuse existing Dialogue System
- Responsible only for ending the day
- After dialogue completes: call `DailyCycleManager.EndCurrentDay()`
- No duplicate interaction logic

## Transition UI

Sequence: Fade Out → "End of Day X" → Random Event Card → Fade In

- Animations should be smooth
- Player cannot move during transition

## Editor Debug Tools

Menu path: `Flipit → Daily Cycle`

Options:
- Force Next Day
- Trigger Random Event
- Reset Day Counter
- Go To Day...

Display during development: Current Day, Coins, Flipits, Current Daily Event. Must be removable for release builds.

## Persistence

For now, keep all information in memory. Do NOT implement Save/Load. However, the architecture must allow Save/Load integration later without major refactoring.

## Future Expansion

Design the architecture to later support (without redesigning the DailyCycleManager):
- Weather, Weekdays, Holidays, Story Events, Festivals, Exams
- NPC schedules, Temporary city decorations, Seasonal events

## Development Rules

- Reuse existing systems — never rewrite validated systems
- Never replace existing implementations
- Prefer extension over modification
- If an existing validated system must change: explain why, list affected files, wait for approval
- Use Event-Driven communication whenever possible
- Avoid tightly coupled systems
- Follow SOLID principles

## Development Stages

Each stage must STOP and wait for validation before proceeding.

| Stage | Scope |
|-------|-------|
| 1 | DailyCycleManager, Day Counter, School Respawn System |
| 2 | Mother NPC integration, Dialogue integration, End Day Transition, Fade System |
| 3 | Daily Event System, Event Selection, Event Execution |
| 4 | Transition UI, Event Card UI, End of Day Presentation |
| 5 | Editor Debug Tools, Testing Utilities, Inspector Configuration |

### Stage Deliverables

At the end of every stage provide:
- Files Created
- Files Modified
- Unity Components Added
- Inspector Configuration
- Scene Changes
- Testing Instructions

Never continue to the next stage until approval is given.
