using UnityEngine;
using System.Collections;
using UnityEngine.UI;       // this namespace declaration will allow us to modify the foodText Text UI object
using UnityEngine.SceneManagement;      // this will allow us to use the SceneManager to load a level instead of application.loadlevel

// This class will inherit from the MovingObject class instead of the default MonoBehaviour
// This is done by changing 'public class Player : MonoBehaviour' to 'public class Player : MovingObject'

public class Player : MovingObject {

    public int wallDamage = 1;      // wallDamage is the damage the player is going to apply to the wall when they chop a wall
    public int enemyDamage = 1;     // the damage the player does to an enemy, basically how many times they have to hit the enemy to kill it

    public int pointsPerFood = 10;      // these two values are going to be the number of points added to the player's score when they pick up these objects
    public int pointsPerSoda = 20;

    public float restartLevelDelay = 1f;

    public Text foodText;       // reference to the food text UI object

    // the various sound effects that are going to be played throughout the game
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;      // this will store a reference to our animator component
    private SpriteRenderer spriteRenderer;  // this will store a reference to the player's SpriteRenderer

    private int food;       // this will store the player's score during the level before passing it back to the GameManager as we change levels

    // We are disabling the warning for this variable since it is assigned, but only when code for touchscreens is being used.
    // We are mostly debugging in the unity editor, so the value never gets assigned and the warning shows up, so I'm disabling the warning for just this
    #pragma warning disable 0414
    private Vector2 touchOrigin = -Vector2.one;     // this will record where the player's finger started touching the touchscreen
                                                    // it is initialized to -Vector2.one which is a position off the screen
                                                    // this means that the conditional that is is going to check and see if there has been any..
                                                    // .. touch input will initally evaluate to false until there is actually a touch input to change..
                                                    // ..touchOrigin
    #pragma warning restore 0414

    // We add the keywords protected and override because we are going to have a different implementation for Start() in the player class than..
    // .. we have in the Moving Object class
    protected override void Start ()
    {
        animator = GetComponent<Animator>();        //get a reference to our Animator component
        spriteRenderer = GetComponent<SpriteRenderer>();    // set the reference to the SpriteRenderer component

        food = GameManager.instance.playerFoodPoints;       // set food to be the value stored in GameManager
                                                            // this is so that the Player script can manage the food score during a level
                                                            // and then store it in the GameManager as we change levels

        foodText.text = "Food: " + food;        // set the value of foodText to the current food score

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

        // example of coding for multiple platforms 
        // check the platform that it is running on, and react accordingly
        // if the game is being run as a standalone or as a webplayer, use the keyboard controls
        // change the next line to #if UNITY_STANDALONE || UNITY_WEBPLAYER to use it with a plugged in Unity Remote device (remove UNITY_EDITOR)
    #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

        // example of using arrows as a contrl scheme
        // get some input from the input manager, cast it form a float to an integer, and store it in our horizontal/vertical variable we declared
        // this will be input from the keyboard specifically
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");       // this will allow us to use this with keyboard and later on touch screen input

        // check if we are moving horizontally, and if so, set vertical to 0
        // this is to we are constrained to moving on a grid one direction at a time
        // prevents the player from moving diagonallys
        if (horizontal != 0)
            vertical = 0;
        // end of the keyboard-based code
    #else   // the code for other platforms, aka iPhone, android, windows phone
            // example of touch input code

        if (Input.touchCount > 0)       // if the input system has registered one or more touches,
        {
            Touch myTouch = Input.touches[0];       // store the first touch detected in the variable of type Touch in myTouch
                                                    // we are grabbing the first touch and ignoring all other touches in this case because the..
                                                    // .. game is oonly going to support a single finger swiping in the cardinal directions

            // check the phase of that first touch and make sure it is equal to Began so that we can
            // determine that this is the beginning of a touch on the screen
            if (myTouch.phase == TouchPhase.Began)      
            {
                touchOrigin = myTouch.position;     // if this is the beginning of a touch on the screen, set the touchOrigin Vector2 to be the position
                                                    // of this first touch
            }
            // since we initialzed touchOrigin to -1, we can now check if we had a touch that ended (meaning the finger lifted off the screen)
            // and if the touchOrigin.x is >= 0, meaning that it is inside the bound of the screen and has changed from the value we initialized it to when
            // we declared it and that it has ended
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;        // create a vector2 that has myTouch's position
                float x = touchEnd.x - touchOrigin.x;       // calculate the difference between the beginning and end of the touch on the x axis
                                                            // which will give us a direction to move in
                float y = touchEnd.y - touchOrigin.y;       // calculate the difference between the start and end of the touch to get the y direction
                touchOrigin.x = -1;     // set touchOrigin to -1 so the conditional does not repeatedly evaluate to true
                                        // this is off screen, so it cannot be linked to any other touches

                // now we have to determine if the user's touch swipe was generally more horizontal or vertical to choose one direction
                if (Mathf.Abs(x) > Mathf.Abs(y))        // if the x change in direction is greater than the y,
                    horizontal = x > 0 ? 1 : -1;        // if the change is positive(right), set horiontal to one, else the direction was left so make it -1
                else        // if the swipe was more vertical, aka the change in y was greater than the change in x,
                    vertical = y > 0 ? 1 : -1;      // set the vertical change to be up if the swipe difference is position, and down if negative
            }
        }

    #endif      // end of the mobile code

        // check if we have a non-zero value for horizontal or vertical
        // if we do, meaning we are attempting to move, call the AttemptMove function
        // TODO: Try different AttemptMove<T> with passing in enemies in addition to walls
        if (horizontal != 0 || vertical != 0)
        {
            
            if(horizontal == -1)                    // here, we want to check if the player is moving left into an object, and flip the sprite accordingly
            {
                spriteRenderer.flipX = true;        // flip the sprite to face left when moving left or breaking the wall              
            }
            else if (horizontal == 1)       // the player has changed to a right-wards movement, so keep the player facing right
            {
                spriteRenderer.flipX = false;       // keep the sprite facing right, which is the default flip(aka not flipped)
                
            }

            //TODO: change the vertical orientation here later

            AttemptMove<Wall>(horizontal, vertical);       // pass in the generic parameter wall, meaning it is a component the player can interact with
                                                           // also pass in the horizontal and vertical values which is going to be the direction the
                                                           // player is trying to move in
            
            //TODO: make it so AttemptMove only happens once, instead of calling it 2 times for different interaction types
            AttemptMove<Enemy>(horizontal, vertical);       // pass in the paramter enemy to see if the palyer tried to move into an enemy
                                                            // and therefore tried to attack it

        }

    }

    // we use procted and override because this attemptMove implementation is different than the original MovingObject Script
    protected override void AttemptMove <T> (int xDir, int yDir)
    {
        food--;     // every time the player moves, one food point is lost
        foodText.text = "Food: " + food;        // update the food score

        base.AttemptMove<T>(xDir, yDir);    // call AttemptMove from the base class

        RaycastHit2D hit;       // this will allow us to reference the result of the LineCast done in Move

        // check if move returns true, aka the player was able to move
        if(Move(xDir, yDir, out hit))
        {
            // example of using a comma separated list for an array as a parameter for a function
            // because we set up RandomizeSfx using the params keyword, we can input the clips as a comma separated list into the clips array
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);     // play one of the two move sounds randomly upon moving
        }

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
            foodText.text = "+" + pointsPerFood + " Food: " + food;     // show the picking up of food as an increase in food points
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);   // play one of the two eat sound effects randomly
            other.gameObject.SetActive(false);      // set the food object to be inactive after "picking it up"
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;      // add points to the food score
            foodText.text = "+" + pointsPerSoda + " Food: " + food;     // show the picking up of soda as an increase in food points
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);   // play one of the two drink sound effects randomly
            other.gameObject.SetActive(false);      // set the soda object to be inactive after using it
        }

    }

    // Player implementation for OnCantMove, which was declared in the MovingObject script without any implementation
    // we want the player to take an action if they are trying to move into a space where there is a wall and they are blocked by it
    // OR, the player can attack an enemy if they try to move into the enemy and thus deal damage to it (while also taking damage from the enemy themselves)
    protected override void OnCantMove <T> (T component)
    {
        if(typeof(T) == typeof(Wall))               // if the player tries to move into a wall, damage the wall
        {
            Wall hitWall = component as Wall;       // casting the component parameter to a Wall type
            hitWall.DamageWall(wallDamage);         // pass in the variable wallDamage for how much damage the player is going to do to the wall
            animator.SetTrigger("playerChop");      // set the playerChop trigger of our animator component we stored a reference to earlier
        }
        else if(typeof(T) == typeof(Enemy))         // if the player tries to move into an enemy, damage the enemy
        {
            Enemy hitEnemy = component as Enemy;    // casting the component paramater to Enemy type
            hitEnemy.DamageEnemy(enemyDamage);                 // damage the enemy fir enemyDamage amount
            animator.SetTrigger("playerChop");      // set the playerChop trigger of the Player object's animator
        }        
    }

    // reload the level if the player collides with the exit object, meaning that we are going to the next level
    private void Restart()
    {
        // the obsolete code from Unity <5.3 is:
        //Application.LoadLevel(Application.loadedLevel);
        // this reloads the current level

        // instead we use the new syntax:
        SceneManager.LoadScene(0);
    }

    // this is called when an enemy attacks the player, loss specifies how many points the player will lose
    public void LoseFood (int loss)
    {
        // first, set the playerHit trigger in our animator
        animator.SetTrigger("playerHit");
        food -= loss;       // take away from the player's food total
        foodText.text = "-" + loss + " Food: " + food;      // show the loss of food as a minus next to the food score
        CheckIfGameOver();      // check if the game is over because the player has lost food, so maybe the game has ended
    }

    // this simple function will check if our food score is less than or equal to zero
    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);        // play the game over sound once
            SoundManager.instance.musicSource.Stop();       // stop the looping music playing on our music source
            GameManager.instance.GameOver();        // if we reached the game ending condition, call GameOver from the GameManager            
        }
    }
}
