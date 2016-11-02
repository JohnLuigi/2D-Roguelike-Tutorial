using UnityEngine;
using System.Collections;

// This class will inherit from the MovingObject class instead of the default MonoBehaviour
// This is done by changing 'public class Player : MonoBehaviour' to 'public class Player : MovingObject'

public class Player : MovingObject {

    public int wallDamage = 1;      // wallDamage is the damage the player is going to apply to the wall when they chop a wall

    public int pointsPerFood = 10;      // these two values are going to be the number of points added to the player's score when they pick up these objects
    public int pointsPerSoda = 20;

    public float restartLevelDelay = 1f;
    private Animator animator;      // this will store a reference to our animator component

    private int food;       // this will store the player's score during the level before passing it back to the GameManager as we change levels

	// We add the keywords protected and override because we are going to have a different implementation for Start() in the player class than...
    // ... we have in the Moving Object class
	protected override void Start ()
    {
        animator = GetComponent<Animator>();        //get a reference to our Animator component

        food = GameManager.instance.playerFoodPoints;       // set food to be the value stored in GameManager
                                                            // this is so that the Player script can manage the food score during a level
                                                            // and then store it in the GameManager as we change levels
        base.Start();       // call the start function of the base class, MovingObject            
	}

    // declare a private function that returns void called OnDisable(). This function is part of the Unity API that will be called when the player
    // gameObject is disabled
    // we will use it to store the value of food in the GameManager as we change levels
    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;       // store the player's food value in GameManager
    }
	
	// Update is called once per frame
	void Update ()
    {
        // check if it is currently the player's turn
        if (!GameManager.instance.playersTurn)
            return;      // if it is not the players turn, the following code will not be executed

        // these two values will store the direction we are moving either as a 1 or a -1 along the horizontal and vertical axes
        int horizontal = 0;
        int vertical = 0;

        // get some input from the input manager, cast it form a float to an integer, and store it in our horizontal/vertical variable we declared
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");       // this will allow us to use this with keyboard and later on touch screen input

        // check if we are moving horizontally, and if so, set vertical to 0
        // this is to we are constrained to moving on a grid one direction at a time
        // prevents the player from moving diagonally
        if (horizontal != 0)
            vertical = 0;

        // check if we have a non-zero value for horizontal or vertical
        // if we do, meaning we are attempting to move, call the AttemptMove function
        if (horizontal != 0 || vertical != 0)
            AttemptMove<Wall> (horizontal, vertical);       // pass in the generic parameter wall, meaning it is a component the player can interact with
                                                            // also pass in the horizontal and vertical values which is going to be the direction the...
                                                            // ...player is trying to move in
	}

    // we use procted and override because this attemptMove implementation is different than the original MovingObject Script
    protected override void AttemptMove <T> (int xDir, int yDir)
    {
        food--;     // every time the player moves, one food point is lost

        base.AttemptMove<T>(xDir, yDir);    // call AttemptMove from the base class

        RaycastHit2D hit;       // this will allow us to reference the result of the LineCast done in Move

        // since the player has lost food points by moving, we are going to check if the game has ended
        CheckIfGameOver();

        GameManager.instance.playersTurn = false;       // end the players turn after they attempt to move
    }

    // give the player the ability to interact with the other objects on the board, namely the food, soda, and exit objects
    // by using the Unity API OnTriggerEnter2D function and making use of the IsTrigger collider property on the food, soda, and exit prefabs
    private void OnTriggerEnter2D (Collider2D other)
    {
        // check the tag of the other object we collided with, invoke the declared Restart() function
        if (other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);       // include the delay so that we can call that function one second after the trigger
                                                        // causing a 1 second pause, then the restart of the level
            enabled = false;        // since the level is over, set the player to not be enabled
        }
        else if (other.tag == "Food")       // if the collided object is food:
        {
            food += pointsPerFood;      // add food points to the food score
            other.gameObject.SetActive(false);      // set the food object to be inactive after "picking it up"
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;      // add points to the food score
            other.gameObject.SetActive(false);      // set the soda object to be inactive after using it
        }

    }

    // Player implementation for OnCantMove, which was declared in the MovingObject script without any implementation
    // we want the player to take an action if they are trying to move into a space where there is a wall and they are blocked by it
    protected override void OnCantMove <T> (T component)
    {
        Wall hitWall = component as Wall;       // casting the component parameter to a Wall type
        hitWall.DamageWall(wallDamage);     // pass in the variable wallDamage for how much damage the player is going to do to the wall
        animator.SetTrigger("playerChop");      // set the playerChop trigger of our animator component we stored a reference to earlier
    }

    // reload the level if the player collides with the exit object, meaning that we are going to the next level
    private void Restart()
    {
        // the obsolete code from Unity <5.3 is:
        Application.LoadLevel(Application.loadedLevel);
        // this reloads the current level

        // instead we use the new syntax:
        //SceneManager.LoadScene(0);
    }

    // this is called when an enemy attacks the player, loss specifies how many points the player will lose
    public void LoseFood (int loss)
    {
        // first, set the playerHit trigger in our animator
        animator.SetTrigger("playerHit");
        food -= loss;       // take away from the player's food total
        CheckIfGameOver();      // check if the game is over because the player has lost food, so maybe the game has ended
    }

    // this simple function will check if our food score is less than or equal to zero
    private void CheckIfGameOver()
    {
        if (food <= 0)
            GameManager.instance.GameOver();        // if we reached the game ending condition, call GameOver from the GameManager
    }
}
