using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Holds attributes, skills and animator component  character
/// </summary>
public abstract class Character : MonoBehaviour {

	// name of character, colours ir can use and references to panels
	public string name;
	public int[] colours;
	public PartyMemberPanel originalPanel;
	public PartyMemberPanel memberPanel;
	public SkillDisplay skillDisplay;
	public Skill[] skills;
	[HideInInspector]

	// Colours that the character can use
	public bool[] canUseColours;

	// Game character attributes
	public int level;
	public float baseHealth;
	public float healthPerLevel;
	public float baseAttack;
	public float attackPerLevel;
	public float defense;
	public float critChance;
	
	[HideInInspector]
	public float currentHealth;
	[HideInInspector]
	public float maxHealth;
	
	protected float attack;
	[HideInInspector]
	public float modifiedAttack;
	
	protected float currentExp;
	protected float expToNext;

	public bool dead;

	// Animator and how long delays should be for each skill
	[HideInInspector]
	public Animator animator;
	public float singleAttackAnimDuration = 0.5f;
	public float comboedAttackDurationFactor = 0.8f;
	public float multiAttackAnimDuration = 0.5f;
	public float supportAnimDuration = 0.5f;
	public float hurtAnimDuration = 0.5f;

	// Reference to gamemanager and enemies
	private GameManager gameManager;
	public List<Enemy> enemies;
	public Enemy target;

	// Where the character's feet relative to the image's centre and where particles should appear relatively
	public Vector3 feetOffset;
	public Vector3 buffParticlesOffset;
	public Vector3 healParticlesOffset;

	// Initialize Stats
	public virtual void Awake() {
		gameManager = GameObject.FindWithTag ("GameController").GetComponent<GameManager>();
		animator = GetComponent<Animator> ();
		maxHealth = baseHealth + healthPerLevel * level;
		attack = baseAttack + attackPerLevel * level;
		modifiedAttack = attack;
		currentHealth = maxHealth;
	}

	/// <summary>
	/// Executes a skill base on specified index
	/// </summary>
	/// <returns>float - duration taken for skill to execute cased on animations and particles </returns>
	/// <param name="s">s - skill by id</param>
	public virtual float UseSkill(int s) {
		animator.SetInteger("tier", skills[s].activationTier);
		skills[s].StartCoroutine(skills[s].UseSkill(target, gameManager.characters, enemies));
		skills[s].CalculateSkillDuration(target, gameManager.characters, enemies);
		return skills [s].GetSkillDuration();
	}

	//TODO end of turn events
	public virtual void EndTurn() {
	}

	//TODO buffs
	public virtual void TickDownBuffs() {
	}

	/// <summary>
	/// Adds a specified gem 'charge' by colour to the character's gem pool
	/// </summary>
	/// <param name="colour">c - colour of gem by id </param>
	/// <param name="amount">n - number to add (default 1)</param>
	public virtual void AddColour(int c, int n = 1) {
		if (!dead && canUseColours[c]) {
			colours[c] += n;
		}
		memberPanel.UpdateColours ();
	}

	/// <summary>
	/// Determines the colours that will be tracked for the character based on the skills
	/// that they have.
	/// </summary>
	public void InitializeUsableColours() {
		canUseColours = new bool[Constants.NUM_COLOURS];

		// if check through all skills and skill tiers for gem use and declare true where
		// appropriate
		for (int i = 0; i < Constants.NUM_COLOURS; i++) {
			for(int j = 0; j < skills.Length; j++) {
				for(int k = 0; k < skills[j].tiers.Length; k++) {
					if(skills[j].tiers[k].gemCosts[i] > 0) {
						canUseColours[i] = true;
					}
				}
			}
		}
	}

	/// <summary>
	/// Determines whether the character may execute a skill based on the gem cost of the tier
	/// </summary>
	/// <returns><c>true</c> if this instance can use skill the specified skill of the specified tier</returns>
	/// <param name="s">s - skill by id</param>
	/// <param name="o">o - still tier, initial tier by default</param>
	public bool CanUseSkill(Skill s, int o = 0) {

		// Checks that the character satisfies gem requirements for the specified skill/tier and skill tier is
		// within range of tiers and prematurely exit when proven false
		for (int i = 0; i < Constants.NUM_COLOURS; i++) {
			if(s.activationTier + o >= s.tiers.Length || colours[i] < s.tiers[s.activationTier + o].gemCosts[i]) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Takes the damage!
	/// </summary>
	/// <param name="d">d - amount of damage to be taken, negative if healing</param>
	public void TakeDamage(float d) {

		// calculate and update the interface
		currentHealth = Mathf.Clamp (currentHealth - d, 0, maxHealth);
		memberPanel.UpdateHealth();

		// play animation if character is still alive or declare them dead
		if (currentHealth > 0) {
			animator.Play(Constants.HURT_ANIM_NAME);
		} else {
			Die ();
		}
	}

	/// <summary>
	/// Declare character dead and play death animation
	/// </summary>
	protected void Die() {
		animator.Play(Constants.DIE_ANIM_NAME);
		dead = true;
	}
}
