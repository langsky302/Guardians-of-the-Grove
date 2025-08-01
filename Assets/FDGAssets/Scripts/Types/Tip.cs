using UnityEngine;
using System;

namespace ForestDefenseGame.Types
{
	/// <summary>
	/// This script defines a level in the game. When the player kills all enemies, the level is increased and the difficulty is changed accordingly
	/// This class is used in the Game Controller script
	/// </summary>
	[Serializable]
	public class Tip
	{
		[Tooltip("The tooltip object that will be displayed for this tip")]
		public Transform tooltipObject;

		[Tooltip("The position of the tooltip object on the screen")]
		public Vector2 tooltipPosition = new Vector2(0,0);

		//[Tooltip("The speed of the enemies in this level")]
		//public float enemySpeed = 1;

		//[Tooltip("How quickly enemies are spawned in this level")]
		//public float spawnDelay = 1;
	}
}