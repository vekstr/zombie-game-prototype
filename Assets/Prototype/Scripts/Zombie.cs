using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public abstract class Zombie : MonoBehaviour
{
	[Header("Base Component References")]
	public Animator anim;
	public NavMeshAgent agent;

	[Header("Base Settings")]
	public int hitpoint;
	public float moveSpeed;
	public float startAttackRange;
	public float attackHitRange;
	public float attackHitRangeTolerance; // add range since collision shifts the object's origin
	[Space]
	public float afterAttackDelay;
	public float stunDuration;
	[Space]
	public float bodyRadius;

	new protected Transform transform;
	protected Transform playerTransform;
	protected PlayerController player;
	protected LevelManager levelManager;

	protected bool isMoving;
	protected bool isAttacking;

	protected virtual void Awake()
	{
		player = FindObjectOfType<PlayerController>();
		playerTransform = player.transform;
		transform = base.transform;
	}

	protected virtual void Start()
	{
		agent.speed = moveSpeed;
		StartCoroutine(UpdateAgentDestination());
		StartCoroutine(CheckForChanceToAttack());
	}

	protected virtual void FixedUpdate()
	{
		if (isMoving)
		{
			transform.position = agent.nextPosition;
		}

		agent.nextPosition = transform.position;
	}

	public virtual void Initialize(LevelManager levelManager, int extraHitpoints)
	{
		this.levelManager = levelManager;
		hitpoint += extraHitpoints;
		isMoving = true;
		isAttacking = false;
		agent.updatePosition = false;
		levelManager.AddZombie(this);
	}

	public virtual void Damage(int damage)
	{
		if (hitpoint != 0)
		{
			hitpoint = Mathf.Max(0, hitpoint - damage);

			if (hitpoint == 0)
			{
				Die();
			}
			else if (!isAttacking)
			{
				StartCoroutine(Stun());
			}
		}
	}

	protected virtual void Die()
	{
		levelManager.RemoveZombie(this);
		Destroy(gameObject);
	}

	private IEnumerator Stun()
	{
		isMoving = false;
		yield return new WaitForSeconds(stunDuration);
		isMoving = !isAttacking;
	}

	private IEnumerator Attack()
	{
		isMoving = false;
		isAttacking = true;
		anim.Play("Zombie_Eating");

		yield return null;
		yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

		isMoving = true;
		isAttacking = false;
		anim.Play("Zombie_Walk");

		if (CheckPlayerInsideRadius(attackHitRange + attackHitRangeTolerance))
		{
			player.Damage();
		}
	}

	private IEnumerator CheckForChanceToAttack()
	{
		var routineCheckWait = new WaitForSeconds(0.1f);
		var afterAttackWait = new WaitForSeconds(afterAttackDelay);

		while (hitpoint != 0)
		{
			if (CheckPlayerInsideRadius(startAttackRange))
			{
				yield return Attack();
				yield return afterAttackWait;
			}
			else
			{
				yield return routineCheckWait;
			}
		}
	}

	private IEnumerator UpdateAgentDestination()
	{
		var wait = new WaitForSeconds(0.2f);
		while (gameObject.activeSelf)
		{
			agent.SetDestination(playerTransform.position);
			yield return wait;
		}
	}

	protected bool CheckPlayerInsideRadius(float range)
	{
		var centerToCenterRange = player.bodyRadius * playerTransform.localScale.x + bodyRadius * transform.localScale.x + range;
		return Vector3.SqrMagnitude(playerTransform.position - transform.position) < centerToCenterRange * centerToCenterRange;
	}
}
