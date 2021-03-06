﻿using UnityEngine;
using System.Collections;

public class ClearButton : MonoBehaviour {
	public Material green;
	public bool _isLocalPlayer;
	Material startupColor;
	// Use this for initialization
	void Start () {
		startupColor=GetComponent<MeshRenderer> ().material ;
	}
	
	// Update is called once per frame
	void Update () {
		if (_isLocalPlayer)SetToClear(true);
	}
	public void SetToClear (bool setting)
	{
		if (setting & GetComponent<MeshRenderer> ().material != green)
			GetComponent<MeshRenderer> ().material.color = Color.green;
		else
			GetComponent<MeshRenderer> ().material = startupColor;
	}
}
