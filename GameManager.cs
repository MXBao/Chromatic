using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Handles gamestate and processes the different game phases and functions to carry them out
/// </summary>
public class GameManager : MonoBehaviour {

	// grid and tiles
	private Grid grid;
	private Image tileSelector;
	private Tile selectedTile;
	private Tile destinationTile;

	// cursor visuals
	public Texture2D battleCursor;
	public Texture2D waitCursor;
	public Texture2D matchCursor;
	private Vector2 cursorHotSpot = new Vector2(0, 0);

	// the selected target and selection circle
	[HideInInspector]
	public Enemy selectedTarget;
	public EnemySelectionCircle targetCircle;

	// the characters and the currently active one
	public GameObject[] characterPrefabs;
	[HideInInspector]
	public Character[] characters;
	[HideInInspector]
	public int currentCharacter;
	public Text characterNavigation;

	// party member positions and the display panels
	public Transform[] partyMemberPlaces;
	public RectTransform partyDisplay;
	public PartyMemberPanel[] partyMemberPanels;
	public RectTransform[] partyDisplayPositions;

	// action panel and skill buttons
	public GameObject actionPanel;
	public SkillButton[] skillButtons;
	public SkillDisplay skillDisplay;

	// wave display
	public Text waveCounter;
	private int waveNumer;

	// enemies, their position and places
	public GameObject[] enemyPrefabs;

	[HideInInspector]
	public List<Enemy> enemies;
	public Transform[] enemyPlaces;

	// counter of how many gems have been broken each round by colour
	private int[] gemsBroken;

	// the game's state
	private enum GameState {
		Waiting,
		// Gem falling/destruction
		Processing,
		// Skill selection
		Preparation,
		// Fighting
		Combat,
		// Gem movement
		Play,
		UndoSwap,
	}

	private GameState gameState;

	/// <summary>
	/// Initilize characters, enemies, the gems array, components, tiles and begin the first move
	/// </summary>
	void Start() {
		enemies = new List<Enemy> ();
		InitializeCharacters ();
		SpawnEnemies ();
		gemsBroken = new int[Constants.NUM_COLOURS];
		tileSelector = GameObject.FindWithTag ("TileSelector").GetComponent<Image>();
		grid = GameObject.FindWithTag ("Grid").GetComponent<Grid>();

		// Only here for testing purposes, should ideally be in a new Levels class eventually
		Constants.TileState[,] map = new Constants.TileState[Constants.GRID_WIDTH, Constants.GRID_HEIGHT]
		{
			{ Constants.TileState.Null, Constants.TileState.Normal, Constants.TileState.Null, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal },
			{ Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal },
			{ Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Null, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Null, Constants.TileState.Normal },
			{ Constants.TileState.Normal, Constants.TileState.Null, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal },
			{ Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Null, Constants.TileState.Normal, Constants.TileState.Null, Constants.TileState.Normal, Constants.TileState.Normal },
			{ Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal, Constants.TileState.Normal },
		};
		grid.SetGrid (map);
		StartCoroutine(ProcessMove());
	}

	/// <summary>
	/// Initializes the characters, what colours they can use depending on the skills they have
	/// and the related displays
	/// </summary>
	void InitializeCharacters() {
		characters = new Character[characterPrefabs.Length];
		for(int i = 0; i < characterPrefabs.Length; i++) {
			GameObject clone = Instantiate(characterPrefabs[i],
			                               partyMemberPlaces[i].position,
			                               partyMemberPlaces[i].rotation) as GameObject;
			Character c = clone.GetComponent<Character>();
			c.InitializeUsableColours();
			characters[i] = c;
			partyMemberPanels[i].SetCharacter(c);
			c.memberPanel = partyMemberPanels[i];
			c.skillDisplay = skillDisplay;
		}
	}

	/// <summary>
	/// Spawns a wave of enemies and increment the wave number
	/// </summary>
	void SpawnEnemies() {
		waveNumer++;
		waveCounter.text = "Wave " + waveNumer;
		foreach (Transform t in enemyPlaces) {
			GameObject clone = Instantiate(enemyPrefabs[0], t.position, t.rotation) as GameObject;
			Enemy e = clone.GetComponent<Enemy>();
			e.characters = characters;
			enemies.Add (e);
		}
		AssignEnemiesToCharacters();
	}

	/// <summary>
	/// Check the game state each frame
	/// </summary>
	void Update() {
		CheckGameState ();
	}

	/// <summary>
	/// Selects a target based on who is pointed at
	/// </summary>
	/// <param name="e">e - selected enemy</param>
	public void SelectTarget(Enemy e) {

		selectedTarget = e;

		/// move targetting circle or disable it if none is selected/selected is dead
		if (e != null) {
			targetCircle.SelectTarget(e);
		} else {
			selectedTarget = null;
			targetCircle.spriteRenderer.enabled = false;
		}
	}

	/// <summary>
	/// Checks the state of the game and call appropriate functions depending on phase
	/// </summary>
	void CheckGameState() {
		switch (gameState) {
			case GameState.Processing:
				Cursor.SetCursor(waitCursor, cursorHotSpot, CursorMode.Auto);
				StartCoroutine(ProcessMove());
				break;

			case GameState.Preparation:
				Cursor.SetCursor(battleCursor, cursorHotSpot, CursorMode.Auto);
				OpenPrepPhase();
				gameState = GameState.Waiting;
				break;

			case GameState.Combat:
				Cursor.SetCursor(waitCursor, cursorHotSpot, CursorMode.Auto);
				gemsBroken = new int[Constants.NUM_COLOURS];
				StartCoroutine(ProcessCombat());
				break;

			case GameState.Play:
				Cursor.SetCursor(matchCursor, cursorHotSpot, CursorMode.Auto);
				break;

			case GameState.UndoSwap:
				UndoSwap();
				gameState = GameState.Play;
				break;
		}
	}

	/// <summary>
	/// Processes gem destruction and count
	/// </summary>
	/// <returns>The move.</returns>
	IEnumerator ProcessMove() {

		// wait for gems to die
		gameState = GameState.Waiting;
		yield return new WaitForSeconds (0.3f);

		// calculate gems broken by cascades and powerups
		int[] gemsBrokenInCascade = grid.ProcessMatches();
		int[] gemsBrokenByPowerUps = grid.CalculatePowerUpBreaks();
		int tempGemsBroken = 0;
		for (int i = 0; i < gemsBrokenInCascade.Length; i++) {
			for(int j = 0; j < characters.Length; j++) {
				characters[j].AddColour(i, gemsBrokenInCascade[i]);
				characters[j].AddColour(i, gemsBrokenByPowerUps[i]);
			}
			gemsBroken[i] += gemsBrokenInCascade[i];
			gemsBroken[i] += gemsBrokenByPowerUps[i];
			tempGemsBroken += gemsBrokenInCascade[i];
			tempGemsBroken += gemsBrokenByPowerUps[i];
		}

		// if gems were broken, apply gravity and process that as a move again
		if (tempGemsBroken > 0) {
			yield return new WaitForSeconds (0.5f);
			grid.ProcessGravity (false);

			// move all gems by up to height x height times
			for(int i = 0; i < Constants.GRID_HEIGHT; i++) {

				// move all gems 
				for(int j = 0; j < Constants.GRID_HEIGHT; j++) {

					grid.ProcessGravity (false);

					// if the grid is not filled, spawn new gems
					if (!grid.GridFilled()) {
						for (int x = 0; x < Constants.GRID_WIDTH; x++) {
							Tile tile = grid.GetTiles () [x, Constants.GRID_HEIGHT - 1];
							if (tile.GetGem () == null) {
								grid.SpawnGem (tile, (int)Constants.GemType.Normal, true);
							}
						}
					}
					grid.ProcessGravity (false);
				}

				// repeat as above but take diagonal movements into account
				for(int j = 0; j < Constants.GRID_HEIGHT; j++) {
					grid.ProcessGravity (true);
					if (!grid.GridFilled()) {
						for (int x = 0; x < Constants.GRID_WIDTH; x++) {
							Tile tile = grid.GetTiles () [x, Constants.GRID_HEIGHT - 1];
							if (tile.GetGem () == null) {
								grid.SpawnGem (tile, (int)Constants.GemType.Normal, true);
							}
						}
					}
					grid.ProcessGravity (true);
				}
			}
			grid.ProcessGravity (false);

			// let the effects sink in then initiate another round
			yield return new WaitForSeconds (0.25f);
			StartCoroutine (ProcessMove ());

		// if no gems were (or eventually) broken, move on to the next appropriate phase
		} else {

			// check how many gems in total number of gems were broken through various processes
			int gemsMatched = 0;
			for(int i = 0; i < gemsBroken.Length; i++) {
				gemsMatched += gemsBroken[i];
			}

			// if absolutely no gems were broken i.e. invalid swap, undo the swap
			if(gemsMatched == 0 && selectedTile != null) {
				gameState = GameState.UndoSwap;

			// otherwise start prep phase
			} else {
				gameState = GameState.Preparation;
				selectedTile = null;
				destinationTile = null;
				gemsMatched = 0;
			}
		}
	}

	/// <summary>
	/// Handles tile selection and movement
	/// </summary>
	/// <param name="tile">tile - tile selected</param>
	public void SelectTile(Tile t) {

		// ignore tile selection if not in play phase
		if (gameState != GameState.Play) {
			return;
		}

		// ignore if selected tile is not valid or empty of gem
		if (t == null || t.GetGem() == null) {
			tileSelector.enabled = false;
			selectedTile = null;
			destinationTile = null;
			return;
		}

		// if there is no currently selected tile, select it
		if(selectedTile == null) {
			selectedTile = t;
			tileSelector.transform.SetParent(selectedTile.transform);
			tileSelector.transform.localScale = new Vector3 (1, 1, 1);
			tileSelector.GetComponent<RectTransform>().position = selectedTile.GetComponent<RectTransform>().position;
			tileSelector.enabled = true;
		
		// otherwise attempt to swap a secondary selection of it's not the same tile being selected and
		// it is an adjacent tile
		} else {
			if(t != selectedTile) {
				if(grid.AreNeighbors(selectedTile, t)) {
					gameState = GameState.Waiting;
					destinationTile = t;
					Swap(true);
					tileSelector.enabled = false;
				} else {
					selectedTile = t;
					tileSelector.transform.SetParent(selectedTile.transform);
					tileSelector.transform.localScale = new Vector3 (1, 1, 1);
					tileSelector.GetComponent<RectTransform>().position = selectedTile.GetComponent<RectTransform>().position;
					tileSelector.enabled = true;
				}
			}
		}
	}

	/// <summary>
	/// Undos a tile swap by reversing the swap
	/// </summary>
	void UndoSwap() {
		Tile tempTile = selectedTile;
		selectedTile = destinationTile;
		destinationTile = tempTile;
		Swap(false);
		selectedTile = null;
		destinationTile = null;
	}

	/// <summary>
	/// Attempts to swap the tiles content
	/// </summary>
	/// <param name="mixPowerUp">true - tries to mix the powerup of possible</param>
	void Swap(bool mixPowerUp) {
		Gem tempGem = selectedTile.GetGem ();
		selectedTile.SetGem(destinationTile.GetGem());
		if (selectedTile.GetGem () != null) {
			selectedTile.GetGem ().Move (selectedTile);
		}
		destinationTile.SetGem(tempGem);
		if (destinationTile.GetGem () != null) {
			destinationTile.GetGem ().Move (destinationTile);
		}

		// attempts to mix powerups and cause explosions if possible
		if (mixPowerUp) {
			grid.TryMixPowerUps (destinationTile, selectedTile);
		}

		// Move on to processing phase!
		gameState = GameState.Processing;
	}

	/// <summary>
	/// Opens the prep phase.
	/// </summary>
	void OpenPrepPhase() {
		actionPanel.SetActive (true);
		SwitchCharacter (true);
	}

	/// <summary>
	/// Switch towards the next character or previous character
	/// </summary>
	/// <param name="next">If set to <c>true</c> next.</param>
	public void IncrementCharacter(bool next) {
		if (next) {
			currentCharacter++;
		} else {
			currentCharacter--;
		}
		SwitchCharacter (false);
	}

	/// <summary>
	/// Handles character 'switching' and updates the panels appropriately
	/// </summary>
	/// <param name="reset">If set to <c>true</c> reset.</param>
	void SwitchCharacter(bool reset) {

		// if reset or out of bounds, default to first character
		if (reset || currentCharacter > characters.Length - 1) {
			currentCharacter = 0;
		} else if(currentCharacter < 0) {
			currentCharacter = characters.Length - 1;
		}

		// display the character's position i.e. if 3 characters, 1st character is 1/3, second is 2/3 etc
		characterNavigation.text = (currentCharacter + 1) + "/" + characters.Length;

		// move the party panel to center on the current character
		partyDisplay.position = partyDisplayPositions[currentCharacter].position;

		// display the current character's skills on the skill buttons
		for(int i = 0; i < skillButtons.Length; i++) {
			if(i < characters[currentCharacter].skills.Length) {
				skillButtons[i].ReplaceSkill(characters[currentCharacter], characters[currentCharacter].skills[i]);
			} else {
				skillButtons[i].ReplaceSkill(characters[currentCharacter], null);
			}
		}
	}

	/// <summary>
	/// Initiates the combat phase, disabling the skill selection aka actionpanel and defaulting the
	/// Party display offset
	/// </summary>
	public void BeginCombat() {
		actionPanel.SetActive (false);
		partyDisplay.position = partyDisplayPositions[0].position;
		gameState = GameState.Combat;
	}

	/// <summary>
	/// Processes the combat phase by executing the character's skills with an appropriate wait time,
	/// then the enemies in between, then initiate play state
	/// </summary>
	/// <returns>The combat.</returns>
	IEnumerator ProcessCombat() {
		gameState = GameState.Waiting;

		// Picks an enemy for each character and let them have at it
		PickEnemy ();
		for (int i = 0; i < characters.Length; i++) {
			for (int j = 0; j < characters[i].skills.Length; j++) {
				if(characters[i].skills[j].activationTier > -1) {
					float animDelay = characters[i].UseSkill(j) + Constants.BASE_ANIM_DELAY;
					yield return new WaitForSeconds(animDelay);
				}
			}
		}

		// Dispose dead enemies
		CleanUpEnemies ();

		// Allow surviving enemies to take their turn
		foreach (Enemy e in enemies) {
			if(e != null) {
				e.TakeTurn();
			}
		}

		// Dispose again for those who die due to poison
		CleanUpEnemies ();

		// Spawn new enemies if the wave has been cleared
		if (enemies.Count == 0) {
			SpawnEnemies();
		}

		// Advance to play state
		gameState = GameState.Play;
	}

	/// <summary>
	/// Sets the character's targets either randomly if one is not selected
	/// </summary>
	void PickEnemy() {
		foreach (Character c in characters) {
			if (selectedTarget == null) {
				c.target = enemies [Random.Range (0, enemies.Count)];
			} else {
				c.target = selectedTarget;
			}
		}
	}

	/// <summary>
	/// Cleans up dead enemies and reassigns the cleaned list to the characters
	/// </summary>
	void CleanUpEnemies() {
		int i = enemies.Count - 1;
		while (i >= 0) {
			if(enemies[i] == null || enemies[i] != null && enemies[i].dead) {
				enemies.RemoveAt(i);
			}
			i--;
		}
		AssignEnemiesToCharacters ();
	}

	/// <summary>
	/// Assigns the enemies to characters.
	/// </summary>
	void AssignEnemiesToCharacters() {
		foreach (Character c in characters) {
			c.enemies = enemies;
		}
	}
}
