using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Author Phuc Tran ptvtran@gmail.com
/// Depiction of an enemy
/// </summary>
public class Enemy : MonoBehaviour, IPointerClickHandler {

	/// stats
	[HideInInspector]
	public float currentHealth;
	public float health = 200f;
	public float attack = 10f;
	public bool dead;

	/// reference to all characters and which one is the current target
	public Character[] characters;
	private Character target;

	/// whether the enemy has a special attack and how many turns should pass between it's use
	public bool hasSpecial = false;
	public int specialInterval = 3;
	protected int turnsToSpecial = 0;

	/// animator component 
	private Animator animator;
	public float attackDelay = 0.5f;
	public float specialAttackDelay = 0.5f;
	public float hurtAnimDuration = 0.5f;
	public float deathAnimDuration = 0.5f;
	
	/// values depicting how fast melee enemies should travel on attack/return
	private Vector3 homePosition;
	public Vector3 feetOffset = new Vector3(0, -0.5f, 0);
	public float lungeSpeed = 20.0f;
	public float returnSpeed = 30.0f;
	public float attackRange = 1.2f;

	/// reference to gamemanager and interfaces
	public GameObject healthBar;
	public GameObject floatingUI;
	private GameManager gameManager;

	/// <summary>
	/// Initialize components, defaults and interfaces
	/// </summary>
	void Awake() {
		animator = GetComponent<Animator>();
		gameManager = GameObject.FindWithTag ("GameController").GetComponent<GameManager>();
		currentHealth = health;
		UpdateHealthBars ();
		homePosition = transform.position;
	}

	/// <summary>
	/// Set the enemy as selected when it's pointed to
	/// </summary>
	/// <param name="e">e - pointer event</param>
	public void OnPointerClick(PointerEventData e) {
		gameManager.SelectTarget (this);
	}

	/// <summary>
	/// Picks a target from the characters randomly unless one is vulnerable (below 50% health)
	/// </summary>
	protected virtual void PickTarget() {
		float lowestHealthPercentage = Mathf.Infinity;
		Character lowestHealthCharacter = null;
		foreach (Character c in characters) {
			float characterHealthPercentage = c.currentHealth / c.maxHealth;
			if(characterHealthPercentage > 0 && characterHealthPercentage < lowestHealthPercentage) {
				lowestHealthPercentage = characterHealthPercentage;
				lowestHealthCharacter = c;
			}
		}

		if (lowestHealthPercentage <= Constants.PRIORITY_HEALTH_PERCENT_THRESHOLD) {
			target = lowestHealthCharacter;
		} else {
			target = characters[Random.Range(0, characters.Length)];
		}
	}

	/// <summary>
	/// Takes the turn
	/// </summary>
	public virtual void TakeTurn() {
		if (!dead) {
			turnsToSpecial--;
			PickTarget ();
			if (hasSpecial && turnsToSpecial <= 0) {
				StartCoroutine (SpecialAttack ());
				turnsToSpecial = specialInterval;
			} else {
				StartCoroutine (Attack ());
			}
		}
	}

	/// <summary>
	/// Excetures the enemy's standard attack
	/// </summary>
	protected virtual IEnumerator Attack() {
		animator.Play (Constants.SINGLE_ATTACK_ANIM_NAME);
		float timer = attackDelay/2;
		while(timer > 0) {
			timer -= Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
			if(Vector3.Distance(transform.position, target.transform.position + target.feetOffset - feetOffset) >= attackRange) {
				transform.position = Vector3.MoveTowards(transform.position, target.transform.position + target.feetOffset - feetOffset, Time.deltaTime * lungeSpeed);
			}
		}
		target.TakeDamage (attack);
		while (transform.position != homePosition) {
			transform.position = Vector3.MoveTowards(transform.position, homePosition, Time.deltaTime * lungeSpeed);
			yield return new WaitForSeconds(Time.deltaTime);
		}
	}

	/// <summary>
	/// Execute the enemy's special attack
	/// </summary>
	/// <returns>The attack</returns>
	protected virtual IEnumerator SpecialAttack() {
		animator.Play (Constants.SPECIAL_ATTACK_ANIM_NAME);
		yield return new WaitForSeconds (specialAttackDelay);
		target.TakeDamage (attack);
	}

	/// <summary>
	/// Updates health bar visual
	/// </summary>
	private void UpdateHealthBars() {
		currentHealth = Mathf.Clamp (currentHealth, 0.0f, health);
		healthBar.transform.localScale = new Vector3 (currentHealth / health, healthBar.transform.localScale.y, 0);
	}

	/// <summary>
	/// Takes the damage!
	/// </summary>
	/// <param name="d">d - how much damage is taken</param>
	public void TakeDamage(float d) {
		GameObject damageDisplay = Instantiate (floatingUI, transform.position, transform.rotation) as GameObject;
		if (d > 0) {
			damageDisplay.GetComponent<FloatingText>().Initialize(transform.position, (Mathf.RoundToInt(d)).ToString(), FloatingText.DisplayType.Damage);
		} else {
			damageDisplay.GetComponent<FloatingText>().Initialize(transform.position, (Mathf.RoundToInt(Mathf.Abs(d))).ToString(), FloatingText.DisplayType.Damage);
		}

		currentHealth -= d;
		UpdateHealthBars ();

		/// Play hurt animation or kill off enemy
		if (currentHealth > 0) {
			animator.Play(Constants.HURT_ANIM_NAME);
		} else {
			StartCoroutine(Die ());
		}
	}

	/// <summary>
	/// Kill off the enemy, resetting targetting and remove it from the game
	/// </summary>
	protected IEnumerator Die() {
		dead = true;
		gameManager.SelectTarget (null);
		animator.Play(Constants.DIE_ANIM_NAME);
		yield return new WaitForSeconds (deathAnimDuration);
		Destroy(gameObject);
	}
}
