using UnityEngine;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Holds constants
/// </summary>
public class Constants : MonoBehaviour {

	public const int GRID_WIDTH = 6;
	public const int GRID_HEIGHT = 8;
	public const float TILE_OFFSET = 15f;
	public const float TILE_DISTANCE = 1.1f;
	public const int NUM_COLOURS = 6;
	public const int SPAWNED_COLOURS = 4;

	public const float ONE = 1.0f;
	public const float DOUBLE = 2.0f;
	public const float HALF = 0.5f;
	public const float QUARTER = 0.3f;
	public const float HUNDRED = 100f;

	public const int INDEX_OFFSET = 1;
	public const int ZERO_INDEX = 0;

	public const float BASE_ANIM_DELAY = 0.25f;

	public const string SINGLE_ATTACK_ANIM_NAME = "SingleAttack";
	public const string SPECIAL_ATTACK_ANIM_NAME = "SpecialAttack";
	public const string MULTI_ATTACK_ANIM_NAME = "MultiAttack";
	public const string SUPPORT_ANIM_NAME = "SupportSpell";
	public const string HURT_ANIM_NAME = "Hurt";
	public const string DIE_ANIM_NAME = "Die";

	public const float PRIORITY_HEALTH_PERCENT_THRESHOLD = 0.5f;

	public enum TileState {
		Null,
		Normal,
		Stoned,
		Webbed,
		Corrupted
	}

	public enum GemColour {
		Red,
		Orange,
		Yellow,
		Green,
		Blue,
		Purple,
		Rainbow
	}

	public enum GemType {
		Normal,
		HorizontalClear,
		VerticalClear,
		Burst,
		Rainbow,
		TripleCrossClear,
		BigBurst
	}
}
