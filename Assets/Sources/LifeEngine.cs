using System.Collections.Generic;

namespace GameOfLife
{
    public static class LifeEngine
    {
        public static HashSet<Cell> Step(HashSet<Cell> aliveCells)
        {
            // Capacity is only a hint: Count * 8 overflows int past ~268M cells, so fall back to Count.
            int capacity = aliveCells.Count <= int.MaxValue / 8 ? aliveCells.Count * 8 : aliveCells.Count;
            var counts = new Dictionary<Cell, int>(capacity);
 
            foreach (var cell in aliveCells)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    // Skip neighbours outside the int64 range
                    if (dx == 1 && cell.X == long.MaxValue) continue;
                    if (dx == -1 && cell.X == long.MinValue) continue;
 
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        if (dy == 1 && cell.Y == long.MaxValue) continue;
                        if (dy == -1 && cell.Y == long.MinValue) continue;
 
                        var n = new Cell(cell.X + dx, cell.Y + dy); // safe: checked above
                        counts.TryGetValue(n, out int v);
                        counts[n] = v + 1;
                    }
                }
            }
 
            var next = new HashSet<Cell>();
            foreach (var (cell, aliveNeighbours) in counts)
            {
                if (aliveNeighbours == 3 || (aliveNeighbours == 2 && aliveCells.Contains(cell)))
                    next.Add(cell);
            }
            return next;
        }
 
        public static HashSet<Cell> Run(HashSet<Cell> start, int generations)
        {
            var state = start;
            for (int i = 0; i < generations; i++)
                state = Step(state);
            return state;
        }
    }
}