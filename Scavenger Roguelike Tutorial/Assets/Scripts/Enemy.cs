﻿using UnityEngine;
using System.Collections;

// set it to inherit from the Moving Object class by replacing the standard MonoBehaviour with MovingObject in the initial class declaration
// this will allow us to have the Enemy class make use of the movement code made in the MovingObject class and not have to duplicate code
public class Enemy : MovingObject {

    public int playerDamage;        // this is the number of food points that will be subtracted when the enemy attacks the player

    private Animator animator;      // the animator that will control the enemy
    private Transform target;       // will store the player's position here, the enemy will try to move towards this, aka the player
    private bool skipMove;          // this will cause the enemy to move every other turn as opposed to every turn

	// add protected override to use a different implementation than that of MovingObject
	protected override void Start () {

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

    // this is going to be called by the GameManager when it issues the order to move to each of the enemies in the Enemies list
    public void MoveEnemy()
    {
        // the directions for the enemies to move in
        int xDir = 0;
        int yDir = 0;

        // we will check the position of the player (target) against the current position of this enemy and figure out which direction to move in
        // if the difference between the x coordinate of target's position and the x coordinate of the enemy's position is less than Epsilon (in this
        // case a number close to 0), aka if the player and enemy are in the same column
        if(Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
        {
            // if the enemy and player are in the same column, we are going to check if the y coordinate of the player is greater than the
            // y coordinate of the enemy's transform position
            // if so, we move up aka towards the player, and if not, we move down to the player
            yDir = target.position.y > transform.position.y ? 1 : -1;       // if this condition evluates to true, it sets the y direction to 1
                                                                            // if it evaluates to false, it sets the y direction to -1
        }
        // if the player and enemy are NOT in the same column, move towards the player's column
        else        
        {
            xDir = target.position.x > transform.position.x ? 1 : -1;       // if the player is to the right of the enemy, set the enemy x direction to 1
                                                                            // if the player is to the left of the enemy, set the x direction to -1
        }

        AttemptMove<Player>(xDir, yDir);        // try to move towards the player by passing in the Player as the parameter
    }

    // OnCantMove is called if the enemy attempts to move into a space occupied by the player
    // it overrides the OnCantMove function of MovingObject, which was implemented as abstract (aka only implemented by its child class)
    // it takes in the generic parameter T which we use to pass in the component we expect to encounter, which in this case is the player object
    protected override void OnCantMove <T> (T component)
    {
        Player hitPlayer = component as Player;     // this will be the passed in component which we will cast to a player class

        // call the lose food function from the Player class with the playerDamage from above to subtract the amount of food points from the player
        hitPlayer.LoseFood(playerDamage);
    }

}