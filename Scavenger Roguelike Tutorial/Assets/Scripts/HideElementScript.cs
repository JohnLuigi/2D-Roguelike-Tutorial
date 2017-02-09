using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideElementScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // disable this object if it is not supposed to appear when being run in the Unity editor, standalone, or webplayer
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        gameObject.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update () {

    
    }
}
