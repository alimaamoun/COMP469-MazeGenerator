using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject wallPrefab;
    public int gridSize = 8;
    public float wallProbability = 0.3f;

    private GameObject[,] grid;


    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();
    }

    //Create the maze with the selected amount of cells (8x8)
    void GenerateGrid()
    {
        for( int x = 0; x < gridSize; x++ )
        {
            for( int z = 0; z < gridSize; z++ ) {
                //place cell prefab in position of grid
                Vector3 position = new Vector3(x, 0, z);
                GameObject cell;
                if (Random.value < wallProbability)
                {
                    cell = Instantiate(wallPrefab, position +  new Vector3(0,.25f,0), Quaternion.identity);
                }
                else 
                {
                    cell = Instantiate(cellPrefab, position, Quaternion.identity);
                }
                
                //add cell copy position to grid
                //grid[x, z] = cell;

                

            }
        }
    }


}
