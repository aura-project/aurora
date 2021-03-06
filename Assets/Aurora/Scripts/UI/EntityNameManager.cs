﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityNameManager : MonoBehaviour
{
	public Transform EntityList;

	private Transform me;
	private Camera mainCamera;
	private GameObject reference;
	private Dictionary<Transform, Transform> nameTransforms = new Dictionary<Transform, Transform>();

	void Start()
	{
		me = transform;
		mainCamera = Camera.main;
		reference = me.FindChild("TxtReference").gameObject;
	}

	void Update()
	{
		// Go through all entities
		foreach (Transform entityTransform in EntityList)
		{
			// Get entity info component, ignore empty names
			var entityInfo = entityTransform.GetComponent<EntityInfo>();
			if (string.IsNullOrEmpty(entityInfo.Name))
				continue;

			// Get information for placement
			var nameTarget = entityTransform.FindChild("NameTarget");
			var pos = mainCamera.WorldToScreenPoint(nameTarget.transform.position);

			// Create new name object if new entity
			Transform nameTransform;
			if (!nameTransforms.TryGetValue(entityTransform, out nameTransform))
			{
				// Get creature
				Entity creature;
				Connection.Entities.TryGetValue(entityInfo.Id, out creature);

				// Create object in list
				var newNameObj = (GameObject)GameObject.Instantiate(reference, pos, Quaternion.identity);
				nameTransform = newNameObj.transform;
				nameTransform.SetParent(me);

				// Set colored name
				var color = MabiMath.GetNameColor(entityInfo.Name);
				var text = nameTransform.GetComponent<Text>();
				var name = entityInfo.Name;
				var displayName = string.Format("<color=#{0:X6}>{1}</color>", color, name);

				// NPC prefix
				if (entityInfo.IsConversationNpc)
					displayName = "<size=8>NPC</size> " + displayName;

				text.text = displayName;

				// Save for automatic removal
				nameTransforms.Add(entityTransform, nameTransform);
			}

			var nameObject = nameTransform.gameObject;

			// If entity is visible, activate name object and move it into
			// position.
			if (pos.z > 0)
			{
				nameTransform.position = pos;
				if (!nameObject.activeSelf)
					nameObject.SetActive(true);
			}
			// If entity is not visible deactivate the name.
			else if (nameObject.activeSelf)
				nameObject.SetActive(false);
		}

		// Check for removed entities
		var toRemove = new List<Transform>();
		foreach (var nameTransformX in nameTransforms)
		{
			// Remove name if entity transform became null (destroyed)
			var entityTransform = nameTransformX.Key;
			if (entityTransform == null)
			{
				var nameTransform = nameTransformX.Value;

				GameObject.Destroy(nameTransform.gameObject);
				toRemove.Add(entityTransform);
			}
		}

		// Remove destroyed entity's names from the list
		if (toRemove.Count != 0)
		{
			foreach (var key in toRemove)
				nameTransforms.Remove(key);
		}
	}
}
