using System.Collections;
using UnityEngine;
using ForestDefenseGame.Types;
using ForestDefenseGame;

namespace ForestDefenseGame
{
	/// <summary>
	/// This script defines an enemy, which spawn and moves along a lane. The enemy has health and can be killed with shot from the player. 
	/// If the enemy passes the player or touches it the game ends
	/// </summary>
	public class FDGItem : MonoBehaviour
	{
		internal Transform thisTransform;
		internal GameObject GameController;

		[Tooltip("The effect that is created at the location of this enemy when it dies")]
		public Transform pickupEffect;

		[Tooltip("The movement speed of the enemy. This is controlled through the Levels in the Game Controller")]
		public float moveSpeed = 1;

		[Tooltip("The tag of the object that this enemy can touch")]
		public string touchTargetTag = "Player";

		[Tooltip("A list of functions that run when this enemy touches the target")]
		public TouchFunction[] touchFunctions;

		[Tooltip("Various animation clips")]
		public AnimationClip spawnAnimation;
		public AnimationClip moveAnimation;

		[Tooltip("Various sounds that play when the enemy touches the target, or when it gets hurt")]
		public AudioClip soundHitTarget;
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;

		// The enemy is still spawning, it won't move until it finises spawning
		internal bool isSpawning = true;
	
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

			// Register the game controller for easier access
			GameController = GameObject.FindGameObjectWithTag("GameController");

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

			// Add all the needed animation clips if they are missing from the animation component.
            if ( GetComponent<Animation>() )
            {
                if (spawnAnimation && GetComponent<Animation>().GetClip(spawnAnimation.name) == null) GetComponent<Animation>().AddClip(spawnAnimation, spawnAnimation.name);
                if (moveAnimation && GetComponent<Animation>().GetClip(moveAnimation.name) == null) GetComponent<Animation>().AddClip(moveAnimation, moveAnimation.name);
            }

            // Play the spawn animation, and then retrun to the move animation
            StartCoroutine( PlayAnimation( spawnAnimation, moveAnimation));
		}

		void Update()
		{
			// Move the enemy based on its speed
			if ( isSpawning == false )    thisTransform.Translate( Vector3.forward * moveSpeed * Time.deltaTime, Space.Self );
		}

        public void EndSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// Is executed when this obstacle touches another object with a trigger collider
        /// </summary>
        /// <param name="other"><see cref="Collider"/></param>
        void OnTriggerEnter(Collider other)
		{	
			// Check if the object that was touched has the correct tag
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
						else if ( touchFunction.targetTag != string.Empty )    // Otherwise, apply the function on the target tag set in this touch function
						{
							// Run the function
							GameObject.FindGameObjectWithTag(touchFunction.targetTag).SendMessage(touchFunction.functionName, touchFunction.functionParameter);
						}
                        else if (touchFunction.targetTag != string.Empty)    // Otherwise, apply the function on the target tag set in this touch function
                        {
                            // Run the function
                            GameObject.FindGameObjectWithTag(touchFunction.targetTag).SendMessage(touchFunction.functionName, touchFunction.functionParameter);
                        }
                    }
				}

                // Remove the object
                Die();

                // If there is a sound source and audio to play, play the sound from the audio source
                if ( soundSource && soundHitTarget )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundHitTarget);
			}
		}

		/// <summary>
		/// Sets the speed of the enemy
		/// </summary>
		/// <param name="setValue">value of speed</param>
		void SetSpeed( float setValue )
		{
			// Change the movement speed of the enemy
			moveSpeed = setValue;

            // Set the animation according to the move speed
            if (GetComponent<Animation>()) GetComponent<Animation>()[moveAnimation.name].speed = 1 + moveSpeed * 0.2f;
            if (GetComponent<Animator>()) GetComponent<Animator>().SetFloat("SpeedMultiplier", 1 + moveSpeed * 0.2f);
		}
        

        /// <summary>
        /// Plays an animation and when it finishes it reutrns to a default animation
        /// </summary>
        /// <returns>The animation.</returns>
        /// <param name="firstAnimation">First animation.</param>
        /// <param name="defaultAnimation">Default animation to be played after first animation is done</param>
        IEnumerator PlayAnimation( AnimationClip firstAnimation, AnimationClip defaultAnimation )
		{
			if( GetComponent<Animation>() )
			{
				// If there is a spawn animation, play it
				if( firstAnimation )
				{
					// Stop the animation
					GetComponent<Animation>().Stop();
					
					// Play the animation
					GetComponent<Animation>().Play(firstAnimation.name);
				}
			
				// Wait for some time
				yield return new WaitForSeconds(firstAnimation.length);

				// If the spawning animation finished, we are no longer spawning and can start moving
				if ( isSpawning == true && firstAnimation == spawnAnimation )    isSpawning = false;

				// If there is a walk animation, play it
				if( defaultAnimation )
				{
					// Stop the animation
					GetComponent<Animation>().Stop();
					
					// Play the animation
					GetComponent<Animation>().CrossFade(defaultAnimation.name);
				}
			}
		}

        /// <summary>
		/// Changes the health of this enemy. If health reaches 0, it dies
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		IEnumerator ChangeHealth(float changeValue)
        {
            //health += changeValue;

            //// If health is low, show the low health effect
            //if (lowHealthEffect)
            //{
            //    if (health <= 0.2f * healthMax)
            //    {
            //        lowHealthEffect.gameObject.SetActive(true);
            //    }
            //}

            //// If health reaches 0, we die
            //if (health <= 0) Die();

            //// If the change in health is negative, then we get hurt
            //if (changeValue < 0)
            //{
            //    // If there is a hurt animation, play it
            //    if (GetComponent<Animation>() && hurtAnimation)
            //    {
            //        // Stop the animation
            //        //animation.Stop();

            //        // Play the animation
            //        GetComponent<Animation>().CrossFade(hurtAnimation.name);
            //    }

            //    // If there is a sound source and audio to play, play the sound from the audio source
            //    if (soundSource && soundHurt) soundSource.GetComponent<AudioSource>().PlayOneShot(soundHurt);

                // Wait for some time
                yield return new WaitForSeconds(0.1f);

            //    // If there is a walk animation, play it
            //    if (GetComponent<Animation>() && moveAnimation)
            //    {
            //        // Stop the animation
            //        //animation.Stop();

            //        // Play the animation
            //        GetComponent<Animation>().CrossFade(moveAnimation.name);
            //    }
            //}
        }

        /// <summary>
        /// Kills the enemy, and creates a death effect
        /// </summary>
        void Die()
        {
            // Create a death effect, if we have one assigned
            if (pickupEffect) Instantiate(pickupEffect, transform.position, Quaternion.identity);

            // Increase the kill count in the game controller
            GameObject.FindGameObjectWithTag("GameController").SendMessage("ChangeKillCount", 1);

            // Give hit bonus for this target
            GameObject.FindGameObjectWithTag("GameController").SendMessage("HitBonus", thisTransform);

            // Increase the bonus streak in the game controller
            GameObject.FindGameObjectWithTag("GameController").SendMessage("ChangeStreak", 1);

            // Remove the object from the game
            Destroy(gameObject);
        }
    }
}