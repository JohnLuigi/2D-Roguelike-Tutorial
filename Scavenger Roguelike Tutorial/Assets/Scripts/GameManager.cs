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

    [HideInInspector]
    public bool gameEnded = false;          // This will be used to check if the game is in the game over state after the player has died, and can either
                                            // close the game or start over from the beginning
    private Text yesText;       // These will store references to the yes or no at the end of the game to let the player choose to restart or not
    private Text noText;
    private float restartPromptDelay = 3f;   // the default delay for the try again prompt to appear after the survived message appears
    [HideInInspector]
    public bool yesChosen = true;       // The default is to have "Yes" to restart chosen, if it is false, then "No" has bene highlighted.

    [HideInInspector]
    public bool playerCanMove = true;   // this will allow us to stop the player from moving during the restart prompt screen
    [HideInInspector]
    public bool gamePaused = false;     // This boolean tracks if the game is currently paused or not.

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
        yesText = GameObject.Find("YesText").GetComponent<Text>();      // set the reference to the yes Text
        yesText.text = "";                                              // set the yes text to be blank so nothing is visible
        noText = GameObject.Find("NoText").GetComponent<Text>();        // set the reference to the no text
        noText.text = "";                                               // set the no text to be blank so nothing is visible
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
        playerCanMove = false;
        Invoke("RestartPrompt", restartPromptDelay);  // wait for seconds until the player can make any changes and either restart or quit
        gameEnded = true;   // set the tracker for the game's over status to true to allow the player to make a choice to end or not
    }
	
	// Update is called once per frame
	void Update () {

        if (gameEnded)       // if the game has ended, allow the player to choose to start over or close the game
        {
            // these two values will store the direction we are selecting, either as a 1 or a -1 along the horizontal and vertical axes
            int horizontal = 0;

            horizontal = (int)Input.GetAxisRaw("Horizontal");   // this will allow us to use this with keyboard and later on touch screen input

            // set whether Yes is chosen or not based on the keyboard input            
            if (horizontal == 1)     // if the player presed right, "No" was chosen in the restart menu
                yesChosen = false;
            else if (horizontal == -1)   // the player pressed left, choosing "Yes" to continue            
                yesChosen = true;

            // update the color of yes or no depending on which is chosen
            if (yesChosen)   // if yes is chosen, make the "No" text gray and the "Yes" text white
            {
                yesText.color = Color.white;
                noText.color = Color.gray;
            }
            else            // otherwhise, no is chosen so make "Yes" gray and "No" white
            {
                yesText.color = Color.gray;
                noText.color = Color.white;
            }

            // if the player presses enter in the restart prompt screen, check the state of yesChosen and then quit or restart
            if ( Input.GetKey("enter") || Input.GetKey("return") )
            {
                ResetOrQuit();
            }
        }       

        // TODO figure out how to have this code work when using unity remote without taking out "UNITY_EDITOR"
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        // If the player has pressed the escape key, the game shoudl pause or unpause accordingly
        if (Input.GetKeyUp("escape"))
        {
            PauseGame();           
        }
#endif

        // check if it is the player's turn or if the enemies are already moving or if the level is being initiated with the title card showing
        if (playersTurn || enemiesMoving || doingSetup)
            return;     // do nothing for this update loop since the player cannot move and the enemies already started their movement

        // if it is not the players turn and the enemies have not started moving yet, start the coroutine to move the enemies
        StartCoroutine(MoveEnemies());

        
    }

    // the steps to take in order to reset the game back to the initial starting conditions
    public void ResetGame()
    {
        yesChosen = true;           // reset the yesChosen to true so that on the next game over screen, yes is once again the default
        level = 0;                  // the level to start again on is 0
        playerFoodPoints = 100;     // reset the player's food(health) back to 100
        Player thePlayer = GameObject.Find("Player").GetComponent<Player>();
        thePlayer.ResetHealth();
        playerCanMove = true;       // allow the player to be able to move again
        gameEnded = false;      // this will allow the player to be able to pause and unpause the game again upon restarting
        SoundManager.instance.musicSource.Play();       // reenable the looping music playing on our music source
        SceneManager.LoadScene(0);  // reload the Main scene, in this case the only scene
    }

    // Reset the game or quit the application depending on the status of the yesChosen boolean
    public void ResetOrQuit()
    {
        if (yesChosen)           // restart the game
        {
            ResetGame();    // call the reset game function to reset the necessary values to start over from scratch
        }
        else                    // close the program
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    // code to run to either pause or unpause the game
    public void PauseGame()
    {
        if(gameEnded)
        {
            // do nothing if the game has ended, so that the player cannot pause or unpause the game while it is on the restart prompt screen
        }
        else if (gamePaused)      // if the player presses escape while the game is paused, resume the game
        {
            // reset the black background's opacity to 100%
            Image tempImage = levelImage.GetComponent<Image>();  // get the image component of the level image
            Color c = tempImage.color;          // set the temporary color to be that of levelImage's
            c.a = 1f;                         // make this color have 50% alhpa value (aka opacity)
            tempImage.color = c;                // set the levelImage's color to be this 50% opacity black
            levelImage.SetActive(false);        // hide the black background and thus also hide the paused text

            SoundManager.instance.musicSource.volume = 0.7f;    // reset the game's volume back to it's original level
            playerCanMove = true;
            gamePaused = false;         // set the game's status to be unpaused
            Time.timeScale = 1.0f;   // resume the game by setting the time scale back to 1
        }
        else                // the player pressed escape while the game is not paused, so resume the game
        {
            levelText.text = "PAUSED";      // set the text to be shown as PAUSED
                                            // set the black background image to 50% opacity
            yesText.text = "";      // hide the yes and no text during the pause screen so they can't be clicked on
            noText.text = "";
            Image tempImage = levelImage.GetComponent<Image>();  // get the image component of the level image
            Color c = tempImage.color;      // set the temporary color to be that of levelImage's
            c.a = 0.5f;                // make this color have 50% alhpa value (aka opacity)
            tempImage.color = c;            // set the levelImage's color to be this 50% opacity black
            levelImage.SetActive(true);     // make the black background visible

            SoundManager.instance.musicSource.volume = 0.20f;   // reduce the game's volume to 50% of it's defualt during a pause
            playerCanMove = false;
            gamePaused = true;      // set the game's status to become paused
            Time.timeScale = 0.0f;  // pause the game  by setting the timeScale to 0
        }
    }

    // This function will be called with a delay after the "you survived for X days" message appears.
    // It will make the "Try again?", Yes, and No option visible to the player.
    public void RestartPrompt()
    {   
        // set the three text objects to their new respective texts for the restart prompt
        levelText.text = "Try Again?";
        yesText.text = "Yes";
        noText.text = "No";
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
            if (enemies[i] != null)     // only try to move this enemy if it exists, aka don't try to move enemies that have been destroyed
            {
                enemies[i].MoveEnemy();     // issue the move enemy command to each of the enemies in the list            
                yield return new WaitForSeconds(enemies[i].moveTime);       // wait after each enemy move using yield, using the move time saved for each of
                                                                            // the enemies
            }

        }

        // now that all enemies have moved, let the player be able to move and the enemies not be able to move
        playersTurn = true;
        enemiesMoving = false;

    }

}
