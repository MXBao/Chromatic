using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Depicts a skill button and handles when it is activated
/// </summary>
public class SkillButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

	// character, skill and tiers referred to by the button and the infobox
	public InfoBox infoBox;
	private Character character;
	private Skill skill;
	private int[] currentTierValue;
	private int[] nextTierCost;

	// the button and visual components
	private Button button;
	public Image skillIcon;
	public Text skillName;

	// Stars depicting what tier the skill is activated to
	public Image[] stars;
	public Sprite starActiveImage;
	public Sprite starInactiveImage;

	// Button variants depending on character's gem currency
	public Sprite availableImage;
	public Sprite unavailableImage;
	public Sprite nullImage;

	void Awake() {
		button = GetComponent<Button> ();
	}

	/// <summary>
	/// Update the skill infobox on click, refund if right click
	/// </summary>
	/// <param name="e">E.</param>
	public void OnPointerClick(PointerEventData e) {
		if (e.button == PointerEventData.InputButton.Right) {
			RefundTier();
		}

		DisplaySkillInfo ();
	}

	/// <summary>
	/// Show and update the info box on mouse entering
	/// </summary>
	/// <param name="e">E.</param>
	public void OnPointerEnter(PointerEventData e) {
		DisplaySkillInfo ();
	}

	/// <summary>
	/// Hide the info box and cost preview when mouse leaving
	/// </summary>
	/// <param name="e">E.</param>
	public void OnPointerExit(PointerEventData e) {
		infoBox.gameObject.SetActive (false);
		character.memberPanel.SetPreview (false);
	}

	/// <summary>
	/// Helper method to display the skill info inthe info box
	/// </summary>
	private void DisplaySkillInfo() {
		if (skill == null) {
			return;
		}

		infoBox.gameObject.SetActive (true);
		infoBox.SetInfo (skill);
		
		if (skill.activationTier + 1 < skill.tiers.Length) {
			character.memberPanel.SetPreview (true, skill.tiers [skill.activationTier + Constants.INDEX_OFFSET].gemCosts);
		}
	}

	/// <summary>
	/// Calculates whether the player can afford the next tier of the skill based the gem charges of the character
	/// </summary>
	/// <returns><c>true</c> if the player/character can afford future tier the specified skill tier <c>false</c>.</returns>
	/// <param name="i">i - skill tier by index</param>
	public bool CanAffordFutureTier(int i = 1) {
		if (skill.activationTier >= skill.tiers.Length) {
			return true;
		} else {
			return character.CanUseSkill(skill, i);
		}
	}

	/// <summary>
	/// Reverses the cost of a tier and increments the tier backwards
	/// </summary>
	public void RefundTier() {
		if(skill.activationTier >= 0) {
			for(int i = 0; i < Constants.NUM_COLOURS; i++) {
				character.AddColour(i, skill.tiers[skill.activationTier].gemCosts[i]);
			}
			skill.activationTier--;
		}
		UpdateDisplay ();
	}

	/// <summary>
	/// Spends gem charges to activate skill at a higher tier
	/// </summary>
	public void PurchaseNextTier() {
		if (CanAffordFutureTier()) {
			skill.activationTier++;
			if (skill.activationTier < skill.tiers.Length) {
				for (int i = 0; i < Constants.NUM_COLOURS; i++) {
					character.AddColour (i, -skill.tiers[skill.activationTier].gemCosts[i]);
				}
			}
		}
		UpdateDisplay ();
	}

	/// <summary>
	/// Replaces the skill depicted by the infobox
	/// </summary>
	/// <param name="c">c - the character that the skill originates from</param>
	/// <param name="s">s - the skill</param>
	public void ReplaceSkill(Character c, Skill s) {
		character = c;
		skill = s;
		if (skill == null) {
			skillIcon.sprite = nullImage;
			skillName.text = null;
		} else {
			skillIcon.sprite = skill.skillIcon;
			skillName.text = skill.skillName;
		}
		UpdateDisplay ();
	}

	/// <summary>
	/// Updates the display.
	/// </summary>
	public void UpdateDisplay() {
		// display stars to represent skill tier
		for(int i = 0; i < stars.Length; i++) {
			if(skill != null && i < skill.tiers.Length) {
				stars[i].color = Color.white;
				if(skill.activationTier >= i) {
					stars[i].sprite = starActiveImage;
				} else {
					stars[i].sprite = starInactiveImage;
				}
			} else {
				stars[i].color = Color.clear;
			}
		}

		// look of button depending on tier/next tier status of skill
		if (skill == null) {
			button.image.sprite = nullImage;
		} else if (CanAffordFutureTier ()) {
			button.image.sprite = availableImage;
		} else {
			button.image.sprite = unavailableImage;
		}
	}
}
