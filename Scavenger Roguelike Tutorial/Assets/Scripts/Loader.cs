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

    // change the position of the camera depending on the screen orientation
    private void Update()
    {        
        // if the screen is in a horizontal orientation, change the camera to this
        if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            Camera.main.transform.position = new Vector3(3.5f, 3.5f, -10f);
            Camera.main.orthographicSize = 5f;
        }
        else            // then the screen is in a vertical orientation, so the camera needs to have these settings
        {
            Camera.main.transform.position = new Vector3(3.5f, -0.4f, -10f);
            Camera.main.orthographicSize = 8.88f;
        }
    }

}
