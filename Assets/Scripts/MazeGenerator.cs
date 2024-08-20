using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject wallPrefab;
    public GameObject startPrefab;
    public GameObject finishPrefab;
    public GameObject agentPrefab;
    public int gridSize = 10;

    public CameraController cameraController;

    public GameObject mazeParent;  // A parent GameObject to hold all the maze elements
    public int[,] mazeGrid;
    public Vector2Int start;
    public Vector2Int finish;

    void Start()
    {
        cameraController.PositionCamera(gridSize);
        mazeParent = new GameObject();
        mazeParent.name = "Maze";
    }

    public void GenerateMaze()
    {
        // Clear the existing maze before generating a new one
        ClearMaze();
        mazeGrid = new int[gridSize, gridSize];

        // Initializes all cells to 1 (walls)
        InitializeGrid();

        start = ChooseEdgePoint();
        finish = ChooseEdgePointFarFromStart();

        mazeGrid[start.x, start.y] = 0; // Start position
        mazeGrid[finish.x, finish.y] = 0; // Finish position

        // Ensure a path is generated between start and finish
        GenerateSolvablePath(start, finish);

        // Validate the path to make sure it's clear from start to finish
        if (!ValidatePath())
        {
            // If the path is blocked, regenerate the maze
            Debug.Log("Maze generation failed. Regenerating...");
            GenerateMaze();
            return;
        }

        // After the guaranteed path, you can add more random paths to increase maze complexity
        AddRandomPaths();

        // Validate the maze again to ensure the path is still clear after adding complexity
        if (!ValidatePath())
        {
            // If the path is blocked after adding complexity, regenerate the maze
            Debug.Log("Maze generation failed after adding complexity. Regenerating...");
            GenerateMaze();
            return;
        }

        PlacePrefabs();
    }

    bool ValidatePath()
    {
        // Perform a BFS or DFS to check if there is still a valid path from start to finish
        bool[,] visited = new bool[gridSize, gridSize];
        return DFS(start, visited);
    }

    bool DFS(Vector2Int position, bool[,] visited)
    {
        if (position == finish)
        {
            return true;
        }

        visited[position.x, position.y] = true;

        List<Vector2Int> neighbors = GetValidNeighbors(position);
        foreach (Vector2Int neighbor in neighbors)
        {
            if (!visited[neighbor.x, neighbor.y])
            {
                if (DFS(neighbor, visited))
                {
                    return true;
                }
            }
        }
        return false;
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (cell.x > 0 && mazeGrid[cell.x - 1, cell.y] == 0)
            neighbors.Add(new Vector2Int(cell.x - 1, cell.y));
        if (cell.x < gridSize - 1 && mazeGrid[cell.x + 1, cell.y] == 0)
            neighbors.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.y > 0 && mazeGrid[cell.x, cell.y - 1] == 0)
            neighbors.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.y < gridSize - 1 && mazeGrid[cell.x, cell.y + 1] == 0)
            neighbors.Add(new Vector2Int(cell.x, cell.y + 1));

        return neighbors;
    }

    public void ClearMaze()
    {
        if (mazeParent != null)
        {
            foreach (Transform child in mazeParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("Maze parent not set. Unable to clear maze.");
        }
    }

    void InitializeGrid()
    {
        // Initialize all cells as walls (1)
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                mazeGrid[x, y] = 1; // Default to walls
            }
        }
    }

    Vector2Int ChooseEdgePoint()
    {
        // Randomly choose an edge point
        int x = Random.Range(0, gridSize);
        int y = (x == 0 || x == gridSize - 1) ? Random.Range(0, gridSize) : (Random.Range(0, 2) * (gridSize - 1));
        return new Vector2Int(x, y);
    }

    Vector2Int ChooseEdgePointFarFromStart()
    {
        Vector2Int point;
        do
        {
            point = ChooseEdgePoint();
        } while (Vector2Int.Distance(start, point) < gridSize / 2); // Ensuring it's far from start
        return point;
    }

    void AddRandomPaths()
    {
        for (int x = 0; x < gridSize; x += 2)
        {
            for (int y = 0; y < gridSize; y += 2)
            {
                if (mazeGrid[x, y] == 0) // If the cell is part of the valid path
                {
                    GenerateRandomPathFromPosition(new Vector2Int(x, y));
                }
            }
        }
    }

    void GenerateRandomPathFromPosition(Vector2Int position)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(position);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                stack.Push(current);

                Vector2Int chosenNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWallBetween(current, chosenNeighbor);
                mazeGrid[chosenNeighbor.x, chosenNeighbor.y] = 0; // Mark as part of the path

                stack.Push(chosenNeighbor);
            }
        }
    }

    void GenerateSolvablePath(Vector2Int start, Vector2Int finish)
    {
        // This method generates a path from start to finish
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();

            if (current == finish)
            {
                return; // Path to finish has been successfully carved
            }

            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                stack.Push(current);

                // Choose a random neighbor and carve a path
                Vector2Int chosenNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWallBetween(current, chosenNeighbor);
                mazeGrid[chosenNeighbor.x, chosenNeighbor.y] = 0; // Mark as part of the path

                stack.Push(chosenNeighbor);
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (cell.x > 1 && mazeGrid[cell.x - 2, cell.y] == 1)
            neighbors.Add(new Vector2Int(cell.x - 2, cell.y));
        if (cell.x < gridSize - 2 && mazeGrid[cell.x + 2, cell.y] == 1)
            neighbors.Add(new Vector2Int(cell.x + 2, cell.y));
        if (cell.y > 1 && mazeGrid[cell.x, cell.y - 2] == 1)
            neighbors.Add(new Vector2Int(cell.x, cell.y - 2));
        if (cell.y < gridSize - 2 && mazeGrid[cell.x, cell.y + 2] == 1)
            neighbors.Add(new Vector2Int(cell.x, cell.y + 2));

        return neighbors;
    }

    void RemoveWallBetween(Vector2Int current, Vector2Int neighbor)
    {
        Vector2Int wallPosition = (current + neighbor) / 2;
        mazeGrid[wallPosition.x, wallPosition.y] = 0; // Remove wall
    }

    void PlacePrefabs()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = new Vector3(x, 0, y);
                if ((x == start.x && y == start.y) || (x == finish.x && y == finish.y))
                {
                    continue;
                }

                if (mazeGrid[x, y] == 0)
                {
                    GameObject floor = Instantiate(cellPrefab, position, Quaternion.identity);
                    floor.transform.SetParent(mazeParent.transform);
                }
                else if (mazeGrid[x, y] == 1)
                {
                    GameObject wall = Instantiate(wallPrefab, position + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
                    wall.transform.SetParent(mazeParent.transform);
                }
            }
        }

        GameObject _start = Instantiate(startPrefab, new Vector3(start.x, 0, start.y), Quaternion.identity);
        GameObject _finish = Instantiate(finishPrefab, new Vector3(finish.x, 0, finish.y), Quaternion.identity);

        _start.transform.SetParent(mazeParent.transform);
        _finish.transform.SetParent(mazeParent.transform);
    }

    void SpawnAgent()
    {
        Instantiate(agentPrefab, new Vector3(start.x, 0, start.y), Quaternion.identity);
    }
}
