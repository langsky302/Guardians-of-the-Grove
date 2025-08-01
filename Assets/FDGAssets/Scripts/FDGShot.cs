using UnityEngine;
using System.Collections;

namespace ForestDefenseGame
{
	/// <summary>
	/// This script defines a shot that can hit a target
	/// </summary>
	public class FDGShot:MonoBehaviour 
	{
		internal Transform thisTransform;

        public bool shotKillsAll = false;

		[Tooltip("The target that the shot can hit")]
		public string targetTag = "Enemy1";

		[Tooltip("How much damage this shot does when it hits a target")]
		public float shotDamage = 5;

		[Tooltip("The animation that plays when the target is hit")]
		public AnimationClip hitAnimation;

		[Tooltip("The sound that plays when the target is hit")]
		public AudioClip soundHit;

		[Tooltip("The sound that plays when the target is missed (ex:when you hit an enemy of the wrong type)")]
		public AudioClip soundMiss;

		[Tooltip("The tag of the sound object")]
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;

		[Tooltip("The effect that is created at the location of this object when it is destroyed")]
		public Transform hitEffect;

		// Use this for initialization
		void Start()
		{
			thisTransform = transform;

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);
		}

		/// <summary>
		/// Raises the trigger enter2d event. Works only with 2D physics.
		/// </summary>
		/// <param name="other"> The other collider that this object touches</param>
		void OnTriggerEnter(Collider other) 
		{
			// Check if we hit the correct target
			if ( other.tag == targetTag || shotKillsAll == true ) 
			{
                // Change the health of the target
                other.SendMessage( "ChangeHealth", -shotDamage, SendMessageOptions.DontRequireReceiver);

				// Create a hit effect where the shot hit the target
				if( hitEffect )    Instantiate(hitEffect, thisTransform.position, Quaternion.identity);

				// If there is a sound source and a sound assigned, play it
				if( soundSource && soundHit )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundHit);

				Destroy(gameObject);
			}
			else
			{
				// Reset the streak bonus counter
				GameObject.FindGameObjectWithTag("GameController").SendMessage("ResetStreak");

				if( hitEffect )    
				{
					// Create a hit effect where the shot hit the target
					Transform newHitEffect = Instantiate( hitEffect, thisTransform.position, Quaternion.identity ) as Transform;

					//And attach it to the target to keep the effect aligned with it
					newHitEffect.parent = other.transform;
				}

				// If there is a sound source and a sound assigned, play it
				if( soundSource && soundMiss )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundMiss);

				Destroy(gameObject);
			}
		}
	}
}
