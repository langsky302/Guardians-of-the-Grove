using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ForestDefenseGame.Types
{
	/// <summary>
	/// Toggles a toggle source when clicked on. It also records the toggle state (on/off) in a PlayerPrefs. 
	/// In order to detect clicks you need to attach this script to a UI Button and set the proper OnClick() event.
	/// </summary>
	public class FDGToggleState:MonoBehaviour
	{
		[Tooltip("The tag of the object in which a toggle function exists")]
		public string toggleObjectTag = "GameController";

		[Tooltip("The object in which a toggle function exists")]
		public Transform toggleObject;

		[Tooltip("The function we call when toggling this object")]
		public string toggleFunction = "ToggleTips";

		[Tooltip("The current state of the toggle; 0-off, 1-on")]
		public float currentState = 1;

		[Tooltip("The PlayerPrefs name of the state")]
		public string playerPref = "ShowTips";

		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// Awake is used to initialize any variables or game state before the game starts. Awake is called only once during the 
		/// lifetime of the script instance. Awake is called after all objects are initialized so you can safely speak to other 
		/// objects or query them using eg. GameObject.FindWithTag. Each GameObject's Awake is called in a random order between objects. 
		/// Because of this, you should use Awake to set up references between scripts, and use Start to pass any information back and forth. 
		/// Awake is always called before any Start functions. This allows you to order initialization of scripts. Awake can not act as a coroutine.
		/// </summary>
		void Awake()
		{
			if ( !toggleObject && toggleObjectTag != string.Empty )    toggleObject = GameObject.FindGameObjectWithTag(toggleObjectTag).transform;

			// Get the current state of the toggle from PlayerPrefs
			currentState = PlayerPrefs.GetFloat(playerPref, currentState);
		
			// Set the toggle in the toggle source
			SetState();
		}
	
		/// <summary>
		/// Sets the toggle volume
		/// </summary>
		void SetState()
		{
			if ( !toggleObject && toggleObjectTag != string.Empty )    toggleObject = GameObject.FindGameObjectWithTag(toggleObjectTag).transform;

			// Set the toggle in the PlayerPrefs
			PlayerPrefs.SetFloat(playerPref, currentState);

			Color newColor = GetComponent<Image>().material.color;

			// Update the graphics of the button image to fit the toggle state
			if( currentState == 1 )
				newColor.a = 1;
			else
				newColor.a = 0.5f;

			GetComponent<Image>().color = newColor;

		}
	
		/// <summary>
		/// Toggle the toggle. Cycle through all toggle modes and set the volume and icon accordingly
		/// </summary>
		public void ToggleState()
		{
			currentState = 1 - currentState;

			GameObject.FindGameObjectWithTag(toggleObjectTag).SendMessage(toggleFunction, currentState);
		
			SetState();
		}
	}
}