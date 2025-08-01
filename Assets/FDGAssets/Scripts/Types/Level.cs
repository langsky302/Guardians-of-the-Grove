using UnityEngine;
using System;

namespace ForestDefenseGame.Types
{
	/// <summary>
	/// This script defines a level in the game. When the player kills all enemies, the level is increased and the difficulty is changed accordingly
	/// This class is used in the Game Controller script
	/// </summary>
	[Serializable]
	public class Level
	{
		[Tooltip("The name of the current level")]
		public string levelName = "LEVEL 1";

		[Tooltip("The number of kills needed to win this level")]
		public int enemyCount = 10;

		[Tooltip("The speed of the enemies in this level")]
		public float enemySpeed = 1;

		[Tooltip("How quickly enemies are spawned in this level")]
		public float spawnDelay = 1;

		[Tooltip("The boss enemy that appears at the end of the level. You can leave this empty if you don't want a boss in this level")]
		public Transform enemyBoss;

		[Tooltip("How many seconds to wait before spawning the boss. The delay count stars after the last enemy is spawned")]
		public float bossDelay = 2;
	}
}