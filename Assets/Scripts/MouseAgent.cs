using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;
using TMPro;

public class MouseAgent : Agent {
    
    public MazeGenerator mazeGenerator;
    
    public Vector2Int currentGridPosition;

    private float timeSinceLastAction = 0f;
    private int lastAction = 4;
    Stack<int> actionStack = new Stack<int>(9);
    private float actionCooldown = .1f; // Adjust based on your frame rate

    private HashSet<Vector2Int> visitedLocations; // To track visited locations

    float distanceToGoal;
    float newDistanceToGoal;

    public TextMeshProUGUI rewardText; // Reference to the reward UI Text component
    public TextMeshProUGUI actionText; // Reference to the action UI Text component

    private float cumulativeReward;

    public override void Initialize()
    {
        //O_MazeGenerator = GameObject.Find("MazeGenerator");
        //mazeGenerator = O_MazeGenerator.GetComponent<MazeGenerator>();
        //Target = mazeGenerator.finish;
        visitedLocations = new HashSet<Vector2Int>();
        cumulativeReward = 0f;
        //mazeGenerator.GenerateMaze();
    }

    public override void OnEpisodeBegin()
    {
        // 1. Regenerate or reset the maze

        mazeGenerator.GenerateMaze();
        // 2. Reset the agent's position to the start of the maze
        currentGridPosition = mazeGenerator.start;
        transform.position = new Vector3(currentGridPosition.x, 0, currentGridPosition.y);
        // Perhaps UI? reset here.

        // Clear the visited locations set at the start of each episode
        visitedLocations.Clear();
        actionStack.Clear();
        visitedLocations.Add(currentGridPosition); // Add the start position as visited

        cumulativeReward = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //collect data of the world around the agent
        //Target location (normalized)
        sensor.AddObservation((float)mazeGenerator.finish.x / mazeGenerator.gridSize);
        sensor.AddObservation((float)mazeGenerator.finish.y / mazeGenerator.gridSize);
        //Agent location (normalized)
        sensor.AddObservation((float)currentGridPosition.x / mazeGenerator.gridSize);
        sensor.AddObservation((float)currentGridPosition.y / mazeGenerator.gridSize);
        // 3. Surrounding environment (Immediate neighboring cells)
        // Checking if the neighboring cells are open (0) or walls (1)
        sensor.AddObservation(IsCellOpen(currentGridPosition.x - 1, currentGridPosition.y)); // Left
        sensor.AddObservation(IsCellOpen(currentGridPosition.x + 1, currentGridPosition.y)); // Right
        sensor.AddObservation(IsCellOpen(currentGridPosition.x, currentGridPosition.y - 1)); // Down
        sensor.AddObservation(IsCellOpen(currentGridPosition.x, currentGridPosition.y + 1)); // Up
        //sensor.AddObservation(neighbors);
        // 4. Distance to the goal (optional, Euclidean distance)
        distanceToGoal = Vector2Int.Distance(currentGridPosition, mazeGenerator.finish);
        sensor.AddObservation(distanceToGoal / mazeGenerator.gridSize); // Normalized distance
    }

    private float IsCellOpen(int x, int y)
    {
        if (x >= 0 && x < mazeGenerator.gridSize && y >= 0 && y < mazeGenerator.gridSize)
        {
            return mazeGenerator.mazeGrid[x, y] == 0 ? 1.0f : 0.0f; // 1.0f if open, 0.0f if wall
        }
        return 0.0f; // Out of bounds is considered as a wall
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        cumulativeReward = 0.0f;
        if (Time.time - timeSinceLastAction < actionCooldown)
        {
            // Skip processing this action if still in cooldown
            return;
        }

        timeSinceLastAction = Time.time;

        // Get the action from the actionBuffers
        int action = actionBuffers.DiscreteActions[0];
        string actionDescription;

        if (action == 4) {
            AddReward(-0.02f);
            cumulativeReward += -0.02f;
            //punish doing nothing
        }
        
        Vector2Int moveDirection = Vector2Int.zero;

        if (!actionStack.Contains(action))
        {
            //reward choosing a different direction in the past 9 moves
            AddReward(.03f);
            cumulativeReward += 0.03f;
            actionStack.Push(action);
        }

        if (lastAction == 0 || lastAction == 1)
        {
            if (action != 0 && action != 1)
            {
                AddReward(0.01f); //reward for turning a corner
                cumulativeReward += 0.01f;
            }
        }

        switch (action)
        {
            case 0:
                moveDirection = Vector2Int.up;    // Move up
                actionDescription = "UP";
                break;
            case 1:
                moveDirection = Vector2Int.down;  // Move down
                actionDescription = "DOWN";
                break;
            case 2:
                moveDirection = Vector2Int.left;  // Move left
                actionDescription = "LEFT";
                break;
            case 3:
                moveDirection = Vector2Int.right; // Move right
                actionDescription = "RIGHT";
                break;
            case 4:
                //do nothing
                actionDescription = "NOOP";
                break;
            default:
                actionDescription = "INVALID";
                Debug.LogError("Invalid action received.");
                return;
        }

        lastAction = action;

        // Calculate the new position based on the action
        Vector2Int newPosition = currentGridPosition + moveDirection;
        newDistanceToGoal = Vector2Int.Distance(newPosition, mazeGenerator.finish) / mazeGenerator.gridSize;
        //Debug.Log(newDistanceToGoal);
        float distancePenalty = newDistanceToGoal * -0.01f;
        AddReward(distancePenalty);
        if (newDistanceToGoal < distanceToGoal)
        {
            AddReward(0.05f);
            cumulativeReward += 0.1f;
        }
        else
        {
            AddReward(-0.02f);
            cumulativeReward += -0.02f;
        }

        // Check if the new position is valid (within bounds and not a wall)
        if (IsValidMove(newPosition))
        {
            // Reward the agent for a valid move
            AddReward(-0.01f); // Small penalty to encourage shorter paths
            cumulativeReward += -0.01f;

            // Update the agent's position
            currentGridPosition = newPosition;
            transform.position = new Vector3(currentGridPosition.x, 0, currentGridPosition.y);

            // Check if the new position has already been visited
            if (visitedLocations.Contains(currentGridPosition))
            {
                // Penalize the agent for revisiting a location
                AddReward(-0.05f); // Adjust the penalty value as needed
                cumulativeReward += -0.05f;
            }
            else
            {
                // Reward for visiting a new location
                AddReward(0.2f); // Adjust the reward value as needed
                cumulativeReward += 0.2f;
                visitedLocations.Add(currentGridPosition); // Mark this location as visited
            }


            

            // Check if the agent has reached the goal
            if (currentGridPosition == mazeGenerator.finish)
            {
                SetReward(5.0f);  // Large reward for reaching the goal
                cumulativeReward += 5f;
                EndEpisode();
            }
        }
        else
        {
            // Penalize the agent for trying to move into a wall or out of bounds
            AddReward(-0.2f);
            cumulativeReward += -0.02f;
        }

        updateUI(actionDescription);
    }

    private void updateUI(string actionDescription)
    {
        rewardText.text = $"Reward: {cumulativeReward:F2}";
        actionText.text = $"Action: {actionDescription}";
    }

    bool IsValidMove(Vector2Int position)
    {
        // Ensure the move is within bounds and not into a wall
        return position.x >= 0 && position.x < mazeGenerator.gridSize &&
               position.y >= 0 && position.y < mazeGenerator.gridSize &&
               mazeGenerator.mazeGrid[position.x, position.y] == 0;
    }

    //allows us to control the agent
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 4; // Do Nothing

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 0; // Move up
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 1; // Move down
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 2; // Move left
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 3; // Move right
        }
    }


}
