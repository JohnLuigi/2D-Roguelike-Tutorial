using UnityEngine;
using System.Collections;
using System.Collections.Generic;       // declaring this namespace so we can use lists which we will use to keep track of our enemies

public class GameManager : MonoBehaviour {

    public float turnDelay = 0.1f;      // this is how long the game is going to wait between turns

    public static GameManager instance = null;      // make the GameManager a singleton
                                                    // declaring instances public means that it will be accessible outside the class
                                                    // declaring instances static means that the variable will be long to the class itself and not...
                                                    // ... just an instance of the class
                                                    // by making it public and static, this variable will be accessible from any script in the game

    public BoardManager boardScript;        // the reference to the Board Manager that will be used further below

    public int playerFoodPoints = 100;      // the food value that will decrease over time and be increased by picking up food objects
    [HideInInspector]
    public bool playersTurn = true;     // the [HideInInspector] makes the variable not appear in the inspecter in unity

    private int level = 3;  // start using 3 to test level 3 since that is the first level where enemies appear

    private List<Enemy> enemies;        // this list will keep track of our enemies and will be used ot send them their orders to move
    private bool enemiesMoving;     // used to determine if the enemies should be moving or not

	// Use this for initialization
    // use awake so that it runs before any other start scripts in the game
	void Awake () {

        // check for duplicates of this object, destroy any other than the very first one created
        if (instance == null)
            instance = this;        // set the very first instance to be null
        else if (instance != null)
            Destroy(gameObject);        // if any instance is not null aka an extra GameManager exists, destroy this game object

        DontDestroyOnLoad(gameObject);      // keep this object alive between scenes, also gameObject refers to the Game Object this script is attached to

        // initialize the list that the enemies will be added to
        enemies = new List<Enemy>();

        boardScript = GetComponent<BoardManager>();     // set the board manager script to be the one found in the scene
        InitGame();     // initializethe game using the function below
	}

    void InitGame()
    {
        enemies.Clear();        // clear the list of enemies when the game starts
                                // need to do this because the GameManager will not be reset when the level starts
                                // and we need to clear out any enemies from the last level

        boardScript.SetupScene(level);      // pass in the parameter level (using 3 for initial test) to setup the board using the
                                            // SetupScene function from the boardManager script
    }

    public void GameOver()
    {
        enabled = false;        // when the game ends, the GameManager is disabled
    }
	
	// Update is called once per frame
	void Update () {

        // check if it is the player's turn or if the enemies are already moving
        if (playersTurn || enemiesMoving)
            return;     // do nothing for this update loop since the player cannot move and the enemies already started their movement

        // if it is not the players turn and the enemies have not started moving yet, start the coroutine to move the enemies
        StartCoroutine(MoveEnemies());
	}

    // this takes a parameter of the type Enemy called script
    // will use this to have enemies register themselves with this GameManager so that this GameManager can issue move orders to them
    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);        // add the enemy object to the list of enemies
    }

    // coroutine to move our enemies one at our time, in sequence
    // will be called within the Update() function
    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;       // set the enemies to be able to move
        yield return new WaitForSeconds(turnDelay);     // wait for the turn delay, aka 0.1 seconds

        // check if no enemies have been spawned yet (aka the count of the list of enemies is 0), which would be the case in the first level
        if (enemies.Count == 0)
        {
            // if they have not been spawned yet, we are going to add an additional yield
            // to cause our player to wait, even though there is no enemy to wait for
            yield return new WaitForSeconds(turnDelay);
        }

        // for loop to loop through our enemies list
        for(int i = 0; i < enemies.Count; i++)
        {
            
            enemies[i].MoveEnemy();     // issue the move enemy command to each of the enemies in the list            
            yield return new WaitForSeconds(enemies[i].moveTime);       // wait after each enemy move using yield, using the move time saved for each of
                                                                        // the enemies
        }

        // now that all enemies have moved, let the player be able to move and the enemies not be able to move
        playersTurn = true;
        enemiesMoving = false;

    }

}
