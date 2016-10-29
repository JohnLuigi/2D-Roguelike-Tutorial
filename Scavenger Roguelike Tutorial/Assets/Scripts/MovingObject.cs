using UnityEngine;
using System.Collections;

// making this class abstract enables us to make classes and class members that are incomplete and must be implemented in the derived class
public abstract class MovingObject : MonoBehaviour {

    public float moveTime = 0.1f;       // the time it will take our object to move in seconds
    public LayerMask blockingLayer;     // the layer on which we are going to check collision as we are moving to determine if a space is open to be moved
                                        // into

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;       // to be used to store a reference to the RigidBody2D component of the unit we are moving
    private float inverseMoveTime;      // will be used to make our movement calculations more efficient

	// we are making this proteted virtual be cause protected virtual functions can be overridden by their inherited classes
    // this is useful if we want one of these inherited classes to have a different implementation of start
	protected virtual void Start ()
    {
        // get the references of the attached components
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        
        inverseMoveTime = 1f / moveTime;        // set the inverse move time to the reciprocal of move time for easy use later
                                                // we can use it by multiplying instead of dividing which is more efficient computationally
	}

    // the out keyword allows arguments to be passed by reference
    // in this case we are using out to return more than one value from our Move function, will be returning a boolean and the RaycastHit2D hit
    protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;     // this will store the current transform position
                                                // transform.position is a Vector3, but by casting it to a Vector2, we can implicitly convert it, 
                                                // discarding the z axis data

        // we will use this to calculate the end position based on the direction parameters passed in when calling the Move function                                                
        Vector2 end = start + new Vector2(xDir, yDir);

        // disable the attached boxcollider 2d of this object  to make sure that when we are casting our ray, that we are not going to hit
        // our own collider
        boxCollider.enabled = false;

        // we cast a line from our start point to our endpoint checking collision on blockingLayer
        // we will store hte return value of this calculation in hit
        hit = Physics2D.Linecast (start, end , blockingLayer);

        // after the ray is cast, reenable the box collider
        boxCollider.enabled = true;

        // check if anything was hit by seeing if the hit's transform is null or not
        if(hit.transform == null)       // if the space we cast our line in is open and available to move into
        {
            StartCoroutine(SmoothMovement(end));    // start the SmoothMovement coroutine using end
            return true;        // return true to say that we were able to move
        }

        return false;       // if hit.transform == false, we return false to say that it was unsuccessful
    }    

    // declare our smooth movement coroutine
    // we wll use tis coroutine for moving units from one space to the next
    // the parameter end will specify where to move to
    protected IEnumerator SmoothMovement (Vector3 end)
    {
        // first we are going to calculate the remaining distance to move based on the square magnitude of the difference between the current position
        // and our end parameter
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;   // we are using square agnitude because it is computationally cheaper than
                                                                                // magnitude
                                                                                
        // this loop will check that our square remaining distance is greater than float.epsilon which we are going to use for a very small number 
        // aka almost 0
        while (sqrRemainingDistance > float.Epsilon)
        {
            // we will find a new position which is proportionally closer to the end based on the move time
            // Vector3.MoveTowards moves a point in a straight line from a starting point to an end point in an input amount of time
            // the newPosition Vector3 will be inverseMoveTime * Time.deltaTime units closer to the destination
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);     // use MovePosition to move the Rigid Body 2D to the new Position we have found

            // now we recalculate the remaining distance after we have moved
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;      // we will wait a frame before reevaluating the condition of the loop
        }

    }

    // the generic parameter T is used to specify the type of component we expect our unity to interact with if blocked
    // in the case of enemies, this is going to be a player
    // in the case of the player, this is going to be walls so that the player can attack and destroy the walls
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);       // try to move the object, so canMove is set to true if the move was successful, and false if failed
                                                        // because hit is an out parameter in Move, we can use hit to see if the transform we hit in move is
                                                        // null or not
        
        // if nothing was hit by our line cast in Move, we are going to return and not execute the following code
        if (hit.transform == null)
            return;

        // if something was hit, we are going to get a component reference to the component of type T attached to the object that was hit
        T hitComponent = hit.transform.GetComponent<T>();

        // if canMove is false and hitComponent is not null, meaning that MovingObject is blocked and has hit something that it can interact with,
        // call the OnCantMove function and pass it hitComponent as a parameter
        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);

        // the reason we are using a generic parameter is that the player and enemy will inherit from MovingObject so player is going to need to
        // be able to interact with walls and the enemy is going to need to be able to interact with the player
        // AKA we don't know in advance what type of hit component they are going to be interacting with
    }  


    // here, the abstract modifier indicates that the thing being modified has a missing or incomplete implementation
    // OnCantMove is going to be overriden by the functions in the inherting classes
    // because it is an abstract function, it has no opening or closing brackets
    protected abstract void OnCantMove <T> (T component)
        where T : Component;

}
