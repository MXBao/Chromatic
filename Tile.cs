using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Depicts the tile, the gem it holds and it's background
/// </summary>
public class Tile : MonoBehaviour {

	// the gem contained by the tile, whether it's a bottom tile and it's position
	private Gem gemContained;
	private bool bottom;
	[HideInInspector]
	public Vector2 position;

	// visual component of the tile
	public Image stateImage;
	public Sprite[] stateSprites;
	public Constants.TileState tileState;


	void Awake() {
		if (Application.isMobilePlatform) {
			Cursor.visible = false;
		}
//		RectTransform rectTransform = GetComponent<RectTransform>();
//		BoxCollider2D collider = GetComponent<BoxCollider2D> ();
//		collider.size = new Vector2(rectTransform.rect.height, rectTransform.rect.width);

	}

	/// <summary>
	/// Unhighlight the tile on mouse over
	/// </summary>
	public void OnMouseOver() {
		GetComponent<Image> ().color = Color.black;
	}

	/// <summary>
	/// Set's the tile's position and state
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="t">the tile's state</param>
	public void SetTile(int x, int y, Constants.TileState t) {
		position.x = x;
		position.y = y;
		tileState = t;
		RenderState();
	}

	/// <summary>
	/// Render different sprites depending on the state of the tile
	/// </summary>
	private void RenderState() {
		stateImage.sprite = stateSprites[(int)tileState];
	}

	/// <summary>
	/// Sets the gem contained in the tile
	/// </summary>
	/// <param name="g">g - the gem</param>
	public void SetGem(Gem g) {
		gemContained = g;
	}

	/// <summary>
	/// Gets the gem contained in the tile
	/// </summary>
	/// <returns>The gem</returns>
	public Gem GetGem() {
		return gemContained;
	}

	/// <summary>
	/// Destroys the gem contained in the tile and delete the reference to it
	/// </summary>
	public void ClearTile() {
		if (gemContained != null) {
			gemContained.Die ();
			gemContained = null;
		}
	}
}
