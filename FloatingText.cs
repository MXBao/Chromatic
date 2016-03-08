using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Script for any pop-up style text
/// </summary>
public class FloatingText : MonoBehaviour {

	/// the type of value depicted and the corresponding colour
	public enum DisplayType {
		Damage,
		Heal,
		Buff,
		StatusEffect
	}

	public DisplayType displayType;
	public Color[] textColor;

	/// Reference to panel, the text components and their offsets
	public RectTransform panelTransform;
	public Text backGroundText;
	public Text foreGroundText;
	private float horizontalAnchorOffset = 0.3f;
	private float verticalAnchorOffset = 0.2f;

	/// How long/much the text should move after it appears and how long it should stay on screen
	private float displaceMentDuration = 0.2f;
	private float timeOutDuration = 0.5f;
	private float maxDisplaceMent = 255.0f;

	/// <summary>
	/// Initializes the text by position, it's content and colour by display type
	/// </summary>
	/// <param name="p">p - position of the text</param>
	/// <param name="s">s - the text message</param>
	/// <param name="t">t - the type of text depicted/param>
	public void Initialize(Vector3 p, string s, DisplayType t) {

		// position in the world
		Vector3 viewPos = Camera.main.WorldToViewportPoint (p);
		Transform canvas = GameObject.FindWithTag ("ForeGroundCanvas").GetComponent<Transform>();
		transform.SetParent(canvas, false);
		transform.SetAsLastSibling ();
		panelTransform.anchorMin = new Vector2 (viewPos.x - horizontalAnchorOffset, viewPos.y - verticalAnchorOffset);
		panelTransform.anchorMax = new Vector2 (viewPos.x + horizontalAnchorOffset, viewPos.y + verticalAnchorOffset);

		// what strings should be displayed
		backGroundText.text = s;
		foreGroundText.text = s;

		// the colour based on display type
		displayType = t;
		foreGroundText.color = textColor[(int)displayType];

		// begin to animate the text
		StartCoroutine (Animate());
	}

	/// <summary>
	/// Animates movement of the floating text
	/// </summary>
	IEnumerator Animate() {
		float timer = displaceMentDuration;
		float displaceMent = maxDisplaceMent;
		while (timer > 0) {
			timer -= Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
			switch(displayType) {

				//Move up then down after peak duration if text depicts damage
				case DisplayType.Damage:
					if(timer > displaceMentDuration * Constants.HALF) {
						panelTransform.anchoredPosition = new Vector2(panelTransform.anchoredPosition.x, panelTransform.anchoredPosition.y + displaceMent * Time.deltaTime);
					} else {
						panelTransform.anchoredPosition = new Vector2(panelTransform.anchoredPosition.x, panelTransform.anchoredPosition.y - displaceMent * Time.deltaTime);
					}
				break;

				//TODO
				case DisplayType.Heal:

				//NOT IN
				case DisplayType.Buff:
					panelTransform.anchoredPosition = new Vector2(panelTransform.anchoredPosition.x, panelTransform.anchoredPosition.y + displaceMent * Time.deltaTime);
					displaceMent -= displaceMent * Time.deltaTime;
				break;

				//TODO
				case DisplayType.StatusEffect:
				break;
			}
		}

		yield return new WaitForSeconds(timeOutDuration - displaceMentDuration);
		Destroy (gameObject);
	}
}
