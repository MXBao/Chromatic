using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Depicts a gem and holds powerup logic, gem destruction and visuals
/// </summary>
public class Gem : MonoBehaviour {

	// the gem's colour and it's appropriate sprite
	public Sprite[] sprites;
	private Constants.GemColour gemColour;
	private Constants.GemType gemType;

	// whether the gem is in motion and it's home tile
	private bool moving;
	private Tile homeTile;

	// animation factors, how fast gems move/shrink on death/fast they disappear from the world
	private float moveSpeed = 5f;
	private float scaleFactor = 3.0f;
	private float deathTimeOut = 0.2f;

	// when the gem was last move, if the movement was vertical and whether it is rainbow gem
	// being used
	private float lastMoved = 0;
	private bool lastMoveVertical = true;
	private bool rainbowSpent;

	// particle references
	public GameObject[] powerUpParticles;
	public GameObject[] deathParticles;

	// references to the grid, the gem panel and the tiles
	private Grid grid;
	private Transform gemPanel;
	private Tile[,] map;

	/// <summary>
	/// Initialize references to grid, panel and tiles
	/// </summary>
	void Awake() {
		grid = GameObject.FindWithTag ("Grid").GetComponent<Grid>();
		gemPanel = GameObject.FindWithTag ("GemPanel").GetComponent<Transform> ();
		map = grid.GetTiles ();
	}

	/// <summary>
	/// Attempts to move moving gems
	/// </summary>
	void Update() {
		if (moving) {
			float distance = Vector3.Distance(GetComponent<RectTransform>().position, homeTile.GetComponent<RectTransform>().position);
			GetComponent<RectTransform>().position = Vector2.MoveTowards(GetComponent<RectTransform>().position, homeTile.GetComponent<RectTransform>().position, distance * moveSpeed * Time.deltaTime);
			if(distance == 0) {
				moving = false;
			}
		}
	}

	/// <summary>
	/// Move a gem to a specified tile, tracking of the time it was moved and whether it was a vertical movement
	/// </summary>
	/// <param name="t">t - destination tile</param>
	public void Move(Tile t) {
		homeTile = t;
		transform.SetParent (gemPanel.transform, true);
		transform.SetAsLastSibling ();
		transform.localScale = new Vector3 (1, 1, 1);
		moving = true;
		lastMoved = Time.time;
		float verticalMovement = Mathf.Abs(GetComponent<RectTransform>().position.y - t.GetComponent<RectTransform>().position.y);
		lastMoveVertical = verticalMovement > 0.5f;
	}

	/// <summary>
	/// Sets the gem colour.
	/// </summary>
	/// <param name="i">i - colour by index</param>
	public void SetGemColour(int i) {
		gemColour = (Constants.GemColour)i;
		GetComponent<Image> ().sprite = sprites[i];
	}

	/// <summary>
	/// Gets the gem colour.
	/// </summary>
	/// <returns>The gem colour.</returns>
	public Constants.GemColour GetGemColour() {
		return gemColour;
	}

	/// <summary>
	/// Sets the type of the gem.
	/// </summary>
	/// <param name="i">i - type by index.</param>
	public void SetGemType(int i) {
		gemType = (Constants.GemType)i;
		for(int j = 0; j < powerUpParticles.Length; j++) {
			if(j == i) {
				powerUpParticles[j].SetActive(true);
			} else {
				powerUpParticles[j].SetActive(false);
			}
		}
	}

	/// <summary>
	/// Gets the time when the gem was last moved.
	/// </summary>
	/// <returns>time the gem was last moved.</returns>
	public float GetLastMoved() {
		return lastMoved;
	}

	/// <summary>
	/// Was the last movement a vertical movement?
	/// </summary>
	/// <returns><c>true</c>, if last move vertical was wased, <c>false</c> otherwise.</returns>
	public bool WasLastMoveVertical() {
		return lastMoveVertical;
	}

	/// <summary>
	/// Gets the type of the gem.
	/// </summary>
	/// <returns>The gem type.</returns>
	public Constants.GemType GetGemType() {
		return gemType;
	}

	/// <summary>
	/// Kill the gem
	/// </summary>
	public void Die() {
		StartCoroutine(RemoveFromPlay ());
	}

	/// <summary>
	/// Removes the game from play, activating it's powerup if it has one and play it's death effect
	/// </summary>
	/// <returns>The removal</returns>
	IEnumerator RemoveFromPlay() {
		while (deathTimeOut > 0) {
			deathTimeOut -= Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
			transform.localScale = new Vector3(transform.localScale.x - Time.deltaTime * scaleFactor, transform.localScale.y - Time.deltaTime * scaleFactor, 1);
		}
		SpawnSpecialEffects ();
		TryActivatePowerUp ();
		Destroy (gameObject);

	}

	/// <summary>
	/// Spawns death special effects base on the gem type
	/// </summary>
	void SpawnSpecialEffects () {
		if (gemType == Constants.GemType.HorizontalClear) {
			Instantiate (deathParticles [(int)gemType],
			             new Vector3(deathParticles [(int)gemType].transform.position.x,
			            transform.position.y, 0), transform.rotation);
		} else if (gemType == Constants.GemType.VerticalClear) {
			Instantiate (deathParticles [(int)gemType],
			             new Vector3 (transform.position.x,
			             deathParticles [(int)gemType].transform.position.y, 0), transform.rotation);
		} else {
			Instantiate (deathParticles [(int)gemType], transform.position, transform.rotation);
		}
	}

	/// <summary>
	/// Tries the activate the gem's power up.
	/// </summary>
	void TryActivatePowerUp() {

		// lists of tiles to be affected by powerup destruction to prevent tiles from being counted then there
		// is an overlap in powers
		List<Tile> markedByPowerUps = new List<Tile> ();
		int tileXPos = (int)homeTile.position.x;
		int tileYPos = (int)homeTile.position.y;

		switch(gemType) {
		
		// horizontal clearing
		case Constants.GemType.HorizontalClear:
			for(int x = 0; x < Constants.GRID_WIDTH; x++) {
				Tile t = map[x, tileYPos];
				if(!markedByPowerUps.Contains(t)) {
					markedByPowerUps.Add(t);
				}
			}
			break;
		
		// vertical clearing
		case Constants.GemType.VerticalClear:
			for(int y = 0; y < Constants.GRID_HEIGHT; y++) {
				Tile t = map[tileXPos, y];
				if(!markedByPowerUps.Contains(t)) {
					markedByPowerUps.Add(t);
				}
			}
			break;
		
		// triple cross clear when horizontal/vertical and burst is mixed
		case Constants.GemType.TripleCrossClear:
			for (int x = tileXPos - 1; x <= tileXPos + 1; x++) {
				for(int y = 0; y < Constants.GRID_HEIGHT; y++) {
					if(x >= 0 && x < Constants.GRID_WIDTH
					   && y >= 0 && y < Constants.GRID_HEIGHT) {
						Tile t = map[x,y];
						if(!markedByPowerUps.Contains(t)) {
							markedByPowerUps.Add(t);
						}
					}
				}
			}

			for(int y = tileYPos - 1; y <= tileYPos + 1; y++) {
				for(int x = 0; x < Constants.GRID_WIDTH; x++) {
					if(x >= 0 && x < Constants.GRID_WIDTH
					   && y >= 0 && y < Constants.GRID_HEIGHT) {
						Tile t = map[x,y];
						if(!markedByPowerUps.Contains(t)) {
							markedByPowerUps.Add(t);
						}
					}
				}
			}
			break;
		
		// 3x3 destruction
		case Constants.GemType.Burst:
			for (int x = tileXPos - 1; x <= tileXPos + 1; x++) {
				for(int y = tileYPos - 1; y <= tileYPos + 1; y++) {
					if(x >= 0 && x < Constants.GRID_WIDTH
					   && y >= 0 && y < Constants.GRID_HEIGHT) {
						if(map[x,y] != null && !markedByPowerUps.Contains(map[x,y])) {
							markedByPowerUps.Add(map[x,y]);
						}
					}
				}
			}
			break;
		
		// 5x5 destruction when 2 bursts are mixed
		case Constants.GemType.BigBurst:
			for (int x = tileXPos - 2; x <= tileXPos + 2; x++) {
				for(int y = tileYPos - 2; y <= tileYPos + 2; y++) {
					if(x >= 0 && x < Constants.GRID_WIDTH
					   && y >= 0 && y < Constants.GRID_HEIGHT) {
						if(map[x,y] != null && !markedByPowerUps.Contains(map[x,y])) {
							markedByPowerUps.Add(map[x,y]);
						}
					}
				}
			}
			break;
		
		// rainbow gem, converts and destroy!
		case Constants.GemType.Rainbow:

			// if mixed with normal gem, mark those of the same colour
			int[] tileColours = new int[Constants.NUM_COLOURS];
			for(int x = 0; x < Constants.GRID_WIDTH; x++) {
				for(int y = 0; y < Constants.GRID_HEIGHT; y++) {
					Tile t = map[x,y];
					if(t != null && t.GetGem() != null && !markedByPowerUps.Contains(t)) {
						if(t.GetGem().GetGemColour() != Constants.GemColour.Rainbow) {
							tileColours[(int)t.GetGem().GetGemColour()]++;
						}
					}
				}
			}

			// if destroyed by powerup, mark gems of the most abundant colour
			int mostAbundantColour = (int)Constants.GemColour.Red;
			int mostAbundantCount = 0;
			for(int i = 0; i < Constants.NUM_COLOURS; i++) {
				if(tileColours[i] > mostAbundantCount) {
					mostAbundantCount = tileColours[i];
					mostAbundantColour = i;
				}
			}

			for(int x = 0; x < Constants.GRID_WIDTH; x++) {
				for(int y = 0; y < Constants.GRID_HEIGHT; y++) {
					Tile t = map[x,y];
					if(t != null && t.GetGem() != null && !markedByPowerUps.Contains(t)) {
						if((int)t.GetGem().GetGemColour() == mostAbundantColour) {
							markedByPowerUps.Add(t);
						}
					}
				}
			}
			break;
		}

		// clears the marked gems and count the number of gems destroyed by the powerup
		foreach(Tile t in markedByPowerUps) {
			if(t != null) {
				Gem tempGem = t.GetGem();
				if(tempGem != null) {
					grid.AddBrokenByPowerUpTile(tempGem.GetGemColour());
					t.ClearTile();
				}
			}
		}
	}
}
