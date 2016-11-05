using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

    // references to the two audio source objects attached to the SoundManager game object
    public AudioSource efxSource;
    public AudioSource musicSource;

    public static SoundManager instance = null;     // reference to the SoundManager object

    // these two ranges will add a bit of random variation to the pitch of our sound effects
    public float lowPitchRange = 0.95f;     // -5% of our original pitch
    public float highPitchRange = 1.05f;    // +5% of our original pitch

	// use the same singleton pattern used in GameManager
	void Awake ()
    {
        
        if (instance == null)           // if no other SoundManager has been instantiated...
            instance = this;            // make this the one Sound Manager to be used
        else if (instance != this)      // else, if another SoundManager has been set as the instance and it isn't this one...
            Destroy(gameObject);        // delete this gameobject since there is already another Sound Manager being used

        DontDestroyOnLoad(gameObject);  // set this one Sound Manager to not be destroyed since it will be used in every level

	}

    // this function will be used to play single audio clips
    // we are declaring this as a public function and using the parameter of type AudioClip so that we can call it from our other
    // scripts that are executing the actual game logic
    // audioclips are assets which contain digital audio recordings
    // in this case, our audio clips are our sound effects and our music loop 
    public void PlaySingle (AudioClip clip)
    {
        efxSource.clip = clip;      // set the effects source as the passed in audio clip
        efxSource.Play();       // play the clip in effects source
    }

    // example of using a comma separated list for an array as a parameter for a function

    // This will take in an array of audio clips as a parameter
    // The params keyword allows us to pass in a comma separated list of arguments of the same type, as specified by the parameter..
    // ..this will allow us to send any number of audio clips to this function, as long as we separate each one with a comma
    // This function creates some small amount of randomness to the audio being played by choosing clips randomly and adding a tiny bit of pitch change
    // It will prevent the clips that we will be playing very frequently from sounds too annoying
    public void RandomizeSfx (params AudioClip [] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);        // choose a random index from the passed in clips array to play
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);        // choose a random pitch from our preset range limits

        efxSource.pitch = randomPitch;      // set the pitch of the sound to be played as the randomly chosen pitch
        efxSource.clip = clips[randomIndex];    // set the clip to the index of one of the passed in clips in the array
        efxSource.Play();       // play the clip 
    }
    
}
