using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Holds an array of tiles to simulate a grid and associated functions including
/// gem spawning, match logic and powerup generation
/// </summary>
public class Grid : MonoBehaviour {

	public GameObject tilePrefab;
	public GameObject gemPrefab;
	private Tile[,] tiles;
	private List<Constants.GemColour> powerUpBreaks;
	private GameManager gameManager;

	void Start() {
		gameManager = GameObject.FindWithTag ("GameController").GetComponent<GameManager> ();
		powerUpBreaks = new List<Constants.GemColour> ();
	}

	/// <summary>
	/// Sets up the grid from a 2d array and initializes gem spawning
	/// </summary>
	/// <param name="g">The green component.</param>
	public void SetGrid(Constants.TileState[,] g) {
		tiles = new Tile[Constants.GRID_WIDTH,Constants.GRID_HEIGHT];
		for (int x = 0; x < Constants.GRID_WIDTH; x++) {
			for (int y = 0; y < Constants.GRID_HEIGHT; y++) {
				GameObject o = Instantiate(tilePrefab, transform.position,
				                           transform.rotation) as GameObject;
				Tile tile = o.GetComponent<Tile>();
				o.GetComponent<Button>().onClick.AddListener(delegate { gameManager.SelectTile(tile); });
				tile.transform.SetParent(transform, false);
				tile.SetTile(x, y, g[x, y]);
				tiles[x,y] = tile;
				if(g[x, y] != Constants.TileState.Null) {
					SpawnGem(tile, 0, false);
				}
			}
		}
	}

	/// <summary>
	/// Getter for the tiles
	/// </summary>
	/// <returns>The tiles.</returns>
	public Tile[,] GetTiles() {
		return tiles;
	}

	/// <summary>
	/// Checks whether two tiles are neighors to each other
	/// </summary>
	/// <returns><c>true</c>if tiles were neighbors <c>false</c> otherwise.</returns>
	/// <param name="s">s - source tile</param>
	/// <param name="d">d - destination tile</param>
	public bool AreNeighbors(Tile s, Tile d) {
		return Vector2.Distance(s.position, d.position) < 1.4f;
	}

	/// <summary>
	/// Spawns a gem and whether it has a powerup, offsets the y position if it's on the top row
	/// </summary>
	/// <returns>the gem spawned</returns>
	/// <param name="t">t - tile to be spawned in</param>
	/// <param name="p">p - power up index</param>
	/// <param name="offSet">If set to <c>true</c> off set y position</param>
	/// <param name="c">c - colour by index</param>
	public Gem SpawnGem(Tile t, int p, bool offSet, int c = -1) {
		GameObject go;
		if (offSet) {
			go = Instantiate (gemPrefab, t.GetComponent<RectTransform> ().position + Vector3.up * Constants.TILE_OFFSET, t.GetComponent<RectTransform> ().rotation) as GameObject;
		} else {
			go = Instantiate (gemPrefab, t.GetComponent<RectTransform> ().position, t.GetComponent<RectTransform> ().rotation) as GameObject;
		}
		Gem g = go.GetComponent<Gem>();

		// Set the gem's colour, random if not specified
		g.SetGemType(p);
		if (c < 0) {
			g.SetGemColour (Random.Range (0, Constants.SPAWNED_COLOURS));
		} else {
			g.SetGemColour (c);
		}

		// move the gem into the tile and marry them
		g.Move (t);
		t.SetGem (g);
		return g;
	}

	/// <summary>
	/// Checks for matches
	/// </summary>
	/// <returns>array of gems broken broken by colour</returns>
	public int[] ProcessMatches() {
		int[] gemsBroken = new int[Constants.NUM_COLOURS];
		List<Tile> allMatches = new List<Tile>();
		List<Tile> matchingSet = new List<Tile>();
		List<Tile> powerUpTiles = new List<Tile>();
		List<Constants.GemType> powerUps = new List<Constants.GemType> ();
		Tile lastTile = null;
		Tile currentTile = null;

		// Vertical Matches
		for (int x = 0; x < Constants.GRID_WIDTH; x++) {
			for(int y = 0; y < Constants.GRID_HEIGHT; y++) {

				currentTile = tiles[x,y];

				if(currentTile != null) {

					// last column speficic matching logic, see the else clause for how it works normally until this point
					if(y == 0) {
						if(matchingSet.Count >= 3) {
							EvaluateMatch(matchingSet, allMatches, powerUpTiles, powerUps);
						}
						matchingSet.Clear();
						matchingSet.Add(currentTile);
						lastTile = null;
					
					} else {
						// add colour matching gems to the matching set if valid
						if(lastTile != null && currentTile.GetGem() != null && lastTile.GetGem() != null && currentTile.GetGem().GetGemColour() == lastTile.GetGem().GetGemColour()) {
							if(!matchingSet.Contains(lastTile)) {
								matchingSet.Add(lastTile);
							}
							matchingSet.Add(currentTile);
						
						// once a matching is interrupted, count how many gems were matched and evaluate the match if more than 3 were matched
						} else {
							if(matchingSet.Count >= 3) {
								EvaluateMatch(matchingSet, allMatches, powerUpTiles, powerUps);
							}
							matchingSet.Clear();
						}
					}
					lastTile = currentTile;
				
				// case when a match reaches an empty tile i.e. end of column or blank tile
				} else {
					if(matchingSet.Count >= 3) {
						EvaluateMatch(matchingSet, allMatches, powerUpTiles, powerUps);
					}
					matchingSet.Clear();
					lastTile = null;
				}
			}
		}

		// Horizontal Matches, as with vertical but with x axis
		for (int y = 0; y < Constants.GRID_HEIGHT; y++) {
			for (int x = 0; x < Constants.GRID_WIDTH; x++) {
				
				currentTile = tiles [x, y];

				if (currentTile != null) {
					if (x == 0) {
						if (matchingSet.Count >= 3) {
							EvaluateMatch (matchingSet, allMatches, powerUpTiles, powerUps);
						}
						matchingSet.Clear ();
						matchingSet.Add (currentTile);
						lastTile = null;
					} else {
						if (lastTile != null && currentTile.GetGem () != null && lastTile.GetGem () != null && currentTile.GetGem ().GetGemColour () == lastTile.GetGem ().GetGemColour ()) {
							if (!matchingSet.Contains (lastTile)) {
								matchingSet.Add (lastTile);
							}
							matchingSet.Add (currentTile);
						} else {
							if (matchingSet.Count >= 3) {
								EvaluateMatch (matchingSet, allMatches, powerUpTiles, powerUps);
							}
							matchingSet.Clear ();
						}
					}
					lastTile = currentTile;
				} else {
					if (matchingSet.Count >= 3) {
						EvaluateMatch (matchingSet, allMatches, powerUpTiles, powerUps);
					}
					matchingSet.Clear ();
					lastTile = null;
				}
			}
		}

		if(matchingSet.Count >= 3) {
			EvaluateMatch(matchingSet, allMatches, powerUpTiles, powerUps);
		}

		// finally process all gems through different matching sets
		foreach (Tile tile in allMatches) {
			Gem tempGem = null;
			if(tile.GetGem() != null) {

				// increment break count, clear the tile, destroy the gem
				tempGem = tile.GetGem();
				gemsBroken[(int)tempGem.GetGemColour()]++;

				tile.ClearTile();

				// spawn powerup gems based on tiles marked by evaluation
				if(powerUpTiles.Contains(tile)) {
					int p = (int)powerUps[powerUpTiles.IndexOf(tile)];
					// if a rainbow is to be spawned, mark colour as rainbow, otherwise spawn the same colour
					// as the gem that had existed
					if(powerUps[powerUpTiles.IndexOf(tile)] == Constants.GemType.Rainbow) {
						SpawnGem(tile, p, false, (int)Constants.GemColour.Rainbow);
					} else {
						SpawnGem(tile, p, false, (int)tempGem.GetGemColour());
					}
				}
			}
		}
		return gemsBroken;
	}

	/// <summary>
	/// Evaluates a match and adds it to a list of all the matches made in one round and whether and which tiles should
	/// Spawn a powerup
	/// </summary>
	/// <param name="matchingSet">new matching set of tiles to evaluate</param>
	/// <param name="allMatches">all tiles that have been matched in a round</param>
	/// <param name="powerUpTiles">tiles that should receive powerup spawns</param>
	/// <param name="powerUps">list of power up types that should be spawned</param>
	private void EvaluateMatch(List<Tile> matchingSet, List<Tile> allMatches, List<Tile> powerUpTiles, List<Constants.GemType> powerUps) {
		// tract the most recently moved gem in matching set
		Tile mostRecentTile = null;

		float mostRecentTime = -1;
		foreach (Tile tile in matchingSet) {

			// if theres an overlap in the current matching set and those that already been matched so far, i.e. a cross match,
			// prepare to spawn a burst gem
			if (allMatches.Contains (tile)) {
				powerUpTiles.Add (tile);
				powerUps.Add (Constants.GemType.Burst);

			// add matching set to the gem and keep track of the most recently moved gem of all
			} else {
				allMatches.Add (tile);
				float lastMoved = tile.GetGem ().GetLastMoved ();
				if (lastMoved > mostRecentTime) {
					mostRecentTile = tile;
					mostRecentTime = lastMoved;
				}
			}
		}

		// if a 5 match was present in the current set, spawn a rainbow gem
		if(matchingSet.Count >= 5) {
			if(powerUpTiles.Contains(mostRecentTile)) {
				int i = powerUpTiles.IndexOf(mostRecentTile);
					powerUpTiles.RemoveAt(i);
					powerUps.RemoveAt(i);
			}
			powerUpTiles.Add(mostRecentTile);
			powerUps.Add(Constants.GemType.Rainbow);
		
		// 4 matches, spawn a line clearing gem based on whether the last moved gem made a horizontal or vertical movement
		} else if (matchingSet.Count >= 4) {
			powerUpTiles.Add(mostRecentTile);
			if(mostRecentTile.GetGem().WasLastMoveVertical()) {
				powerUps.Add (Constants.GemType.VerticalClear);
			} else {
				powerUps.Add (Constants.GemType.HorizontalClear);
			}
		}
	}

	/// <summary>
	/// Increments a gem death by colour count due to powerups
	/// </summary>
	/// <param name="c">C.</param>
	public void AddBrokenByPowerUpTile(Constants.GemColour c) {
		powerUpBreaks.Add (c);
	}

	/// <summary>
	/// Processes the total number of gems by colour broken by powerups
	/// </summary>
	/// <returns>Array of gems by colour</returns>
	public int[] CalculatePowerUpBreaks() {
		int[] gemsBroken = new int[Constants.NUM_COLOURS];
		foreach (Constants.GemColour c in powerUpBreaks) {
			if(c != Constants.GemColour.Rainbow) {
				gemsBroken[(int)c]++;
			} else {
				for(int i = 0; i < Constants.NUM_COLOURS; i++) {
					gemsBroken[i]++;
				}
			}
		}
		powerUpBreaks.Clear();
		return gemsBroken;
	}

	/// <summary>
	/// Gets whether the grid has been filled with gems or not
	/// </summary>
	/// <returns><c>true</c>if the grid is filled with gems <c>false</c> otherwise.</returns>
	public bool GridFilled() {
		for (int x = 0; x < Constants.GRID_WIDTH; x++) {
			for (int y = 1; y < Constants.GRID_HEIGHT; y++) {
				if(tiles[x,y] != null && tiles[x,y].GetGem() == null) {
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Applies gravity to all the gem, moving them down/diagonal if possible
	/// </summary>
	/// <param name="sliding">If set to <c>true</c> process diagonal movement </param>
	public void ProcessGravity(bool sliding) {
		for (int x = 0; x < Constants.GRID_WIDTH; x++) {
			for(int y = 1; y < Constants.GRID_HEIGHT; y++) {
				Tile currentTile = tiles[x, y];
				Tile lowerTile = tiles[x, y - 1];
				Tile leftTile = null;
				Tile lowerLeftTile = null;
				Tile rightTile = null;
				Tile lowerRightTile = null;

				// look at tile to the left of current
				if(x - 1 >= 0) {
					leftTile = tiles[x - 1, y];
					lowerLeftTile = tiles[x - 1, y - 1];
				}

				// look at tile to the right of current
				if(x + 1 < Constants.GRID_WIDTH - 1) {
					rightTile = tiles[x + 1, y];
					lowerRightTile = tiles[x + 1, y - 1];
				}

				if(currentTile.tileState == Constants.TileState.Normal && currentTile.GetGem() != null) {

					// standard down movement
					if(!sliding) {

						if(IsTileOccupiable(lowerTile)) {
							currentTile.GetGem().Move(lowerTile);
							lowerTile.SetGem(currentTile.GetGem());
							currentTile.SetGem(null);
						}
					
					// if left and right are obstructed and diagonal movement is allowed and possible, move gem diagonally
					} else {
						if(IsTileObstruction(leftTile) && IsTileOccupiable(lowerLeftTile)) {
							currentTile.GetGem().Move(lowerLeftTile);
							lowerLeftTile.SetGem(currentTile.GetGem());
							currentTile.SetGem(null);
						} else if (IsTileObstruction(rightTile) && IsTileOccupiable(lowerRightTile)) {
							currentTile.GetGem().Move(lowerRightTile);
							lowerRightTile.SetGem(currentTile.GetGem());
							currentTile.SetGem(null);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Whether the tile is marke has an obstruction
	/// </summary>
	/// <returns><c>true</c> if the tile has an obstruction </returns>
	/// <param name="tile">the tile checked.</param>
	private bool IsTileObstruction(Tile tile) {
		return tile == null || tile.tileState != Constants.TileState.Normal;
	}

	/// <summary>
	/// Whether the tile is empty and can receive another gem
	/// </summary>
	/// <returns><c>true</c> if the tile can accept a gem</returns>
	/// <param name="tile">the tile checked.</param>
	private bool IsTileOccupiable(Tile tile) {
		return tile != null && tile.tileState == Constants.TileState.Normal && tile.GetGem() == null;
	}

	/// <summary>
	/// Attempts to mix swapped powerups to create stronger effects
	/// </summary>
	/// <param name="s">s - source tile</param>
	/// <param name="d">d - destination tile</param>
	public void TryMixPowerUps(Tile s, Tile d) {

		Gem sGem = s.GetGem();
		Gem dGem = d.GetGem();

		Constants.GemType sGemType = sGem.GetGemType();
		Constants.GemType dGemType = dGem.GetGemType();

		// Cross clear centred on the destination tile when 2 line clearers are mixed
		if ((dGemType == Constants.GemType.HorizontalClear || dGemType == Constants.GemType.VerticalClear) &&
			(sGemType == Constants.GemType.HorizontalClear || sGemType == Constants.GemType.VerticalClear)) {
			if(dGem.WasLastMoveVertical()) {
				dGem.SetGemType ((int)Constants.GemType.VerticalClear);
				sGem.SetGemType ((int)Constants.GemType.HorizontalClear);
			} else {
				dGem.SetGemType ((int)Constants.GemType.HorizontalClear);
				sGem.SetGemType ((int)Constants.GemType.VerticalClear);
			}
		
		// Triple cross clear on the destination if burst and line are mixed
		} else if ((dGemType == Constants.GemType.Burst && (sGemType == Constants.GemType.HorizontalClear || sGemType == Constants.GemType.VerticalClear)) ||
			(sGemType == Constants.GemType.Burst && (dGemType == Constants.GemType.HorizontalClear || dGemType == Constants.GemType.VerticalClear))) {
			sGem.SetGemType ((int)Constants.GemType.TripleCrossClear);
		
		// 5x5 clear when two 3x3 clears are mixed
		} else if (dGemType == Constants.GemType.Burst && sGemType == Constants.GemType.Burst) {
			dGem.SetGemType ((int)Constants.GemType.BigBurst);
			sGem.SetGemType ((int)Constants.GemType.BigBurst);
		
		// all clear when two rainboes are mixed
		} else if (dGemType == Constants.GemType.Rainbow && sGemType == Constants.GemType.Rainbow) {
			for (int x = 0; x < Constants.GRID_WIDTH; x++) {
				for (int y = 0; y < Constants.GRID_HEIGHT; y++) {
					if (tiles [x, y] != null && tiles [x, y].GetGem () != null) {
						AddBrokenByPowerUpTile(tiles [x, y].GetGem ().GetGemColour());
						tiles [x, y].ClearTile ();
					}
				}
			}
		
		// copy powerup effects over to gems of the same colour when non-rainbow powerup an rainbow are mixed
		} else if (dGemType == Constants.GemType.Rainbow) {
			for (int x = 0; x < Constants.GRID_WIDTH; x++) {
				for (int y = 0; y < Constants.GRID_HEIGHT; y++) {
					if (tiles [x, y] != null && tiles [x, y].GetGem () != null) {
						dGem.SetGemType((int)Constants.GemType.Normal);
						dGem.SetGemColour((int)sGem.GetGemColour());
						Gem tempGem = tiles [x, y].GetGem ();
						if (tempGem.GetGemColour () == sGem.GetGemColour ()) {
							if(tempGem.GetGemType() == Constants.GemType.Normal) {
								tempGem.SetGemType ((int)sGemType);
							}
							tiles [x, y].ClearTile ();
							AddBrokenByPowerUpTile (tempGem.GetGemColour ());
						}
					}
				}
			}
		
		// same as above
		} else if (sGemType == Constants.GemType.Rainbow) {
			for (int x = 0; x < Constants.GRID_WIDTH; x++) {
				for (int y = 0; y < Constants.GRID_HEIGHT; y++) {
					if (tiles [x, y] != null && tiles [x, y].GetGem () != null) {
						sGem.SetGemType((int)Constants.GemType.Normal);
						sGem.SetGemColour((int)dGem.GetGemColour());
						Gem tempGem = tiles [x, y].GetGem ();
						if (tempGem.GetGemColour () == dGem.GetGemColour ()) {
							if(tempGem.GetGemType() == Constants.GemType.Normal) {
								tempGem.SetGemType ((int)dGemType);
							}
							tiles [x, y].ClearTile ();
							AddBrokenByPowerUpTile (tempGem.GetGemColour ());
						}
					}
				}
			}
		} else {
			return;
		}

		// mark the swapped powerup gems as those broken by powerups
		AddBrokenByPowerUpTile (dGem.GetGemColour ());
		AddBrokenByPowerUpTile (sGem.GetGemColour ());

		// kill the powerup tile/gem to trigger it's effects
		d.ClearTile();
		s.ClearTile();
	}
}
