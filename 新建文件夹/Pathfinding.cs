using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Pathfinding
{
    public static List<Vector3Int> FindPath(Tilemap tilemap, Vector3Int start, Vector3Int goal, HashSet<Vector3Int> validCells)
    {
        Dictionary<Vector3Int, Vector3Int> cameFrom = new();
        Dictionary<Vector3Int, int> costSoFar = new();

        PriorityQueue<Vector3Int> frontier = new();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();

            if (current == goal) break;

            foreach (var dir in directions)
            {
                Vector3Int next = current + dir;

                if (!validCells.Contains(next)) continue;
                if (!tilemap.HasTile(next)) continue;

                // ターゲット グリッドが開始点または終了点ではなく、
                // グリッド上にすでにユニットが存在する場合は、それをスキップします。
                if (TurnManager.Instance.GetUnitAtCell(next) != null && next != goal)
                    continue;

                int newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        List<Vector3Int> path = new();
        if (!cameFrom.ContainsKey(goal)) return path;

        Vector3Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

    static int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // 最小ヒープ
    private class PriorityQueue<T>
    {
        private List<(T item, int priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, int priority)
        {
            elements.Add((item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;
            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i].priority < elements[bestIndex].priority)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}
