using UnityEngine;
using System.Collections;

// set it to inherit from the Moving Object class by replacing the standard MonoBehaviour with MovingObject in the initial class declaration
// this will allow us to have the Enemy class make use of the movement code made in the MovingObject class and not have to duplicate code
public class Enemy : MovingObject {

    public int playerDamage;        // this is the number of food points that will be subtracted when the enemy attacks the player
                                    // enemy1 will do 10 damage, and enemy2 will do 20 damage
                                    // this will give us a stronger and a weaker enemy to keep things more interesting

    public int enemyHealth;         // this is the health that the enemy unit has. It will take the player enemyHealth number of hits to kill an enemy

    private Animator animator;      // the animator that will control the enemy
    private Transform target;       // will store the player's position here, the enemy will try to move towards this, aka the player
    private bool skipMove;          // this will cause the enemy to move every other turn as opposed to every turn

    // references to the two audio clips for the enemies to use when attacking the player
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

    // the two audio clips to be chosen from when a player attacks a wall
    // TODO: Make this more of a hitting flesh sound
    public AudioClip hitSound1;
    public AudioClip hitSound2;

    // sound to play when this enemy dies
    public AudioClip enemyDeathSound;

    // add protected override to use a different implementation than that of MovingObject
    protected override void Start () {

        GameManager.instance.AddEnemyToList(this);      // have this enemy add itself to the list of enemies in the GameManager
        animator = GetComponent<Animator>();        // get and store a component reference to our animator
        target = GameObject.FindGameObjectWithTag("Player").transform;      // store a reference of the player's location that is part of its transform
        base.Start();       // call the start function of hte base class, MovingObject

    }

    // enemy will have its own implementation of AttemptMove
    // in this case, we will be passing in the player component for the generic parameter T, because that is what we will expect the enemy
    // to be interacting with
    protected override void AttemptMove <T> (int xDir, int yDir)
    {
        // if it is the turn for the enemy to not move, aka to skip its move, make it false for the next turn
        if(skipMove)
        {
            skipMove = false;
            return;
        }

        base.AttemptMove<T>(xDir, yDir);        // attempt to move using the player as T with the x and y directions
        skipMove = true;        // since the enemy moved, set skip move to true so on the next turn the enemy cannot move
    }

    // Example of a character movement function
    // this is going to be called by the GameManager when it issues the order to move to each of the enemies in the Enemies list
    public void MoveEnemy()
    {
        // the directions for the enemies to move in
        int xDir = 0;
        int yDir = 0;

        // set a value to choose whether the enemy will try to move up or down first randomly
        int chooseDir = 0;
        // this random integer assignment will return 0 or 1
        chooseDir = Random.Range(0, 2);     // if it is 0, try vertical movement first, if 1, try horizontal movement first
                                            // without this, the enemies will only move horizontally and never attempt vertical movement
                                            // Can increase the range so that it's less likely for enemies to try to move, can make it so they only try
                                            // to move if chooseDir is 0 out of 4 or so

        // we will check the position of the player (target) against the current position of this enemy and figure out which direction to move in
        // if the difference between the x coordinate of target's position and the x coordinate of the enemy's position is less than Epsilon (in this
        // case a number close to 0), aka if the player and enemy are in the same column        
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)    // if they are in the same column
        {
            // if the enemy and player are in the same column, we are going to check if the y coordinate of the player is greater than the
            // y coordinate of the enemy's transform position
            // if so, we move up aka towards the player, and if not, we move down to the player
            yDir = target.position.y > transform.position.y ? 1 : -1;       // if this condition evluates to true, it sets the y direction to 1
                                                                            // if it evaluates to false, it sets the y direction to -1
        }
        // if the player and enemy are NOT in the same column, move towards the player's column
        else if(Mathf.Abs(target.position.y - transform.position.y) < float.Epsilon)    // if they are in the same row
        {
            xDir = target.position.x > transform.position.x ? 1 : -1;       // if the player is to the right of the enemy, set the enemy x direction to 1
                                                                            // if the player is to the left of the enemy, set the x direction to -1
        }
        else        // the enemy is diagonal to the player, so randomly choose to move up or down
        {
            // if this makes enemies too agressive, can add more options to chooseDir to make the enemies more or less likely to try to move
            if(chooseDir == 0)      // for now, 50% chance to move up or down, but will happen every turn
            {
                yDir = target.position.y > transform.position.y ? 1 : -1;   // move up if the player is above the enemy, down if player is below
            }
            else                    // add a condition here for other options of chooseDir to try to move or not
            {
                xDir = target.position.x > transform.position.x ? 1 : -1;   // move left if the player is to the left of the enemy, right if to the right
            }
        }

        // old implementation for direction choosing was to randomly try up or down first, but sometimes no movement occurred
        // new movement is more agressive and will require the player to use walls to block the enemies       

        AttemptMove<Player>(xDir, yDir);        // try to move towards the player by passing in the Player as the parameter
    }

    // example of setting an animation trigger upon a condition
    // OnCantMove is called if the enemy attempts to move into a space occupied by the player
    // it overrides the OnCantMove function of MovingObject, which was implemented as abstract (aka only implemented by its child class)
    // it takes in the generic parameter T which we use to pass in the component we expect to encounter, which in this case is the player object
    // TODO: Add a condition for passing in a wall that the enemy tries to move into but cannot
    protected override void OnCantMove <T> (T component)
    {
        // TODO: Add the check for the wall type here
        Player hitPlayer = component as Player;     // this will be the passed in component which we will cast to a player class

        // use setTrigger to set the enemy attack trigger in the animator controller when it collides with the player
        animator.SetTrigger("enemyAttack");

        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);     // randomly chose one one of the two sound effects and play it

        // call the lose food function from the Player class with the playerDamage from above to subtract the amount of food points from the player
        hitPlayer.LoseFood(playerDamage);
    }

    // deal damage to the enemy and reduce enemyHealth. If the enemy drops to zero or below, get rid of the enemy
    public void DamageEnemy(int damageTaken)
    {
        SoundManager.instance.RandomizeSfx(hitSound1, hitSound2);     // randomly choose and then play one of the two enemy being hit sound effects

        // TODO: Make a sprite for the enemy being hit
        // set the sprite of our spriteRenderer to our damaged sprite
        // this gives the visual feedback that they've successfully attacked the enemy
        // spriteRenderer.sprite = dmgSprite;

        enemyHealth -= damageTaken;     // deal the damage to the enemy by subtracting loss from the health of the wall

        // if the health is less than or equal to 0, disable this enemy
        if (enemyHealth <= 0)
        {
            SoundManager.instance.PlaySingle(enemyDeathSound);        // play the enemy dying sound once

            Destroy(gameObject);                             // destroy the enemy
        }   
    }

}
