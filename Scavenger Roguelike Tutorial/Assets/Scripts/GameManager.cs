using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;      // make the GameManager a singleton
                                                    // declaring instances public means that it will be accessible outside the class
                                                    // declaring instances static means that the variable will be long to the class itself and not...
                                                    // ... just an instance of the class
                                                    // by making it public and static, this variable will be accessible from any script in the game

    public BoardManager boardScript;        // the reference to the Board Manager that will be used further below

    private int level = 3;  // start using 3 to test level 3 since that is the first level where enemies appear

	// Use this for initialization
    // use awake so that it runs before any other start scripts in the game
	void Awake () {

        // check for duplicates of this object, destroy any other than the very first one created
        if (instance == null)
            instance = this;        // set the very first instance to be null
        else if (instance != null)
            Destroy(gameObject);        // if any instance is not null aka an extra GameManager exists, destroy this game object

        DontDestroyOnLoad(gameObject);      // keep this object alive between scenes, also gameObject refers to the Game Object this script is attached to

        boardScript = GetComponent<BoardManager>();     // set the board manager script to be the one found in the scene
        InitGame();     // initializethe game using the function below
	}

    void InitGame()
    {
        boardScript.SetupScene(level);      // pass in the parameter level (using 3 for initial test) to setup the board using the
                                            // SetupScene function from the boardManager script
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
