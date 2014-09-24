using System.Collections.Generic;

public class SpType {
	public string Name;
	public Dictionary<int, SpField> Fields = new Dictionary<int, SpField> ();
	public Dictionary<string, SpField> FieldNames = new Dictionary<string, SpField> ();

	public SpType (string name) {
		Name = name;
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
