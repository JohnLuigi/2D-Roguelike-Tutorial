using UnityEngine;
using System.Collections;

// This script checks if a Game Manager object has been instantiated and if it has not, then it instantiates a Game Object

public class Loader : MonoBehaviour {

    public GameObject gameManager;

	// check if there is any GameManager object created, if it has not been created, instantiate one
	void Awake ()
    {
        if (GameManager.instance == null) // here we are using the static variable that was created in GameManager.cs to check a variable globally
            Instantiate(gameManager);
	}

}
