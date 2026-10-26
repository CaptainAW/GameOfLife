# Game of Life — 64-bit sparse implementation

Conway's Game of Life over the **full signed 64-bit coordinate space**, implemented in C# / Unity.

The board is never materialised as a grid: only live cells are stored, in a `HashSet<Cell>` keyed by
two `long` coordinates. Memory and time scale with the number of **live cells**, not with the extent
of the board, so coordinates anywhere in the `long` range cost exactly the same.

Reads the start state from `input.txt`, runs 10 generations, writes the result to `output.txt`,
both in the [Life 1.06](https://conwaylife.com/wiki/Life_1.06) format.

## Running it

1. Open the project in Unity **6000.3.21f1** (scripting API level: .NET Standard 2.1).
2. Put the start state in `Assets/Resources/input.txt`.
3. Open `Assets/Scenes/GameScene.unity` and press **Play**.
4. `Assets/Resources/output.txt` is written next to the input; the console logs the cell counts
   and the full output path.

The iteration count and both file names are serialized fields on the `GameOfLifeRunner` component,
so they can be changed in the Inspector without touching code (defaults: `input.txt`,
`output.txt`, 10 generations).

> **Note:** paths resolve to `Application.dataPath/Resources`, which is `Assets/Resources` in the
> editor. In a built player that directory does not exist as a filesystem folder — `Resources`
> contents are packed into the build — so file I/O there is editor-only. Switch to
> `Application.persistentDataPath` if a standalone build is needed.

## Structure

```
GameOfLife/
├─ Assets/
│  ├─ Resources/
│  │  ├─ input.txt            start state (Life 1.06)
│  │  └─ output.txt           result, written on each run
│  ├─ Scenes/
│  │  └─ GameScene.unity      single scene, holds the runner GameObject
│  └─ Sources/
│     ├─ Cell.cs              readonly struct: two longs, value equality, hashing
│     ├─ LifeEngine.cs        the simulation: Step() / Run()
│     ├─ Life106Util.cs       Life 1.06 parsing and formatting
│     └─ GameOfLifeRunner.cs  MonoBehaviour entry point: file I/O + wiring
├─ Packages/
└─ ProjectSettings/
```

### `Cell.cs`

`readonly struct` holding `long X, Y`. Implements `IEquatable<Cell>` and overrides `GetHashCode`
(via `HashCode.Combine`) so it can be a dictionary/set key without boxing.

### `LifeEngine.cs`

One generation, in two passes:

1. Walk the live cells and increment a neighbour counter for each of their 8 surrounding cells in a
   `Dictionary<Cell, int>`. Only cells adjacent to something alive ever enter the map — everything
   else is implicitly dead and costs nothing.
2. Walk the counters and keep a cell if it has exactly 3 neighbours (birth, or survival) or exactly
   2 while already alive (survival). Cells with 0 neighbours never enter the map, which is the
   correct outcome anyway: they die.

That condition is the task's rule set restated — `B3/S23`.

Cost per generation is `O(L)` time and memory for `L` live cells; the neighbourhood map holds
roughly `3L` entries on clustered boards and up to `8L` on fully scattered ones.

**Edge of the coordinate space:** neighbours that would fall outside `long.MinValue ..
long.MaxValue` are skipped rather than wrapped, so the universe is clipped at the int64 boundary
instead of silently overflowing.

### `Life106Util.cs`

- `Parse` — skips blank lines and `#` comments (including the `#Life 1.06` header), requires exactly
  two signed 64-bit integers per line, and throws `FormatException` with the line number otherwise.
  Duplicate coordinates collapse naturally into the set.
- `Write` — streams straight into a `TextWriter` so a large result never has to exist as one giant
  string. Output is sorted by X then Y, making runs byte-for-byte reproducible and diff-friendly.
- `Format` — convenience wrapper over `Write` for tests.

Numbers are parsed and formatted with `CultureInfo.InvariantCulture`, and lines end with `\n`
rather than `Environment.NewLine`, so output does not depend on the machine's locale or platform.

### `GameOfLifeRunner.cs`

`MonoBehaviour` that resolves the two paths, runs parse → simulate → write on `Start`, and logs
either a summary or the failure reason. All I/O is wrapped in a try/catch so a missing or malformed
input surfaces as a single `Debug.LogError` instead of an unhandled exception.

## Input / output format

Life 1.06 is a flat list of live-cell coordinates, one `X Y` pair per line:

```
#Life 1.06
0 1
1 2
2 0
2 1
2 2
-2000000000000 -2000000000000
-2000000000001 -2000000000001
-2000000000000 -2000000000001
```
