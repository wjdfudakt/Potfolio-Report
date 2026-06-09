using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveToExit : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("가로 칸 수")]
    [SerializeField] private int width = 6;

    [Tooltip("세로 칸 수")]
    [SerializeField] private int height = 4;

    [Tooltip("칸 크기")]
    [SerializeField] private float cellSize = 1f;

    [Header("Path")]
    [Tooltip("경로 탐색 시작 칸")]
    [SerializeField] private Vector2Int start = new Vector2Int(0, 0);

    [Tooltip("경로 탐색 목표 칸")]
    [SerializeField] private Vector2Int goal = new Vector2Int(5, 3);

    [Tooltip("벽 목록과 위치")]
    [SerializeField]
    private Vector2Int[] walls =
    {
        new Vector2Int(2, 0),
        new Vector2Int(2, 1),
        new Vector2Int(2, 2)
    };

    [Tooltip("탐색이 끝없이 반복되지 않도록 막는 최대 반복 횟수입니다.")]
    [SerializeField] private int maxIterations = 100;

    private readonly List<Vector2Int> path = new List<Vector2Int>();
    private readonly HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
    private bool pathFound;
    private bool stoppedBySafetyLimit;

    private void OnValidate()
    {
        RebuildPath();
    }

    private void Awake()
    {
        RebuildPath();
    }

    private void RebuildPath()
    {
        path.Clear();
        closedSet.Clear();
        pathFound = false;
        stoppedBySafetyLimit = false;

        if (!IsInsideGrid(start) || !IsInsideGrid(goal) || IsWall(start) || IsWall(goal))
        {
            return;
        }

        List<Vector2Int> openSet = new List<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        Dictionary<Vector2Int, int> gCost = new Dictionary<Vector2Int, int>();

        openSet.Add(start);
        gCost[start] = 0;

        int iterationCount = 0;

        while (openSet.Count > 0)
        {
            iterationCount++;

            if (iterationCount > maxIterations)
            {
                stoppedBySafetyLimit = true;
                return;
            }

            Vector2Int current = GetLowestFCostNode(openSet, gCost);

            if (current == goal)
            {
                BuildPath(cameFrom, current);
                pathFound = true;
                return;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor) || IsWall(neighbor))
                {
                    continue;
                }

                int newGCost = gCost[current] + 10;

                if (!gCost.ContainsKey(neighbor) || newGCost < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = newGCost;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
    }

    private Vector2Int GetLowestFCostNode(List<Vector2Int> openSet, Dictionary<Vector2Int, int> gCost)
    {
        Vector2Int bestNode = openSet[0];
        int bestCost = GetFCost(bestNode, gCost);

        for (int i = 1; i < openSet.Count; i++)
        {
            int cost = GetFCost(openSet[i], gCost);

            if (cost < bestCost)
            {
                bestNode = openSet[i];
                bestCost = cost;
            }
        }

        return bestNode;
    }

    private int GetFCost(Vector2Int node, Dictionary<Vector2Int, int> gCost)
    {
        int g = gCost.ContainsKey(node) ? gCost[node] : 9999;

        int h = GetManhattanDistance(node, goal) * 10;

        return g + h;
    }

    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int next = node + direction;

            if (IsInsideGrid(next))
            {
                neighbors.Add(next);
            }
        }

        return neighbors;
    }

    private void BuildPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        path.Clear();
        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
    }

    private bool IsInsideGrid(Vector2Int node)
    {
        return node.x >= 0 && node.x < width && node.y >= 0 && node.y < height;
    }

    private bool IsWall(Vector2Int node)
    {
        foreach (Vector2Int wall in walls)
        {
            if (wall == node)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        RebuildPath();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int node = new Vector2Int(x, y);

                Vector3 position = transform.position + new Vector3(x * cellSize, 0f, y * cellSize);

                Gizmos.color = GetNodeColor(node);
                Gizmos.DrawCube(position, Vector3.one * (cellSize * 0.85f));

                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(position, Vector3.one * (cellSize * 0.85f));
            }
        }

        DrawPathLines();
    }

    private Color GetNodeColor(Vector2Int node)
    {
        if (node == start)
        {
            return Color.green;
        }

        if (node == goal)
        {
            return Color.red;
        }

        if (IsWall(node))
        {
            return Color.black;
        }

        if (path.Contains(node))
        {
            return Color.yellow;
        }

        if (closedSet.Contains(node))
        {
            return stoppedBySafetyLimit ? Color.magenta : Color.cyan;
        }

        return Color.gray;
    }

    private void DrawPathLines()
    {
        if (!pathFound || path.Count < 2)
        {
            return;
        }

        Gizmos.color = Color.white;

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 from = transform.position + new Vector3(path[i - 1].x * cellSize, 0.55f, path[i - 1].y * cellSize);
            Vector3 to = transform.position + new Vector3(path[i].x * cellSize, 0.55f, path[i].y * cellSize);
            Gizmos.DrawLine(from, to);
        }
    }
}
