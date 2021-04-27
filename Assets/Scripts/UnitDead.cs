using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDead : MonoBehaviour
{
	[SerializeField]
	private OverrideSprite m_overrideSprite;
	public void SetTexture(Texture _tex)
	{
		m_overrideSprite.overrideTexture = _tex;
	}
	private void OnEnable()
	{
		if (UnitController.allBodies != null)
		{
			Debug.Log("Dead.OnEnable");
			UnitController.allBodies.Add(transform);
		}
	}
	public void Report()
	{
		Debug.Log("Report");
		Destroy(gameObject);
	}
}
