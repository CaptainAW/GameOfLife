using System;

namespace GameOfLife
{

    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly long X;
        public readonly long Y;

        public Cell(long x, long y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Cell o) => X == o.X && Y == o.Y;
        public override bool Equals(object obj) => obj is Cell c && Equals(c);
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}