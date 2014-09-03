using UnityEngine;
using System.Collections.Generic;

public class SpType {
	public string Name;
	public string Fullname;
	public SpType ParentScope;
	public Dictionary<int, SpField> Fields = new Dictionary<int, SpField> ();
	public Dictionary<string, SpField> FieldNames = new Dictionary<string, SpField> ();

	public SpType (string name, SpType scope) {
		Name = name;
		if (scope != null)
			Fullname = scope.Fullname + "." + name;
		else
			Fullname = Name;
		ParentScope = scope;
	}

	public void AddField (SpField f) {
		Fields.Add(f.Tag, f);
		FieldNames.Add(f.Name, f);
	}
	
	public SpField GetFieldByName (string name) {
		if (FieldNames.ContainsKey(name))
			return FieldNames[name];
		return null;
	}
	
	public SpField GetFieldByTag (int tag) {
		if (Fields.ContainsKey(tag))
			return Fields[tag];
		return null;
	}

	public bool CheckAndUpdate () {
		bool complete = true;
		foreach (SpField f in Fields.Values) {
			if (f.CheckAndUpdate ())
				continue;
			complete = false; 
		}
		return complete;
	}
}
