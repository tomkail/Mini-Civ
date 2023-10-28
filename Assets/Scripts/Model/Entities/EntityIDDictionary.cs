using UnityEngine;
using System;
using System.Collections.Generic;

// IDS are 1 indexed
[System.Serializable]
public class EntityIDDictionary {
	#if UNITY_EDITOR
    public 
	#endif
	Dictionary<Type, int> entityTypeIDIndexDictionary = new Dictionary<Type, int>();
	public EntityIDDictionary () {}
	protected EntityIDDictionary (EntityIDDictionary model) {
        entityTypeIDIndexDictionary = new Dictionary<Type, int>(model.entityTypeIDIndexDictionary);
    }
	public EntityIDDictionary Clone () {
		return new EntityIDDictionary(this);
	}
	public void ClearEntityDictionary () {
		entityTypeIDIndexDictionary.Clear();
	}
	
	public int ReserveEntityID (Type type) {
		if(!entityTypeIDIndexDictionary.ContainsKey(type))
			entityTypeIDIndexDictionary.Add(type, 0);
		entityTypeIDIndexDictionary[type]++;
		return entityTypeIDIndexDictionary[type];
	}
}
