using UnityEngine;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Background display of the tile
/// </summary>
public class GridBackGround : MonoBehaviour {

	public GameObject tileBackGroundPrefab;
	private Tile[,] tiles;

	public void SetGridBackGround(Constants.TileState[,] g) {
		tiles = new Tile[Constants.GRID_WIDTH,Constants.GRID_HEIGHT];
		for (int x = 0; x < Constants.GRID_WIDTH; x++) {
			for (int y = 0; y < Constants.GRID_HEIGHT; y++) {
				GameObject o = Instantiate(tileBackGroundPrefab, transform.position,
				                           transform.rotation) as GameObject;
				o.transform.SetParent(transform, false);
			}
		}
	}
}
