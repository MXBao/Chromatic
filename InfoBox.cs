using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Script for panel displaying skill info
/// </summary>
public class InfoBox : MonoBehaviour {

	// Reference to the text components
	public Text skillDescription;
	public Text tierPreview;

	// hexadecimal form of colours for formatting, red to purple
	private string[] hexColours = { "#B4151BFF", "#B44B19FF", "#DCD000FF", "#2F963DFF", "#005A96FF", "#6F0380FF" };

	/// <summary>
	/// Set the 
	/// </summary>
	/// <param name="s">S.</param>
	public void SetInfo(Skill s) {

		if (s == null) {
			return;
		}

		// display the skill/next tier's costs
		string skillCosts = ProcessCostDescription(s);

		// string together and display what the skill does based on whether it is the last tier/ what 
		// skill it is etc
		if (s.activationTier < Constants.ZERO_INDEX) {
			skillDescription.text = "TIER 1" + skillCosts + "\n"
				+ s.tiers [Constants.ZERO_INDEX].skillDescription;
			tierPreview.text = "[Left-click to assign gems. Right-click to refund.]";
		} else {
			skillDescription.text = "TIER " + (s.activationTier + Constants.INDEX_OFFSET) + skillCosts + "\n" + s.tiers [s.activationTier].skillDescription;
			if(s.activationTier == s.tiers.Length - Constants.INDEX_OFFSET) {
				tierPreview.text = "";
			} else {
				tierPreview.text = "[Next Tier: " + s.tiers [s.activationTier].nextTierDescription + "]";
			}
		}
	}

	/// <summary>
	/// Helper method to strings cost description of skill together together for display
	/// </summary>
	/// <returns>The cost description.</returns>
	/// <param name="s">s - the skill in question</param>
	private string ProcessCostDescription(Skill s) {

		if (s.activationTier == s.tiers.Length - Constants.INDEX_OFFSET) {
			return " | MAXED";
		}

		string skillCosts = "";

		if (s.activationTier < 0) {
			skillCosts += " | Cost:";
		} else if (s.activationTier + Constants.INDEX_OFFSET < s.tiers.Length) {
			skillCosts += " | ↑Tier Cost:";
		}
		
		for (int i = 0; i < Constants.NUM_COLOURS; i++) {
			int gemCount = s.tiers[s.activationTier + Constants.INDEX_OFFSET].gemCosts[i];
			if(gemCount > 0) {
				skillCosts += " <color=" + hexColours[i] + ">" + gemCount + " " + (Constants.GemColour)i + "</color>,";
			}
		}
		return skillCosts.Substring(0, skillCosts.Length - Constants.INDEX_OFFSET);
	}

}
