using UnityEngine;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Visual script for the enemy selection circle
/// </summary>
public class EnemySelectionCircle : MonoBehaviour {

	/// renderer and visual attributes
	public SpriteRenderer spriteRenderer;
	private float shrinkRate = 5.0f;
	private float initialScale = 3.0f;
	private float targetScale = 1.0f;
	private float spinRate = 90.0f;

	/// <summary>
	/// Rotate the circle over time
	/// </summary>
	void Update() {
		transform.Rotate(Vector3.forward, Time.deltaTime * spinRate);
	}

	/// <summary>
	/// Places the circle on the selected enemy
	/// </summary>
	/// <param name="e">e - enemy selected</param>
	public void SelectTarget(Enemy e) {
		spriteRenderer.enabled = true;
		transform.position = e.transform.position + e.feetOffset;
		StartCoroutine(Shrink());
	}

	/// <summary>
	/// Scaling effect; setting to default then shrinking the circle
	/// </summary>
	IEnumerator Shrink() {
		transform.localScale = new Vector3 (initialScale, initialScale, targetScale);
		while(transform.localScale.x > 1.0f) {
			yield return new WaitForSeconds(Time.deltaTime);
			transform.localScale = new Vector3(transform.localScale.x - Time.deltaTime * shrinkRate,
			                                   transform.localScale.y - Time.deltaTime * shrinkRate,
			                                   targetScale);
		}
		transform.localScale = new Vector3(targetScale, targetScale, targetScale);
	}


}
