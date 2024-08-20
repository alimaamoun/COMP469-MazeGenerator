using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator2 : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject wallPrefab;
    public GameObject startPrefab;
    public GameObject finishPrefab;
    public GameObject agentPrefab;
    public int gridSize = 8;
    public int numberOfMazes = 1;

    public List<GameObject> mazes = new List<GameObject>();
    public GameObject mazeParents = new GameObject(); //A Parent empty gameobject to hold all the separate mazes.
    public GameObject mazeParent;  // A parent GameObject to hold all the maze elements

    public List<GameObject[,]> grids = new List<GameObject[,]>();
    private List<bool[,]> visitedCells = new List<bool[,]>();
    private List<Vector2Int> frontiers = new List<Vector2Int>();
    private List<GameObject> agentInstances = new List<GameObject>();

    private Vector2Int[] directions = {
        new Vector2Int(0, 1), // North
        new Vector2Int(1, 0), // East
        new Vector2Int(0, -1), // South
        new Vector2Int(-1, 0)  // West
    };

    public List<Vector2Int> startCells = new List<Vector2Int>();
    public List<Vector2Int> goalCells = new List<Vector2Int>();

    void Start()
    {
        for (int i = 0; i < numberOfMazes; i++)
        {
            GenerateMazeInstance(i);
        }
    }

    public void GenerateMazeInstance(int mazeIndex)
    {
        mazeParent = new GameObject();
        mazeParent.name = "Maze" + mazeIndex;
        mazeParent.transform.SetParent(mazeParents.transform);
        mazes.Add(mazeParent);
        ClearMaze(mazeIndex);
        GenerateGrid(mazeIndex);
        GenerateMaze(mazeIndex);
        PlaceStartAndGoal(mazeIndex);
        //InstantiateAgent(mazeIndex);
    }

    void GenerateGrid(int mazeIndex)
    {
        GameObject[,] grid = new GameObject[gridSize, gridSize];
        bool[,] visited = new bool[gridSize, gridSize];
        grids.Add(grid);
        visitedCells.Add(visited);

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 position = new Vector3(x + (mazeIndex * (gridSize + 1)), 0, z);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                grid[x, z] = cell;
                visited[x, z] = false;

                PlaceWall(new Vector2Int(x, z), new Vector2Int(x, z + 1), mazeIndex);
                PlaceWall(new Vector2Int(x, z), new Vector2Int(x + 1, z), mazeIndex);
                if (z == 0) PlaceWall(new Vector2Int(x, z), new Vector2Int(x, z - 1), mazeIndex);
                if (x == 0) PlaceWall(new Vector2Int(x, z), new Vector2Int(x - 1, z), mazeIndex);
            }
        }
    }

    void PlaceWall(Vector2Int current, Vector2Int neighbor, int mazeIndex)
    {
        Vector2Int wallDirection = neighbor - current;
        Vector3 wallPosition = new Vector3(
            current.x + wallDirection.x * 0.5f + (mazeIndex * (gridSize + 1)),
            0.25f,
            current.y + wallDirection.y * 0.5f
        );
        Quaternion wallRotation = (wallDirection.x != 0) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        GameObject wall = Instantiate(wallPrefab, wallPosition, wallRotation);
        wall.transform.SetParent(mazes[mazeIndex].transform);
        wall.layer = LayerMask.NameToLayer("Wall");
    }

    public void GenerateMaze(int mazeIndex)
    {
        frontiers.Clear();
        Vector2Int startCell = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
        startCells.Add(startCell);
        AddToMaze(startCell, mazeIndex);

        while (frontiers.Count > 0)
        {
            int randomIndex = Random.Range(0, frontiers.Count);
            Vector2Int current = frontiers[randomIndex];
            frontiers.RemoveAt(randomIndex);

            List<Vector2Int> neighbors = GetVisitedNeighbors(current, mazeIndex);
            if (neighbors.Count > 0)
            {
                Vector2Int neighbor = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWall(current, neighbor, mazeIndex);
                AddToMaze(current, mazeIndex);
            }
        }

        Vector2Int goalCell = FindFurthestCell(startCell, mazeIndex);
        goalCells.Add(goalCell);
    }

    void AddToMaze(Vector2Int cell, int mazeIndex)
    {
        visitedCells[mazeIndex][cell.x, cell.y] = true;
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = cell + direction;
            if (IsWithinBounds(neighbor) && !visitedCells[mazeIndex][neighbor.x, neighbor.y] && !frontiers.Contains(neighbor))
            {
                frontiers.Add(neighbor);
            }
        }
    }

    List<Vector2Int> GetVisitedNeighbors(Vector2Int cell, int mazeIndex)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = cell + direction;
            if (IsWithinBounds(neighbor) && visitedCells[mazeIndex][neighbor.x, neighbor.y])
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    void RemoveWall(Vector2Int current, Vector2Int neighbor, int mazeIndex)
    {
        Vector2Int wallDirection = neighbor - current;
        Vector3 wallPosition = new Vector3(
            current.x + wallDirection.x * 0.5f + (mazeIndex * (gridSize + 1)),
            0.25f,
            current.y + wallDirection.y * 0.5f
        );

        Collider[] colliders = Physics.OverlapBox(wallPosition, new Vector3(0.4f, 0.4f, 0.4f));
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Wall"))
            {
                Destroy(collider.gameObject);
                break;
            }
        }
    }

    bool IsWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize && position.y >= 0 && position.y < gridSize;
    }

    Vector2Int FindFurthestCell(Vector2Int start, int mazeIndex)
    {
        Vector2Int furthest = start;
        float maxDistance = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int current = new Vector2Int(x, y);
                float distance = Vector2Int.Distance(start, current);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthest = current;
                }
            }
        }

        return furthest;
    }

    void PlaceStartAndGoal(int mazeIndex)
    {
        Vector3 startPosition = new Vector3(startCells[mazeIndex].x + (mazeIndex * (gridSize + 1)), 0, startCells[mazeIndex].y);
        GameObject start = Instantiate(startPrefab, startPosition, Quaternion.identity);
        start.transform.SetParent(mazes[mazeIndex].transform);

        Vector3 goalPosition = new Vector3(goalCells[mazeIndex].x + (mazeIndex * (gridSize + 1)), 0, goalCells[mazeIndex].y);
        GameObject finish = Instantiate(finishPrefab, goalPosition, Quaternion.identity);
        finish.transform.SetParent(mazes[mazeIndex].transform);
    }

    void InstantiateAgent(int mazeIndex)
    {
        if (mazeIndex >= startCells.Count)
        {
            Debug.LogError($"Invalid mazeIndex: {mazeIndex}. Not enough start cells defined.");
            return;
        }

        Vector3 agentStartPosition = new Vector3(startCells[mazeIndex].x + (mazeIndex * (gridSize + 1)), 0.5f, startCells[mazeIndex].y);
        GameObject agentInstance = Instantiate(agentPrefab, agentStartPosition, Quaternion.identity);
        agentInstances.Add(agentInstance);

        if (agentInstance.TryGetComponent(out Rigidbody agentRb))
        {
            agentRb.velocity = Vector3.zero;
            agentRb.angularVelocity = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("Rigidbody component not found on the agent prefab.");
        }

        agentInstance.transform.rotation = Quaternion.identity;

        if (agentInstance.TryGetComponent(out MouseAgent mazeAgent))
        {
            //mazeAgent.SetupAgent(mazeIndex, this);
            Debug.Log($"Agent instantiated for maze {mazeIndex} at position {agentStartPosition}");
        }
        else
        {
            Debug.LogError("MazeAgent component not found on the instantiated agent prefab.");
        }
    }

    public void ClearMaze(int mazeIndex)
    {

        // Check if there is a parent object holding the maze elements
        if (mazeParent != null)
        {
            // Loop through all the children of the mazeParent and destroy them
            foreach (Transform child in mazes[mazeIndex].transform)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("Maze parent not set. Unable to clear maze.");
        }

        // Optionally, clear or reset any data structures storing the maze layout
        // Example:
        // mazeGrid = new int[gridSize, gridSize]; // Reset the grid
    }

    public bool IsValidMove(Vector2Int from, Vector2Int to, int mazeIndex)
    {
        if (!IsWithinBounds(to))
            return false;

        Vector2Int difference = to - from;
        if (Mathf.Abs(difference.x) + Mathf.Abs(difference.y) != 1)
            return false; // Can only move to adjacent cells

        Vector3 wallCenter = new Vector3(
            from.x + difference.x * 0.5f + (mazeIndex * (gridSize + 1)),
            0.25f, // Adjust this height based on your wall height
            from.y + difference.y * 0.5f
        );

        // Use a smaller radius for more precise detection
        Collider[] colliders = Physics.OverlapSphere(wallCenter, 0.1f, LayerMask.GetMask("Wall"));
        return colliders.Length == 0;
    }
    public Vector2Int GetStartCell(int mazeIndex)
    {
        return startCells[mazeIndex];
    }

    public Vector2Int GetGoalCell(int mazeIndex)
    {
        return goalCells[mazeIndex];
    }
}