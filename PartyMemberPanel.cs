using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Script for a party member panel, the display of characters health, name and the gem charges they have
/// </summary>
public class PartyMemberPanel : MonoBehaviour {

	// References to components making up the panel
	public Text nameLabel;
	public Text healthLabel;
	public Image healthForeGround;
	public Image healthBackGround;
	public Image[] colourSlots;
	public Text[] colourCounters;

	// text size boundaries for the gem charge counter(s)
	private int counterTextSizeMin = 32;
	private int counterTextSizeMax = 36;

	// whether there is a next tier and cost of the next tier
	public int[] previewCost;
	public bool previewingCost = false;

	// the character and the gem charges it has by colour
	private Character character;
	private bool[] characterColours;

	/// <summary>
	/// Sets the character of the panel and display their stats
	/// </summary>
	/// <param name="c">c - character depicted on panel</param>
	public void SetCharacter(Character c) {
		if (c != null) {
			nameLabel.enabled = true;
			healthLabel.enabled = true;
			healthForeGround.enabled = true;
			healthBackGround.enabled = true;
			for (int i = 0; i < Constants.NUM_COLOURS; i++) {
				colourSlots [i].enabled = true;
				colourCounters [i].enabled = true;
			}

			character = c;
			nameLabel.text = c.name;
			UpdateHealth ();
			characterColours = c.canUseColours;
			int trackerTransformOffset = -Constants.INDEX_OFFSET;
			for (int i = 0; i < Constants.NUM_COLOURS; i++) {
				if (!characterColours [i]) {
					trackerTransformOffset++;
					colourSlots [i].enabled = false;
					colourCounters [i].enabled = false;
					for (int j = i; j < Constants.NUM_COLOURS; j++) {
						colourSlots [j].transform.position = colourSlots [j - trackerTransformOffset].transform.position;
						colourCounters [j].transform.position = colourCounters [j - trackerTransformOffset].transform.position;
					}
				}
			}
		}
	}

	/// <summary>
	/// Sets the previewing on/off, and colours according to whether the player can afford it
	/// </summary>
	/// <param name="p">If set to <c>true</c> preview the cost</param>
	/// <param name="c">c - array of gem charges</param>
	public void SetPreview(bool p, int[] c = null) {
		previewingCost = p;
		previewCost = c;
		UpdateColours ();
	}

	/// <summary>
	/// Hepler method to updates the gem cost text display
	/// </summary>
	public void UpdateColours() {
		int[] gemsCollected = character.colours;
		for (int i = 0; i < Constants.NUM_COLOURS; i++) {
			if(characterColours[i]) {
				if(!previewingCost || previewCost != null && previewCost[i] == 0) {
					colourCounters[i].text = "x" + gemsCollected[i];
					colourCounters[i].fontSize = counterTextSizeMax;
				} else {
					int gemsRemaining = gemsCollected[i] - previewCost[i];
					if(gemsRemaining < 0) {
						colourCounters[i].text = gemsCollected[i] + "→" + "<color=red>" + Mathf.Abs(gemsRemaining) + "</color>";
					} else {
						colourCounters[i].text = gemsCollected[i] + "→" + gemsRemaining;
					}

					colourCounters[i].fontSize = counterTextSizeMin;
				}
			}
		}
	}

	/// <summary>
	/// Updates the health bar display
	/// </summary>
	public void UpdateHealth() {
		float cHealth = character.currentHealth;
		float mHealth = character.maxHealth;
		healthLabel.GetComponent<Text>().text = Mathf.RoundToInt(cHealth) + "/" + mHealth;
		healthForeGround.fillAmount = cHealth / mHealth;
	}

}
