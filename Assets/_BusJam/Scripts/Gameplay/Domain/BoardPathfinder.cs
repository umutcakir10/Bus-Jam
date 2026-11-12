using System.Collections.Generic;
using BusJam.Core;

namespace BusJam.Gameplay.Domain
{
    
    public sealed class BoardPathfinder
    {
        
        private static readonly GridPos[] Directions =
        {
            new GridPos(0, -1), // toward the front
            new GridPos(-1, 0), // left
            new GridPos(1, 0),  // right
            new GridPos(0, 1),  // toward the back
        };

        private readonly BoardModel _board;
        private readonly int[] _queue;        
        private readonly int[] _cameFrom;     
        private readonly int[] _visitStamp;   
        private int _stamp;

        public BoardPathfinder(BoardModel board)
        {
            _board = board;
            _queue = new int[board.CellCount];
            _cameFrom = new int[board.CellCount];
            _visitStamp = new int[board.CellCount];
        }

        
        public bool TryFindExitPath(GridPos start, List<GridPos> path)
        {
            path.Clear();
            if (!_board.IsInside(start))
                return false;

            if (start.Y == 0)
            {
                path.Add(start);
                return true;
            }

            _stamp++; 

            int startIndex = _board.ToIndex(start);
            int head = 0;
            int tail = 0;
            _queue[tail++] = startIndex;
            _visitStamp[startIndex] = _stamp;
            _cameFrom[startIndex] = -1;

            while (head < tail)
            {
                int current = _queue[head++];
                GridPos currentPos = _board.ToPos(current);

                for (int d = 0; d < Directions.Length; d++)
                {
                    GridPos next = currentPos + Directions[d];
                    if (!_board.IsWalkable(next))
                        continue;

                    int nextIndex = _board.ToIndex(next);
                    if (_visitStamp[nextIndex] == _stamp)
                        continue;

                    _visitStamp[nextIndex] = _stamp;
                    _cameFrom[nextIndex] = current;

                    if (next.Y == 0)
                    {
                        BuildPath(nextIndex, path);
                        return true;
                    }

                    _queue[tail++] = nextIndex;
                }
            }

            return false;
        }

        private void BuildPath(int endIndex, List<GridPos> path)
        {
            
            for (int i = endIndex; i != -1; i = _cameFrom[i])
                path.Add(_board.ToPos(i));

            path.Reverse();
        }
    }
}