using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum UnitRole
{
	MONSTER	= 0,
	IMPOSTER,
};

public class UnitController : StateMachineBase<UnitController>
{
	private Rigidbody2D m_rbBody;
	private Transform m_tfAvator;

	[SerializeField]
	private bool m_bIsLocalUnit;
	[SerializeField]
	private UnitRole m_eRole;

	[SerializeField]
	InputAction m_WASD;
	private Vector2 m_movementInput;
	[SerializeField]
	private float m_fMovementSpeed;

	private Animator m_animator;

	[SerializeField]
	private InputAction m_Kill;
	private float m_fKillRange;
	[SerializeField]
	private List<UnitController> m_targetUnitList = new List<UnitController>();
	[SerializeField]
	private Collider2D m_collider;

	[SerializeField]
	private bool m_bIsDead;

	[SerializeField]
	private OverrideSprite m_overrideSprite;
	[SerializeField]
	private GameObject m_prefDead;

	public static List<Transform> allBodies;
	private List<Transform> bodiesFound;

	[SerializeField]
	private InputAction m_Report;
	[SerializeField]
	private LayerMask m_ignoreLayerReport;

	[SerializeField]
	private CornVision2D m_cornVision2D;
	[SerializeField]
	private MeshFilter m_cornVisionMeshFilter;

	private void Awake()
	{
	}

	private void ReportBody(InputAction.CallbackContext obj)
	{
		Debug.Log($"allBodies:{allBodies.Count}");
		Debug.Log($"ReportBody:{bodiesFound.Count}");
		if ( bodiesFound == null)
		{
			return;
		}
		if (0 < bodiesFound.Count)
		{
			Transform tempBody = bodiesFound[bodiesFound.Count - 1];
			allBodies.Remove(tempBody);
			bodiesFound.Remove(tempBody);
			tempBody.GetComponent<UnitDead>().Report();
		}
	}

	private void OnEnable()
	{
		m_WASD.Enable();
		m_Kill.Enable();
		m_Report.Enable();
	}
	private void OnDisable()
	{
		m_WASD.Disable();
		m_Kill.Disable();
		m_Report.Disable();
	}

	private void Start()
	{
		m_rbBody = GetComponent<Rigidbody2D>();
		m_tfAvator = transform.GetChild(0);
		m_animator = GetComponent<Animator>();

		m_animator.SetFloat("dirX", 0.0f);
		m_animator.SetFloat("dirY", -1.0f);
		m_targetUnitList.Clear();

		allBodies = new List<Transform>();
		bodiesFound = new List<Transform>();

		m_cornVision2D.enabled = m_bIsLocalUnit;
		m_cornVisionMeshFilter.gameObject.SetActive(m_bIsLocalUnit);

		if (m_bIsLocalUnit)
		{
			m_Kill.performed += KillTarget;
			m_Report.performed += ReportBody;
			SetState(new Idle(this));
		}
	}
	protected override void OnUpdatePrev()
	{
		if (m_bIsLocalUnit)
		{
			m_movementInput = m_WASD.ReadValue<Vector2>();

			if (0 < allBodies.Count)
			{
				BodySearch();
			}
		}
	}

	private void KillTarget(InputAction.CallbackContext obj)
	{
		if( obj.phase == InputActionPhase.Performed)
		{
			if( m_targetUnitList.Count == 0)
			{
				return;
			}
			else
			{
				UnitController nearestUnit = null;
				float fNearLengthSqr = 0.0f;
				foreach( UnitController unit in m_targetUnitList)
				{
					if( unit.m_bIsDead)
					{
						continue;
					}
					float fLengthSqr = (transform.position - unit.transform.position).sqrMagnitude;
					if ( nearestUnit == null || fLengthSqr < fNearLengthSqr)
					{
						nearestUnit = unit;
						fNearLengthSqr = fLengthSqr;
					}
					else
					{
					}
				}

				if(nearestUnit != null)
				{
					nearestUnit.Die();
				}
			}
		}
	}
	public void Die()
	{
		m_bIsDead = true;
		m_collider.enabled = false;
		m_tfAvator.GetComponent<SpriteRenderer>().color = new Color(
			1.0f,1.0f,1.0f,0.5f);

		GameObject goDead = Instantiate(m_prefDead) as GameObject;
		goDead.GetComponent<UnitDead>().SetTexture(m_overrideSprite.overrideTexture);

		gameObject.layer = LayerMask.NameToLayer("Ghost");
	}
	private void BodySearch()
	{
		foreach(Transform body in allBodies)
		{
			RaycastHit2D[] hit2dArr;
			Ray2D ray2d = new Ray2D(transform.position, body.position - transform.position);
			Debug.DrawRay(
				transform.position,
				body.position - transform.position,
				Color.cyan );
			hit2dArr = Physics2D.RaycastAll(
				transform.position,
				body.position - transform.position,
				5.0f,
				~m_ignoreLayerReport);

			bool bFind = false;
			foreach(RaycastHit2D hit2d in hit2dArr)
			{
				if (hit2d.transform == body)
				{
					bFind = true;
					if (!bodiesFound.Contains(body.transform))
					{
						bodiesFound.Add(body.transform);
					}
				}
			}
			if( bFind == false)
			{
				bodiesFound.Remove(body.transform);
			}

		}

	}

	public void SetRole(UnitRole _eRole)
	{
		m_eRole = _eRole;
	}
	public void SetTexture( Texture _tex)
	{
		m_overrideSprite.overrideTexture = _tex;
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if( collision.tag == "Unit")
		{
			UnitController tempTarget = collision.GetComponent<UnitController>();
			if( m_eRole== UnitRole.IMPOSTER)
			{
				if( tempTarget.m_eRole == UnitRole.IMPOSTER )
				{
					return;
				}
				else
				{
					if(!m_targetUnitList.Contains(tempTarget))
					{
						m_targetUnitList.Add(tempTarget);
					}
				}
			}
		}
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Unit")
		{
			UnitController tempTarget = collision.GetComponent<UnitController>();
			if (m_targetUnitList.Contains(tempTarget))
			{
				m_targetUnitList.Remove(tempTarget);
			}
		}
	}

	private class Idle : StateBase<UnitController>
	{
		public Idle(UnitController _machine) : base(_machine)
		{
		}
		public override void OnUpdateState()
		{
			base.OnUpdateState();
			if (0 < machine.m_movementInput.sqrMagnitude)
			{
				machine.SetState(new Walk(machine));
			}
		}

		private class Walk : StateBase<UnitController>
		{
			public Walk(UnitController _machine) : base(_machine)
			{
			}
			public override void OnUpdateState()
			{
				base.OnUpdateState();

				if (0 == machine.m_movementInput.sqrMagnitude)
				{
					machine.SetState(new Idle(machine));
				}
				else
				{
					machine.m_animator.SetFloat("dirX", machine.m_movementInput.x);
					machine.m_animator.SetFloat("dirY", machine.m_movementInput.y);
				}
			}
			public override void OnFixedUpdateState()
			{
				machine.m_rbBody.velocity = machine.m_fMovementSpeed * machine.m_movementInput;
			}
			public override void OnExitState()
			{
				base.OnExitState();
				machine.m_rbBody.velocity = Vector2.zero;
			}
		}
	}
}
