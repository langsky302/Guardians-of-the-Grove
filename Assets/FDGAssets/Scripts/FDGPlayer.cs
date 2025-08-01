using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ForestDefenseGame.Types;

namespace ForestDefenseGame
{
	public class FDGPlayer:MonoBehaviour 
	{
		internal Transform thisTransform;

		[Tooltip("The shot types that this player can shoot. These correspond to the shoot buttons defined in the game controller and in the UI buttons")]
		public Transform[] shotObjects;

		[Tooltip("The source from which shots are fired")]
		public Transform shotSource;

		[Tooltip("The muzzle effect when shooting")]
		public Transform shootEffect;

		[Tooltip("The speed of the shot")]
		public float shotSpeed = 10;

		[Tooltip("Your fire rate, or how often you can shoot")]
		public float shootRate = 0.2f;
		internal float shootRateCount = 0;

		// The player is dead now. When dead, the player can't move or shoot.
		internal bool isDead = false;

		[Tooltip("The effect that is created at the location of this object when it is destroyed")]
		public Transform deathEffect;

		[Tooltip("Various animation clips")]
		public AnimationClip spawnAnimation;
		public AnimationClip idleAnimation;
		public AnimationClip dieAnimation;

		[Tooltip("Various sounds and their source")]
		public AudioClip soundDie;
		public AudioClip soundShoot;

		[Tooltip("The source from which sounds are played")]
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;

		[Tooltip("A random range for the pitch of the audio source, to make the sound more varied")]
		public Vector2 pitchRange = new Vector2( 0.9f, 1.1f);

		// Use this for initialization
		void Start() 
		{
			thisTransform = transform;

            // Add all the needed animation clips if they are missing from the animation component.
            if ( GetComponent<Animation>() )
            {
                if (spawnAnimation && GetComponent<Animation>().GetClip(spawnAnimation.name) == null) GetComponent<Animation>().AddClip(spawnAnimation, spawnAnimation.name);
                if (idleAnimation && GetComponent<Animation>().GetClip(idleAnimation.name) == null) GetComponent<Animation>().AddClip(idleAnimation, idleAnimation.name);
                if (dieAnimation && GetComponent<Animation>().GetClip(dieAnimation.name) == null) GetComponent<Animation>().AddClip(dieAnimation, dieAnimation.name);
            }

            //Assign the sound source for easier access
            if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

		}
		
		void Update() 
		{
			// If no other animation is playing, play the idle animation
			if ( GetComponent<Animation>() && !GetComponent<Animation>().isPlaying && idleAnimation )
			{
				GetComponent<Animation>().Play(idleAnimation.name); 
			}

			// Count up the time to the next shot
			if ( shootRateCount < shootRate )
			{
				shootRateCount += Time.deltaTime;
			}
		}


		/// <summary>
		/// Shoot a bullet of a type. Each type of shot can only hit one enemy type
		/// </summary>
		public void Shoot( int shotType )
		{
			if ( isDead == false && shootRateCount >= shootRate )
			{
				shootRateCount = 0;

				// Create a new shot at the position of the mouse/tap
				Transform newShot = Instantiate( shotObjects[shotType] ) as Transform;

				// Set the position of the shot at the shot source object position, or else at the player position
				if ( shotSource )    newShot.position = shotSource.position;
				else    newShot.position = thisTransform.position;

				// Create a new muzzle effect
				Transform newEffect = Instantiate( shootEffect ) as Transform;

				// If we have a muzzle, place it at the muzzle position. Otherwise place it at the position of the shooter
				if ( newEffect )    newEffect.position = shotSource.position;
				else    newEffect.position = thisTransform.position;

				// Give the shot a velocity 
				newShot.GetComponent<Rigidbody>().velocity = new Vector3( 0, 0, shotSpeed);
				
				// Update the power we have left
				//UpdateAmmo();

				// If there is a sound source and audio to play, play the sound from the audio source
				if ( soundSource && soundShoot ) 
				{
					// Give the sound a random pitch
					soundSource.GetComponent<AudioSource>().pitch = Random.Range(pitchRange.x, pitchRange.y);

					// Play the sound
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundShoot);
				}
			}
		}

		/// <summary>
		/// Kills the object, and creates a death effect
		/// </summary>
		IEnumerator Die()
		{
			if ( isDead == false )
			{
				isDead = true;

				GameObject.FindGameObjectWithTag("GameController").SendMessage("GameOver", 1.5f);

				// If there is a sound source and audio to play, play the sound from the audio source
				if ( soundSource && soundDie )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundDie);

				if ( GetComponent<Animation>() && dieAnimation )
				{
					// Play the death animation
					GetComponent<Animation>().Play(dieAnimation.name);

					// Wait until the end of the animation
					yield return new WaitForSeconds(dieAnimation.length);

                    // Remove the player object
                    DestroyPlayer();
                }

                // If there is an Animator, play the "Die" state
                if (GetComponent<Animator>()) GetComponent<Animator>().Play("Die");

			}
		}

        public void DestroyPlayer()
        {
            // If there is a death effect, create it at the position of the player
            if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);

            // Remove the object from the game
            Destroy(gameObject);
        }
    }
}
