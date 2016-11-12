using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

    public Sprite dmgSprite;        // this is the sprite we are going to display once the player has damaged a wall so they can see that
                                    // they're successfully attacked the wall
    public int hp = 4;      // the wall's hit points, aka the number of hits it will take the player to destroy this wall
    
    // the two audio clips to be chosen from when a player attacks a wall
    public AudioClip chopSound1;
    public AudioClip chopSound2;

    // references to this enemy's sprite renderer, it's box collider, and rigidbody2d
    private SpriteRenderer spriteRenderer;


    void Awake ()
    {

        // get and store a reference to our Sprite Renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall (int loss)
    {        
        if(hp == 1)             // if the wall will die in one hit, play a sound that will continue after it is destroyed
            AudioSource.PlayClipAtPoint(chopSound1, Camera.main.transform.position);     // play the chopping sound effect        
        else        // otherwise play the audio normally
            SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);     // randomly choose one of the chopping sound effects
        
        // set the sprite of our spriteRenderer to our damaged sprite
        // this gives the visual feedback that they've successfully attacked the wall
        spriteRenderer.sprite = dmgSprite;

        hp -= loss;     // deal the damage to the wall by subtracting loss from the health of the wall

        // if the health is less than or equal to 0, disable the wall
        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
