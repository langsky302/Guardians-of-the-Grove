#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ForestDefenseGame.Types;

namespace ForestDefenseGame
{
	/// <summary>
	/// This script controls the game, starting it, following game progress, and finishing it with game over.
	/// </summary>
	public class FDGGameController:MonoBehaviour 
	{
		[Header("<Player Options>")]
		[Tooltip("The player object, assigned from the scene")]
		public Transform playerObject;

		[Tooltip("How long to wait before the player can move again. Don't set this to 0, or you'll change lanes instantly resulting in movement from the first to the last lane, and back.")]
		public float moveDelay = 0.1f;
		internal float moveDelayCount = 0;

		// The player is moving now
		internal bool isMoving = false;

		[Header("<Level Options>")]
		[Tooltip("The X positions of the lanes in the game. All lanes are placed at (X,0,0)")]
		public float[] lanePositions;

		// These are the positions that enemies are spawned on. They are at the end of the lanes, opposite the player position
		internal float[] spawnPositions;
		
		[Tooltip("The index number of the current lane the player is in. At the start of the game the player is placed in this lane from the list of lane positions above")]
		public int currentLane;
		
		[Tooltip("The length of a lane. Enemies are spawned at the end of a lane, opposite the player")]
		public float laneLength;

		[Tooltip("A list of all the enemies/objects spawned in the game")]
		public Spawn[] spawns;
		internal Spawn[] spawnsList;

		[Tooltip("How may seconds to wait between enemy spawns")]
		public float spawnDelay = 1;
		internal float spawnDelayCount = 0;

		[Tooltip("A list of levels in the game, including the number of kills needed to win the game, the speed of enemies, and the spawn rate of enemies")]
		public Level[] levels;
		
		[Tooltip("The index number of the current level we are on")]
		public int currentLevel = 0;

		// How many enemies are left in this level, alive
		internal int enemyCount = 0;
		
		// How many enemies have been killed in this level
		internal int killCount = 0;

		// Has the boss been spawned in this level?
		internal bool bossSpawned = false;

		// The current boss in the level
		internal Transform currentBoss;

		[Tooltip("How many points we get when we hit an enemy. This bonus is multiplied by our kill streak")]
		public int hitTargetBonus = 10;
		
		// Counts the current streak. A streak will reset if the player shoots the wrong type of enemy, or shoots the end of the lane
		internal int streak = 1;
		
		[Tooltip("The bonus effect that shows how much bonus we got when we hit a target")]
		public Transform bonusEffect;
		
		[Tooltip("The score of the game. Score is earned by shooting enemies and getting streaks")]
		public float score = 0;
		internal float scoreCount = 0;
		
		[Tooltip("The text object that displays the score, assigned from the scene")]
		public Transform scoreText;
		internal float highScore = 0;
		internal float scoreMultiplier = 1;

		[Tooltip("The effect displayed before starting the game")]
		public Transform readyGoEffect;
		
		[Tooltip("How long to wait before starting gameplay. In this time we usually display the readyGoEffect")]
		public float startDelay = 1;

		[Tooltip("The overall game speed. This affects the entire game (Time.timeScale)")]
		public float gameSpeed = 1;

		// Is the game over?
		internal bool  isGameOver = false;

		[Header("<Mobile Controls>")]
		[Tooltip("This canvas assigned from the scene has the shooting buttons in it for mobile devices")]
		public Transform mobileShootButtons;
		
		[Tooltip("This canvas assigned from the scene has the move buttons in it for mobile devices")]
		public Transform mobileMoveButtons;
		
		[Tooltip("Use swipe controls instead of the move buttons on screen")]
		public bool useSwipeControls = false;
		
		//The start and end positions of the touches, when using swipe controls
		internal Vector3 swipeStart;
		internal Vector3 swipeEnd;
		
		[Tooltip("The swipe distance; How far we need to swipe before detecting movement")]
		public float swipeDistance = 10;
		
		[Tooltip("How long to wait before the swipe command is cancelled")]
		public float swipeTimeout = 1;
		internal float swipeTimeoutCount;

		[Header("<Keyboard and Gamepad Controls>")]
		[Tooltip("The button that moves the player left/right. This is defined in the Input Manager")]
		public string moveButton = "Horizontal";
		
		[Tooltip("A list of buttons that shoot. Each button index number corresponds to the shot type in the player script")]
		public string[] shootButtons;
		
		[Tooltip("The level of the main menu that can be loaded after the game ends")]
		public string mainMenuLevelName = "CS_StartMenu";
		
		[Tooltip("The keyboard/gamepad button that will restart the game after game over")]
		public string confirmButton = "Submit";
		
		[Tooltip("The keyboard/gamepad button that pauses the game")]
		public string pauseButton = "Cancel";
		internal bool  isPaused = false;

		[Header("<User Interface>")]
		[Tooltip("Various canvases for the UI, assign them from the scene")]
		public Transform gameCanvas;
		public Transform progressCanvas;
		public Transform pauseCanvas;
		public Transform gameOverCanvas;
		public Transform levelUpCanvas;
		public Transform transitionCanvas;

		[Header("<Sound Options>")]
		[Tooltip("")]
		// Various sounds and their source
		public AudioClip soundLevelUp;
		public AudioClip soundGameOver;
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;

		[Header("<Gameplay Tips>")]
		[Tooltip("The tip you get when starting the game")]
		public Transform startTip;

		[Tooltip("The tip you get when you earn a bonus streak")]
		public Transform streakTip;

		[Tooltip("The tip you get when shooting the wrong enemy")]
		public Transform missTip;

		[Tooltip("The tip you get when an enemy passes your line of defense")]
		public Transform loseTip;

		[Tooltip("Should we show tips during gameplay? Tips are shown only once when playing the game, unless you reset them")]
		public bool showTips = true;

		[Tooltip("The PlayerPrefs record of the state of the tips, showing or hiding them. This should match the PlayerPrefs in the ShowTips button in the pause menu")]
		public string showTipsPlayerPrefs = "ShowTips";

		// A general use index
		internal int index = 0;
		internal int indexB = 0;
		internal int indexSpawn = 0;

		void Awake()
		{
			// Activate the pause canvas early on, so it can detect info about sound volume state
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(true);
		}

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			// Check if we are not running on a mobile device. If so, remove the mobile shoot buttons.
			if ( Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android )    
			{
				// If mobile controls are assigned, hide them
				if ( mobileShootButtons )    mobileShootButtons.gameObject.SetActive(false);
				if ( mobileMoveButtons )    mobileMoveButtons.gameObject.SetActive(false);
			}

			// Get the current state of the tips, show/hide
			if ( PlayerPrefs.GetFloat(showTipsPlayerPrefs, 1) == 1 )    showTips = true;
			else    showTips = false;

			//Update the score and enemy count
			UpdateScore();
			UpdateKillCount();

			//Hide the game over and pause screens
			if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);

			//Get the highscore for the player
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			highScore = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "HighScore", 0);
			#else
			highScore = PlayerPrefs.GetFloat(Application.loadedLevelName + "HighScore", 0);
			#endif

//CALCULATING SPAWN CHANCES
			// Calculate the chances for the objects to spawn
			int totalSpawns = 0;
			int totalSpawnsIndex = 0;
			
			// Calculate the total number of spawns with their chances
			for( index = 0; index < spawns.Length; index++)
			{
				totalSpawns += spawns[index].spawnChance;
			}
			
			// Create a new list of the objects that can be dropped
			spawnsList = new Spawn[totalSpawns];
			
			// Go through the list again and fill out each type of drop based on its drop chance
			for( index = 0; index < spawns.Length; index++)
			{
				int laneChanceCount = 0;
				
				while( laneChanceCount < spawns[index].spawnChance )
				{
					spawnsList[totalSpawnsIndex] = spawns[index];
					
					laneChanceCount++;
					
					totalSpawnsIndex++;
				}
			}

			// Copy the lane positions into the spawn positions
			spawnPositions = new float[lanePositions.Length];

			for( index = 0 ; index < lanePositions.Length ; index++)
			{
				spawnPositions[index] = lanePositions[index];
			}

			// Shuffle the list of lane positions
			spawnPositions = Shuffle(spawnPositions);

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

			// Check what level we are on
			UpdateLevel();

			// Create the ready?GO! effect
			if ( readyGoEffect )    Instantiate( readyGoEffect );

			// Place the player in the correct lane
			if ( playerObject )    playerObject.position = new Vector3(lanePositions[currentLane],0,0);

			moveDelayCount = moveDelay;
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void  Update()
		{
			// Count the move delay
			if ( moveDelayCount > 0 )    moveDelayCount -= Time.deltaTime;
			else if ( isMoving == true )    isMoving = false;

			// Make the score count up to its current value
			if ( score < scoreCount )
			{
				// Count up to the courrent value
				score = Mathf.Lerp( score, scoreCount, Time.deltaTime * 10);
				
				// Update the score text
				UpdateScore();
			}

			// Delay the start of the game
			if ( startDelay > 0 )
			{
				startDelay -= Time.deltaTime;
			}
			else
			{
				// Show the tip at the start of the game, explaining how to move and shoot
				ShowStartTip();

				//If the game is over, listen for the Restart and MainMenu buttons
				if ( isGameOver == true )
				{
					//The jump button restarts the game
					if ( Input.GetButtonDown(confirmButton) )
					{
						Restart();
					}
					
					//The pause button goes to the main menu
					if ( Input.GetButtonDown(pauseButton) )
					{
						MainMenu();
					}
				}
				else
				{
					//Toggle pause/unpause in the game
					if ( Input.GetButtonDown(pauseButton) )
					{
						if ( isPaused == true )    Unpause();
						else    Pause(true);
					}

					if ( isMoving == false )
					{
// Keyboard and Gamepad controls
						if ( Input.GetAxisRaw(moveButton) > 0.1f )
						{
							ChangeLane(1);
						}
						else if ( Input.GetAxisRaw(moveButton) < -0.1f )
						{
							ChangeLane(-1);
						}

//Using swipe controls to move the player, for mobile devices
						if ( useSwipeControls == true )
						{
							if ( swipeTimeoutCount > 0 )    swipeTimeoutCount -= Time.deltaTime;
								
							//Check touches on the screen
							foreach ( Touch touch in Input.touches )
							{
								//Check the touch position at the beginning
								if ( touch.phase == TouchPhase.Began )
								{
									swipeStart = touch.position;
									swipeEnd = touch.position;
									
									swipeTimeoutCount = swipeTimeout;
								}
								
								//Check swiping motion
								if ( touch.phase == TouchPhase.Moved )
								{
									swipeEnd = touch.position;
								}
								
								//Check the touch position at the end, and move the player accordingly
								if( touch.phase == TouchPhase.Ended && swipeTimeoutCount > 0 )
								{
									if( (swipeStart.x - swipeEnd.x) > swipeDistance ) //Swipe left
									{
										if ( currentLane > 0 )    currentLane--;

										// The player is moving now
										isMoving = true;
										
										// Reset the move delay counter
										moveDelayCount = moveDelay;
									}
									else if((swipeStart.x - swipeEnd.x) < -swipeDistance ) //Swipe right
									{
										if ( currentLane < lanePositions.Length - 1 )    currentLane++;

										// The player is moving now
										isMoving = true;
										
										// Reset the move delay counter
										moveDelayCount = moveDelay;
									}
								}
							}
						}
					}

					// Make sure the player object exists before giving it move and shoot commands
					if ( playerObject )
					{
						// Check keyboard/gamepad shoot buttons only if there are no active mobile shoot buttons
						if ( mobileShootButtons && mobileShootButtons.gameObject.activeSelf == true )
						{

						}
						else
						{
							// Go through all the possible shoot buttons, and check if we pressed them
							for ( indexB = 0 ; indexB < shootButtons.Length ; indexB++ )
							{	
								// If we press the shoot button, SHOOT!
								if ( Time.timeScale != 0 && !EventSystem.current.IsPointerOverGameObject() && Input.GetButtonDown(shootButtons[indexB]) )
								{
									playerObject.SendMessage("Shoot", indexB);
								}
							}
						}

						// Move the player to the correct lane
						if ( playerObject.position != Vector3.right * lanePositions[currentLane] )    playerObject.position = Vector3.MoveTowards( playerObject.position, Vector3.right * lanePositions[currentLane], Time.deltaTime * 5);
					}

					// If we haven't spawned all the enemies yet
					if ( enemyCount > 0 )
					{
						// Count down to the next object spawn
						if ( spawnDelayCount > 0 )    spawnDelayCount -= Time.deltaTime;
						else 
						{
							// Reset the spawn delay count
							spawnDelayCount = spawnDelay;

							// If we spawned an enemy on every lane, reshuffle the order in which enemies spawn on lanes
							if ( indexSpawn >= spawnPositions.Length )
							{
								indexSpawn = 0;
								
								// Shuffle the list of lane positions
								spawnPositions = Shuffle(spawnPositions);
							}

							// Spawn an enemy on the next lane
							SpawnObject(spawnsList);
							
							indexSpawn++;
							
							enemyCount--;
						}
					}
					else if ( levels[currentLevel].enemyBoss && bossSpawned == false )
					{
						// Spawn the enemy boss
						StartCoroutine("SpawnBoss");

						// The boss has been spawned
						bossSpawned = true;
					}

					// If the boss is killed, level up!
					if ( currentLevel < levels.Length - 1 && killCount >= levels[currentLevel].enemyCount + 1 && bossSpawned == true && currentBoss == null )    
					{
						LevelUp();
					}
				}
			}
		}

		/// <summary>
		/// Creates a new enemy at the end of a random lane 
		/// </summary>
		public void SpawnObject( Spawn[] currentSpawnList )
		{
			// Create a new random target from the target list
			Transform newSpawn = Instantiate( currentSpawnList[Mathf.FloorToInt(Random.Range(0,currentSpawnList.Length))].spawnObject ) as Transform;
			
			// Place the target at a random position along the throw height
			newSpawn.position = new Vector3( spawnPositions[indexSpawn], 0, laneLength);
			
			// Give the target a random initial rotation
			newSpawn.eulerAngles = Vector3.up * 180;
			
			// Set the speed of the spawned object
			newSpawn.SendMessage("SetSpeed", levels[currentLevel].enemySpeed);
		}
		
		/// <summary>
		/// Creates a new boss enemy at the end of the middle lane
		/// </summary>
		IEnumerator SpawnBoss()
		{
			// Create a new random target from the target list
			Transform newBoss = Instantiate(levels[currentLevel].enemyBoss) as Transform;

			// Assign the boss to a variable so we can check later if it was killed
			currentBoss = newBoss;

			// Disable the boss object until it's time to enable it
			newBoss.gameObject.SetActive(false);

			yield return new WaitForSeconds(levels[currentLevel].bossDelay);
			
			// Enable the boss object
			newBoss.gameObject.SetActive(true);
			
			// Place the target at a random position along the throw height
			newBoss.position = new Vector3( 0, 0, laneLength - 2);
			
			// Give the target a random initial rotation
			newBoss.eulerAngles = Vector3.up * 180;
			
			// Set the speed of the spawned object
			newBoss.SendMessage("SetSpeed", levels[currentLevel].enemySpeed);
		}

		/// <summary>
		/// Commands the player to move to another lane.
		/// </summary>
		/// <param name="changeValue">Change value, negative for left and positive for right</param>
		public void ChangeLane( int changeValue )
		{
			currentLane = Mathf.Clamp( currentLane + changeValue, 0, lanePositions.Length - 1);
			
			// The player is moving now
			isMoving = true;
			
			// Reset the move delay counter
			moveDelayCount = moveDelay;
		}
		
		/// <summary>
		/// Give a bonus when the target is hit. The bonus is multiplied by the number of targets on screen
		/// </summary>
		/// <param name="hitSource">The target that was hit</param>
		void HitBonus( Transform hitSource )
		{
			// If we have a bonus effect
			if ( bonusEffect )
			{
				// Create a new bonus effect at the hitSource position
				Transform newBonusEffect = Instantiate(bonusEffect, hitSource.position, Quaternion.identity) as Transform;

				// Display the bonus value multiplied by a streak
				newBonusEffect.Find("Text").GetComponent<Text>().text = "+" + (hitTargetBonus * streak).ToString();

				// Rotate the bonus text slightly
				newBonusEffect.eulerAngles = Vector3.forward * Random.Range(-10,10);
			}

			// Add the bonus to the score
			ChangeScore(hitTargetBonus * streak);
		}

		/// <summary>
		/// Changes the bonus streak.
		/// </summary>
		void ChangeStreak( int changeValue )
		{
			// If this is the first time we get a bonus streak, show a tip related to it
			if ( streak > 1 )    ShowStreakTip();

			streak += changeValue;
		}

		/// <summary>
		/// Resets the bonus streak.
		/// </summary>
		void ResetStreak()
		{
			ShowMissTip();

			streak = 1;
		}

		/// <summary>
		/// Change the score
		/// </summary>
		/// <param name="changeValue">Change value</param>
		void  ChangeScore( int changeValue )
		{
			scoreCount += changeValue;

			//Update the score
			UpdateScore();
		}
		
		/// <summary>
		/// Updates the score value and checks if we got to the next level
		/// </summary>
		void  UpdateScore()
		{
			//Update the score text
			if ( scoreText )    scoreText.GetComponent<Text>().text = Mathf.CeilToInt(score).ToString();
		}

		/// <summary>
		/// Set the score multiplier ( Get double score for hitting and destroying targets )
		/// </summary>
		void SetScoreMultiplier( int setValue )
		{
			// Set the score multiplier
			scoreMultiplier = setValue;
		}

		/// <summary>
		/// Changes the kill count for this level
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		void ChangeKillCount( int changeValue )
		{
			killCount += changeValue;

			UpdateKillCount();
		}

		/// <summary>
		/// Updates the kill count, and checking if we need to level up
		/// </summary>
		void UpdateKillCount()
		{
			// If all enemies are killed, level up!
			if ( currentLevel < levels.Length - 1 && killCount >= levels[currentLevel].enemyCount && levels[currentLevel].enemyBoss == null )    
			{
				LevelUp();
			}

			// Update the progress bar to show how far we are from the next level
			if ( progressCanvas )
			{
				progressCanvas.GetComponent<Image>().fillAmount = killCount * 1.0f/levels[currentLevel].enemyCount * 1.0f;
			}
		}
		
		/// <summary>
		/// Levels up, and increases the difficulty of the game
		/// </summary>
		void  LevelUp()
		{
			currentLevel++;

			// Now boss is spawned yet in the new level
			bossSpawned = false;

			// Reset the kill count
			killCount = 0;

			// Reset the spawn delay
			spawnDelay = 0;

			// Update the level attributes
			UpdateLevel();

			//Run the level up effect, displaying a sound
			LevelUpEffect();
		}

		/// <summary>
		/// Updates the level and sets some values like maximum targets, throw angle, and level text
		/// </summary>
		void UpdateLevel()
		{
			// Display the current level we are on
			if ( progressCanvas )    progressCanvas.Find("Text").GetComponent<Text>().text = (currentLevel + 1).ToString();

			// Set the enemy count based on the current level
			enemyCount = levels[currentLevel].enemyCount;

			// Change the speed of the game
			gameSpeed = levels[currentLevel].enemySpeed;

			// Change the spawn delay of the enemies
			spawnDelay = levels[currentLevel].spawnDelay;
		}

		/// <summary>
		/// Shows the effect associated with leveling up ( a sound and text bubble )
		/// </summary>
		void LevelUpEffect()
		{
			// If a level up effect exists, update it and play its animation
			if ( levelUpCanvas )
			{
				// Update the text of the level
				levelUpCanvas.Find("Text").GetComponent<Text>().text = levels[currentLevel].levelName;

				// Play the level up animation
				if ( levelUpCanvas.GetComponent<Animation>() )    levelUpCanvas.GetComponent<Animation>().Play();
			}

			//If there is a source and a sound, play it from the source
			if ( soundSource && soundLevelUp )    
			{
				soundSource.GetComponent<AudioSource>().pitch = 1;

				soundSource.GetComponent<AudioSource>().PlayOneShot(soundLevelUp);
			}
		}

		/// <summary>
		/// Shuffles the specified text list, and returns it
		/// </summary>
		/// <param name="texts">A list of texts</param>
		float[] Shuffle( float[] positions )
		{
			// Go through all the positions and shuffle them
			for ( index = 0 ; index < positions.Length ; index++ )
			{
				// Hold the text in a temporary variable
				float tempNumber = positions[index];
				
				// Choose a random index from the text list
				int randomIndex = UnityEngine.Random.Range( index, positions.Length);
				
				// Assign a random text from the list
				positions[index] = positions[randomIndex];
				
				// Assign the temporary text to the random question we chose
				positions[randomIndex] = tempNumber;
			}
			
			return positions;
		}

		/// <summary>
		/// Shows the tip at the start of the game, explaining how to move and shoot
		/// </summary>
		public void ShowStartTip()
		{
			if ( showTips == true && startTip && PlayerPrefs.GetInt("StartTip", 0) == 0 )
			{
				// Create the start tip canvas
				Instantiate(startTip);
				
				// Record that we showed the tip, so that we don't show it again next time we play
				PlayerPrefs.SetInt("StartTip", 1);
			}
		}
		
		/// <summary>
		/// "Streaks the tip". Haha, no that's just Monodevelop's automatic comment system :D
		/// It actually shows the tip related to the first time we get a bonus streak.
		/// </summary>
		public void ShowStreakTip()
		{
			// If we haven't seen the tip yet, pause the game and show it
			if ( showTips == true && streakTip && PlayerPrefs.GetInt("StreakTip", 0) == 0 )
			{
				// Create the start tip canvas
				Instantiate(streakTip);
				
				// Record that we showed the tip, so that we don't show it again next time we play
				PlayerPrefs.SetInt("StreakTip", 1);
			}
		}
		
		/// <summary>
		/// Shows the tip related to the first time we shoot the wrong enemy.
		/// </summary>
		public void ShowMissTip()
		{
			// If we haven't seen the tip yet, pause the game and show it
			if ( showTips == true && missTip && PlayerPrefs.GetInt("MissTip", 0) == 0 )
			{
				// Create the start tip canvas
				Instantiate(missTip);
				
				// Record that we showed the tip, so that we don't show it again next time we play
				PlayerPrefs.SetInt("MissTip", 1);
			}
		}
		
		/// <summary>
		/// Shows the tip related to the first time we shoot the wrong enemy.
		/// </summary>
		public void ShowLoseTip()
		{
			// If we haven't seen the tip yet, pause the game and show it
			if ( showTips == true && loseTip && PlayerPrefs.GetInt("LoseTip", 0) == 0 )
			{
				// Create the start tip canvas
				Instantiate(loseTip);
				
				// Record that we showed the tip, so that we don't show it again next time we play
				PlayerPrefs.SetInt("LoseTip", 1);
			}
		}
		
		/// <summary>
		/// Resets the gameplay tips, so that they show up again when playing
		/// </summary>
		public void ResetTips()
		{
			PlayerPrefs.DeleteKey("StartTip");
			PlayerPrefs.DeleteKey("StreakTip");
			PlayerPrefs.DeleteKey("MissTip");
			PlayerPrefs.DeleteKey("LoseTip");
		}
		
		/// <summary>
		/// Toggles the gameplay tips, showing or hiding them. If the tips are toggled to show they are also reset
		/// </summary>
		public void ToggleTips( int toggleState )
		{
			if ( toggleState == 1 && showTips == false )
			{
				showTips = true;
				
				ResetTips();
			}
			else
			{
				showTips = false;
			}
		}

		/// <summary>
		/// Pause the game, and shows the pause menu
		/// </summary>
		/// <param name="showMenu">If set to <c>true</c> show menu.</param>
		public void  Pause( bool showMenu )
		{
			isPaused = true;
			
			//Set timescale to 0, preventing anything from moving
			Time.timeScale = 0;
			
			//Show the pause screen and hide the game screen
			if ( showMenu == true )
			{
				if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(true);
				if ( gameCanvas )    gameCanvas.gameObject.SetActive(false);
			}
		}
		
		/// <summary>
		/// Resume the game
		/// </summary>
		public void  Unpause()
		{
			isPaused = false;
			
			//Set timescale back to the current game speed
			Time.timeScale = 1;
			
			//Hide the pause screen and show the game screen
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);
			if ( gameCanvas )    gameCanvas.gameObject.SetActive(true);
		}

		/// <summary>
		/// Runs the game over event and shows the game over screen
		/// </summary>
		IEnumerator GameOver(float delay)
		{
			isGameOver = true;

			ShowLoseTip();

			yield return new WaitForSeconds(delay);
			
			//Remove the pause and game screens
			if ( pauseCanvas )    Destroy(pauseCanvas.gameObject);
			if ( gameCanvas )    Destroy(gameCanvas.gameObject);
			
			//Show the game over screen
			if ( gameOverCanvas )    
			{
				//Show the game over screen
				gameOverCanvas.gameObject.SetActive(true);


				// Tính điểm tổng cộng có cộng thêm VIP NFT bonus
				float finalScore = score + PlayerDataManager.Instance.vipNFT * 10;

				////Write the score text
				//gameOverCanvas.Find("TextScore").GetComponent<Text>().text = "SCORE " + Mathf.RoundToInt(score).ToString();

				// Viết điểm số (score) lên màn hình
				gameOverCanvas.Find("TextScore").GetComponent<Text>().text = "SCORE " + Mathf.RoundToInt(finalScore).ToString();

				////Check if we got a high score
				//if ( score > highScore )    
				//{
				//	highScore = score;
					
				//	//Register the new high score
				//	#if UNITY_5_3 || UNITY_5_3_OR_NEWER
				//	PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", score);
				//	#else
				//	PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", score);
				//	#endif
				//}

				// Kiểm tra nếu đây là điểm cao nhất
				if (finalScore > highScore)
				{
					highScore = finalScore;

					// Ghi lại điểm cao mới
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
					PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", finalScore);
#else
    PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", finalScore);
#endif
				}

				//Write the high sscore text
				gameOverCanvas.Find("TextHighScore").GetComponent<Text>().text = "HIGH SCORE " + Mathf.RoundToInt(highScore).ToString();

				// ✅ Cộng finalScore vào tổng điểm toàn game
				int oldTotal = PlayerPrefs.GetInt("TotalScore", 0);
				int newTotal = oldTotal + Mathf.RoundToInt(finalScore);
				PlayerPrefs.SetInt("TotalScore", newTotal);
				PlayerPrefs.Save();  // Lưu dữ liệu lại

				//If there is a source and a sound, play it from the source
				if ( soundSource && soundGameOver )    
				{
					soundSource.GetComponent<AudioSource>().pitch = 1;
					
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);
				}
			}
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  Restart()
		{
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			#else
			Application.LoadLevel(Application.loadedLevelName);
			#endif
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  MainMenu()
		{
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(mainMenuLevelName);
			#else
			Application.LoadLevel(mainMenuLevelName);
			#endif
		}

		void OnDrawGizmos()
		{
			// Draws a white line showing a lane from start to end
			for ( index = 0 ; index < lanePositions.Length ; index++ )
			{
				Gizmos.DrawLine( new Vector3(lanePositions[index],0,0), new Vector3(lanePositions[index],0,laneLength));
			}
		}

        IEnumerator Freeze( float duration )
        {
            if (GameObject.FindObjectOfType<FDGEnemy>() ) GameObject.FindObjectOfType<FDGEnemy>().SendMessage("SetSpeedMultiplier", 0.2f);

            yield return new WaitForSeconds(duration);

            if (GameObject.FindObjectOfType<FDGEnemy>() ) GameObject.FindObjectOfType<FDGEnemy>().SendMessage("SetSpeedMultiplier", 1);
        }

        public void KillAll()
        {
            foreach( FDGEnemy enemy in GameObject.FindObjectsOfType<FDGEnemy>() )
            {
                enemy.SendMessage("Die");
            }
        }

        IEnumerator ShotKillsAll(float duration)
        {
            if (GameObject.FindObjectOfType<FDGPlayer>())
            {
                foreach (Transform shot in GameObject.FindObjectOfType<FDGPlayer>().shotObjects )
                {
                    if ( shot.GetComponent<FDGShot>() ) shot.GetComponent<FDGShot>().shotKillsAll = true;
                }
            }

            yield return new WaitForSeconds(duration);

            if (GameObject.FindObjectOfType<FDGPlayer>())
            {
                foreach (Transform shot in GameObject.FindObjectOfType<FDGPlayer>().shotObjects)
                {
                    if (shot.GetComponent<FDGShot>()) shot.GetComponent<FDGShot>().shotKillsAll = false;
                }
            }
        }
    }
}