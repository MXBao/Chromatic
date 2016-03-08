using UnityEngine;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Holds properties of a bullet
/// </summary>
/// 
public class Bullet : MonoBehaviour {

	/// rederers and possible variants in bullet sprite
	public TrailRenderer trail;
	public SpriteRenderer spriteRenderer;
	public Sprite[] sprite;

	// the bullet's target and how the bullet should spin
	public Vector3 target;
	public float rotationalSpeed = 45.0f;

	/// <summary>
	/// Sets the bullet's trail renderer to the right layer and choose pool of sprite variants randomly
	/// </summary>
	void Initialize() {
		trail.sortingOrder = 10;
		spriteRenderer.sprite = sprite[Random.Range (0, sprite.Length)];
	}

	/// <summary>
	/// Attempt to rotate the bullet each frame
	/// </summary>
	void Update() {
		transform.Rotate(new Vector3(0, 0, transform.rotation.z + rotationalSpeed * Time.deltaTime));
	}
}
