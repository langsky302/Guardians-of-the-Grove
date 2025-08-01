using System.Collections;
using UnityEngine;
using ForestDefenseGame.Types;
using ForestDefenseGame;

namespace ForestDefenseGame
{
	/// <summary>
	/// This script defines an object which can interact with the player in various ways. A object may be a rock or a wall that
	/// bounces the player back, or it can be an enemy that kills the player, or it can be a coin that can be collected.
	/// </summary>
	public class FDGTriggerFunction:MonoBehaviour
	{
		internal Transform thisTransform;

		[Tooltip("The tag of the object that can touch this object")]
		public string[] touchTargetTags;

		[Tooltip("An array of functions that run when this object is touched by the target")]
		public TouchFunction[] touchFunctions;
	
		[Tooltip("The sound that plays when this object is touched")]
		public AudioClip soundHit;
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;

	
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			thisTransform = transform;

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);
		}
	
		/// <summary>
		/// Is executed when this obstacle touches another object with a trigger collider
		/// </summary>
		/// <param name="other"><see cref="Collider"/></param>
		void OnTriggerEnter(Collider other)
		{	
			// Check if the object that was touched has the correct tag
			foreach ( string touchTargetTag in touchTargetTags )
			{
				if( other.tag == touchTargetTag )
				{
					// Go through the list of functions and runs them on the correct targets
					foreach( var touchFunction in touchFunctions )
					{
						// Check that we have a target tag and function name before running
						if( touchFunction.functionName != string.Empty )
						{
							// If the targetTag is "TouchTarget", it means that we apply the function on the object that ouched this lock
							if ( touchFunction.targetTag == "TouchTarget" )
							{
								// Run the function
								other.SendMessage(touchFunction.functionName, transform);
							}
							else if (touchFunction.targetTag == "SelfTarget")    // Otherwise, apply the function on the target tag set in this touch function
							{
								// Run the function
								gameObject.SendMessage(touchFunction.functionName, touchFunction.functionParameter);
							}
                            else if (touchFunction.targetTag != string.Empty)    // Otherwise, apply the function on the target tag set in this touch function
                            {
                                // Run the function
                                if (GameObject.FindGameObjectWithTag(touchFunction.targetTag)) GameObject.FindGameObjectWithTag(touchFunction.targetTag).SendMessage(touchFunction.functionName, touchFunction.functionParameter);
                            }
                        }
					}

					// If there is a sound source and audio to play, play the sound from the audio source
					if ( soundSource && soundHit )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundHit);
				}
			}
		}
	}
}