using UnityEngine;
using System.Collections;
using System;   // this is a namespace declaration
                // so we can use the serializable attribute which will allow us to modify how variables appear in the editor
                // will be able to show/hide them using a foldout
using System.Collections.Generic;  // to be able to use lists
using Random = UnityEngine.Random; // tells random to use the unity engine random number generator

public class BoardManager : MonoBehaviour {

    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        // an assignment constructor that will allow us to set the minimum and maximum int values on a new declaration
        public Count (int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    // public variables

    // the dimensions of the game board, default being 8x8
    public int columns = 8;
    public int rows = 8;

    public Count wallCount = new Count(5, 9); // random number range for the number of walls to be spawned on each level, min 5, max 9
    public Count foodCount = new Count(1,5); // random number range for the number of food items to be spawned, from 1 to 5

    // variables to hold the references to prefabs we will use
    // arrays will be filled with prefabs designated in the inspector
    public GameObject exit;                 // the exit object
    public GameObject[] floorTiles;         // an array of the floor tiles
    public GameObject[] wallTiles;          // an array for the inner wall tiles
    public GameObject[] foodTiles;          // an array for the food objects
    public GameObject[] enemyTiles;         // an array for the enemies
    public GameObject[] outerWallTiles;     // an array for the outer walls along the border

    private Transform boardHolder;          // will be used to keep the hierarchy clean because we'll spawn a lot of game objects
                                            // we will child all the created objects to the boardHolder so the hierarchy is not filled with objects

    private List<Vector3> gridPositions = new List<Vector3>();      // will track all the positions on the board and if they are filled with an object or not

    // private function to initialize the game board position list
    void InitializeList()
    {
        gridPositions.Clear();      // first clear the list

        // populate the list
        // we are going from 1 to columns/rows - 1 to leave a border of empty spaces so that impassable levels are not created
        for (int x = 1; x < columns - 1; x++)
        {
            for (int y = 1; y < rows - 1; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));       // for each position, add a new entry to the list of positions
            }
        }
    }

    // private function to set up the outer wall and the floor of our game board
    void BoardSetup()
    {
        // going to set the boardHolder to be the transform of a gameObject called Board
        boardHolder = new GameObject("Board").transform;

        // lay out the floor and outer wall tiles for each grid spot
        // going from rows/columns -1 to the rows/columns + 1 because we are building a border around the edge of the active portion of the game board
        for (int x = -1; x < columns + 1; x++)
        {
            for (int y = -1; y < rows + 1; y++)
            {
                // for each of the grid spaces, choose a floor tile index at random from the array of floor tiles made in the inspector
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                // if this grid position is on the border, choose an index from the outerWallTiles array instead
                if(x == -1 || x == columns || y == -1 || y == rows)
                {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                }

                // create the game object by instantiating it at the chosen postion from the preceding step
                // using x and as the coordinates from the current step in our loop, and with z = 0 because we are working in 2D
                // we give Quaternion.identity so it is instantiated with no rotation, and we cast it to a GameObject (the "as" part)
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                // set the parent of this newly created GameObject as the boardHolder to keep the hierarchy neat
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    // function to choose a random position from the list of open grid positions, remove this randomly chosen one from the open positions,
    // and then return the position to be used for spawning GameObjects
    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);    // choose a random index from all the available grid positions(0=first, to Count = #items)
        Vector3 randomPosition = gridPositions[randomIndex];       // get the Vector3 at the randomly generated index from the array of grid positions
        gridPositions.RemoveAt(randomIndex);        // remove the grid position from the list so that 2 objects are not spawned at the same position
        return randomPosition;      // return the random position so we can use it to create a gameObject at this chosen empty spot
    }

    // function to spawn a tile at the random position we have chosen
    // these are the inner walls and objects, not the background or edge tiles
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);       // will control the number of objects to be spawned in a level, such as walls
        
        // spawn the number of objects specified by the objectCount
        for (int i = 0; i < objectCount; i++)
        {
            // choose a random position by calling the RandomPosition function
            Vector3 randomPosition = RandomPosition();
            // choose a random tile from our array of game objects
            GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
            // instantiate the tile at the random position with no rotation
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    // this will be called by the Game Manager when it's time to set up the board
    public void SetupScene(int level)
    {
        // create the floor and edge tiles
        BoardSetup();
        // prepare the inner active game grid positions
        InitializeList();
        // create the inner wall items
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
        // create the food tiles
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        // create a number of enemies based on the current level number using Mathf.Log to create a logarithmic difficulty progression
        // 1 at level 2, 2 at level 4, 3 at level 8, etc. increase enemies by 1 per double of level number
        int enemyCount = (int)Mathf.Log(level, 2f);     // cast the Mathf returned float to an integer
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        // instantiate the exit, which is always at the same space in the upper right, just inside the outer wall tiles
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0F), Quaternion.identity);
    }
    
}
