using UnityEngine;
using System.Collections;

namespace ForestDefenseGame
{
	/// <summary>
	/// Plays a sound from an audio source.
	/// </summary>
	public class FDGPlaySound : MonoBehaviour
	{
		[Tooltip("The sound to play")]
		public AudioClip sound;

		[Tooltip("Should we play the sound when the game starts")]
		public bool playOnStart = true;

		[Tooltip("How long to wait before playing the sound. Make sure you set playOnStart to true if you want the sound to play")]
		public float playDelay = 0;
	
		[Tooltip("The tag of the sound source")]
		public string soundSourceTag = "GameController";

		[Tooltip("A random range for the pitch of the audio source, to make the sound more varied")]
		public Vector2 pitchRange = new Vector2( 0.9f, 1.1f);
	
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			if( playOnStart == true )    StartCoroutine("PlaySound");
		}
	
		/// <summary>
		/// Plays the sound
		/// </summary>
		IEnumerator PlaySound()
		{
			// If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
			if ( soundSourceTag != string.Empty && sound ) 
			{
				if ( playDelay > 0 )    yield return new WaitForSeconds(playDelay);

				// Give the sound a random pitch
				GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().pitch = Random.Range(pitchRange.x, pitchRange.y);

				// Play the sound
				GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(sound);
			}	
		}
	}
}