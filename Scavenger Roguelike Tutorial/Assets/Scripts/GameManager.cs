using UnityEngine;
using System.Collections;
using System.Collections.Generic;       // declaring this namespace so we can use lists which we will use to keep track of our enemies
using UnityEngine.UI;       // this will allow us to work with the UI elements on the canvas
using UnityEngine.SceneManagement;      // this will aloow us to use functions when levels are loaded and more

public class GameManager : MonoBehaviour {

    public float levelStartDelay = 2f;      // the time to wait before starting levels in seconds, aka 2 seconds

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

    private Text levelText;     // this will store a reference to the LevelText object in the object hierarchy. it will display the level text, aka Day 1
    private GameObject levelImage;      // this will store a reference to the black LevelImage so we can activate it and deactivate it
    private int level = 0;  // start using 3 to test level 3 since that is the first level where enemies appear
                            // but use level 0 for the proper starting level in the final product

    private List<Enemy> enemies;        // this list will keep track of our enemies and will be used ot send them their orders to move
    private bool enemiesMoving;     // used to determine if the enemies should be moving or not
    private bool doingSetup;        // this will check if we are setting up the board and prevent the palyer from moving during setup

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
        // no need to run initgame here because we will now call that on the OnLevelFinishedLoading function
        // test code
        //InitGame();     // initializethe game using the function below
	}

    // Example of performing actions when a scene is loaded, aka when a management script is enabled ot disabled
    // increment the level number when a new level is loaded
    // this is called each time a scene is loaded
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {        
        level++;        // one one to our level number
        InitGame();     // call InitGame to initialize our level
    }

    // called when this script is enabled
    void OnEnable()
    {
        // tell our OnLevelFinishedLoading function to start listening for a scene change event as soon as this script is enabled
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    // called when this script is disabled
    void OnDisable()
    {
        // tell our OnLevelWasFinishedLoading function to stop listening for a scene change event as soon as this script  is disabled
        // remember to always have an unsubscription for every delegate you subscribe to
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    // runs at the start of a level
    void InitGame()
    {
        doingSetup = true;      //the player won't be able to move while the title card is up, aka when the board is being made
        levelImage = GameObject.Find("LevelImage");     // set a reference to the black transition image
        levelText = GameObject.Find("LevelText").GetComponent<Text>();       // set the referene to the text object, which is a component of the
                                                                             // object in the hierarchy with the same name
        levelText.text = "Day " + level;        // set the level text to be the stored level number
        levelImage.SetActive(true);     // make the black image visible

        // example of using a funciton with a delay
        Invoke("HideLevelImage", levelStartDelay);      // use this method to run the function with a delay
                                                        // this function will allow us to display the title card (done above), then delay the hiding of the
                                                        // card for 2 seconds (done with this invoked function)

        enemies.Clear();        // clear the list of enemies when the game starts
                                // need to do this because the GameManager will not be reset when the level starts
                                // and we need to clear out any enemies from the last level

        boardScript.SetupScene(level);      // pass in the parameter level (using 3 for initial test) to setup the board using the
                                            // SetupScene function from the boardManager script
    }

    // called at the end of the level intialization to make the black image disappear
    // doing this as its own function so it can be invoked with a delay
    private void HideLevelImage()
    {
        levelImage.SetActive(false);        // disable the black level change image
        doingSetup = false;     // allow the player to move again
    }

    public void GameOver()
    {
        levelText.text = "After " + level + " days, you starved.";      // display text showing the lsat level as how long you survived
        levelImage.SetActive(true);     // enable the black background
        enabled = false;        // when the game ends, the GameManager is disabled
    }
	
	// Update is called once per frame
	void Update () {

        // check if it is the player's turn or if the enemies are already moving or if the level is being initiated with the title card showing
        if (playersTurn || enemiesMoving || doingSetup)
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

    // Example of a coroutine that will move characters and objects in a game
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
