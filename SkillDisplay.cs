using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Script the panel displaying skill names briefly as they are activated
/// </summary>
public class SkillDisplay : MonoBehaviour {

	Image backGround;
	Text skillName;
	float timeOut = 0.75f;

	void Awake() {
		backGround = GetComponentInChildren<Image> ();
		skillName = GetComponentInChildren<Text> ();
	}

	public void DisplaySkill(string s) {
		StopCoroutine (StopRendering ());
		backGround.enabled = true;
		skillName.enabled = true;
		skillName.text = s;
		StartCoroutine (StopRendering ());
	}

	IEnumerator StopRendering() {
		yield return new WaitForSeconds (timeOut);
		backGround.enabled = false;
		skillName.enabled = false;
	}
}
