using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public GameObject O_MazeGenerator;
    public MazeGenerator mazeGenerator;

    public bool isHuman;

    private Vector2Int currentGridPosition;

    void Start()
    {
        // Assuming the agent starts at the start position in the maze
        O_MazeGenerator = GameObject.Find("MazeGenerator");
        mazeGenerator=O_MazeGenerator.GetComponent<MazeGenerator>();
        currentGridPosition = mazeGenerator.start;
        transform.position = new Vector3(currentGridPosition.x, 0, currentGridPosition.y);
    }

    void Update()
    {
        if (isHuman)
        {
            // Handle player input for movement
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Move(Vector2Int.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Move(Vector2Int.down);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Move(Vector2Int.right);
            }
        }
    }

    void Move(Vector2Int direction)
    {
        Vector2Int newPosition = currentGridPosition + direction;

        // Check if the new position is within the maze bounds and is a valid move
        if (IsValidMove(newPosition))
        {
            // Update the agent's position
            currentGridPosition = newPosition;
            transform.position = new Vector3(currentGridPosition.x, 0, currentGridPosition.y);
        }
    }

    bool IsValidMove(Vector2Int position)
    {
        // Ensure the move is within bounds and not into a wall
        if (position.x >= 0 && position.x < mazeGenerator.gridSize &&
            position.y >= 0 && position.y < mazeGenerator.gridSize &&
            mazeGenerator.mazeGrid[position.x, position.y] == 0) // Check if it's a floor
        {
            return true;
        }
        return false;
    }
}
