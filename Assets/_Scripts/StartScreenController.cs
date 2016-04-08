using UnityEngine;
using System.Collections;

public class StartScreenController : MonoBehaviour{
	// This Gameobject is used for the popup button for disabled functions
	[SerializeField]
	GameObject
		Image;


	void Start (){
		// Since this Script is used in several scenes, a check if needed for the popup button image
		if (Application.loadedLevelName == "StartScreen")
			Image.gameObject.SetActive (false);
	}

	// This is going to be reused for every scene change
	public void button_Click (string scene){
		// If the Scene is NALCS, EULCS, LCK or LPL activate the disabled popup (features not implemented to-date)
		if (scene == "NALCS" 
			|| scene == "EULCS"
			|| scene == "LCK"
			|| scene == "LPL")
			Image.gameObject.SetActive (true);
		// Load the scene that was clicked on
		else
			Application.LoadLevel (scene);

	}

	// Disable popup by clicking on it
	public void close_PopUp (){
		Image.gameObject.SetActive (false);
	}
}
