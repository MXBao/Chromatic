using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Depicts a skill that may be used by a character, it's tiers, gem costs and their effects
/// </summary>

[System.Serializable]
public abstract class Skill : MonoBehaviour {

	// The character using the skill, it's tiers and icons
	public Character user;
	public int activationTier;
	public string skillName;
	public Sprite skillIcon;

	// maximum boundary for variation on potency of the skill e.g. 0.15 is 85% - 115%
	public float randOffset = 0.15f;

	// How long the skill takes animate and the type of skill it is
	protected float skillDuration;
	public enum SkillType {
		SingleAttack,
		MultiAttack,
		Support
	}

	public SkillType skillType;

	// Attributes relating to each of the skill's tiers
	[System.Serializable]
	public struct skillTier {
		public int[] gemCosts;
		public float multiplier;
		public GameObject particles;
		[HideInInspector]
		public string skillDescription;
		[HideInInspector]
		public string nextTierDescription;
	}
	
	public skillTier[] tiers;

	private void Start() {
		SetSkillDescriptions ();
	}

	/// <summary>
	/// Uses the skill.
	/// </summary>
	/// <returns>The skill.</returns>
	/// <param name="t">t - the enemy target</param>
	/// <param name="p">p - the party </param>
	/// <param name="e">e - the enemy wave</param>
	public virtual IEnumerator UseSkill (Enemy t, Character[] p, List<Enemy> e) {
		CalculateSkillDuration (t, p, e);
		yield return null;
	}

	/// <summary>
	/// Handles targetting of the skill, attempts to pick a designated enemy if one is selected
	/// and it's still alive, otherwise picks next first available enemy
	/// </summary>
	/// <returns>Targetable Enemy</returns>
	/// <param name="t">t - the enemy target</param>
	/// <param name="e">e - the enemy wave</param>
	protected Enemy PickAliveEnemy(Enemy t, List<Enemy> e) {
		Enemy target = null;
		if (t != null && !t.dead) {
			target = t;
		} else {
			foreach(Enemy enemy in e) {
				if(enemy != null && !enemy.dead) {
					target = enemy;
					break;
				}
			}
		}
		return target;
	}

	/// <summary>
	/// Returns true if at least one enemy is alive
	/// </summary>
	/// <returns><c>true</c>At least one enemy is alive<c>false</c>All enemies dead</returns>
	/// <param name="e">e - list of enemies</param>
	protected bool AreEnemiesAlive(List<Enemy> e) {
		bool enemyAlive = false;
		foreach (Enemy enemy in e) {
			if(enemy != null && !enemy.dead) {
				enemyAlive = true;
				break;
			}
		}
		return enemyAlive;
	}

	/// <summary>
	/// Returns true if at least one ally is alive
	/// </summary>
	/// <returns><c>true</c>At least one ally is alive<c>false</c>All allies dead</returns>
	/// <param name="e">p - array of characters (party)</param>
	protected bool AreAlliesAlive(Character[] p) {
		bool allyAlive = false;
		foreach (Character ally in p) {
			if(ally != null && !ally.dead) {
				allyAlive = true;
				break;
			}
		}
		return allyAlive;
	}

	/// <summary>
	/// Resets the skill back to the first tier
	/// </summary>
	protected void ResetTier() {
		activationTier = -Constants.INDEX_OFFSET;
	}

	/// <summary>
	/// Calculates the duration of the skill
	/// </summary>
	/// <param name="t">t - the enemy target</param>
	/// <param name="p">p - the party </param>
	/// <param name="e">e - the enemy wave</param>
	public abstract void CalculateSkillDuration (Enemy t, Character[] p, List<Enemy> e);

	/// <summary>
	/// Sets the skill descriptions, varies by skill
	/// </summary>
	protected abstract void SetSkillDescriptions ();

	/// <summary>
	/// Gets the duration of the skill.
	/// </summary>
	/// <returns>The skill duration.</returns>
	public float GetSkillDuration() {
		return skillDuration;
	}

	/// <summary>
	/// Random number generator to vary magnitude of skill effect
	/// </summary>
	/// <returns>The random factor.</returns>
	protected float GenerateRandomFactor() {
		return Random.Range(Constants.ONE - randOffset, Constants.ONE + randOffset);
	}
}
