using UnityEngine;
using System.Collections.Generic;

public class SpObject {
	private SpType mType;
	private object mValue;
	private Dictionary<int, SpObject> mChildrenByTag = new Dictionary<int, SpObject>();
	private Dictionary<string, SpObject> mChildrenByName = new Dictionary<string, SpObject>();

	public SpObject (string t) {
		mType = SpTypeManager.Instance.GetType (t);
	}

	public SpObject (SpType t) {
		mType = t;
	}

	public SpObject GetObject (string name) {
		if (mChildrenByName.ContainsKey (name) == false)
			return null;
		
		return mChildrenByName[name];
	}
	
	public SpObject GetOrCreateObject (string name) {
		if (mChildrenByName.ContainsKey (name) == false)
			return CreateObjectByName (name);
		
		return mChildrenByName[name];
	}

	private SpObject CreateObjectByName (string name) {
		if (mType == null)
			return null;
		
		SpField f = mType.GetFieldByName (name);
		if (f == null)
			return null;

		SpObject obj = new SpObject(f.Type);
		mChildrenByTag.Add (f.Tag, obj);
		mChildrenByName.Add (f.Name, obj);
		return obj;
	}
	
	public void Set (string name, object value) {
		SpObject obj = GetOrCreateObject (name);
		if (obj != null)
			obj.Value = value;
	}
	
	public SpObject GetObject (int tag) {
		if (mChildrenByTag.ContainsKey (tag) == false)
			return null;
		
		return mChildrenByTag[tag];
	}
	
	public SpObject GetOrCreateObject (int tag) {
		if (mChildrenByTag.ContainsKey (tag) == false)
			return CreateObjectByTag (tag);
		
		return mChildrenByTag[tag];
	}
	
	private SpObject CreateObjectByTag (int tag) {
		if (mType == null)
			return null;
		
		SpField f = mType.GetFieldByTag (tag);
		if (f == null)
			return null;
		
		SpObject obj = new SpObject(f.Type);
		mChildrenByTag.Add (f.Tag, obj);
		mChildrenByName.Add (f.Name, obj);
		return obj;
	}
	
	public void Set (int tag, object value) {
		SpObject obj = GetOrCreateObject (tag);
		if (obj != null)
			obj.Value = value;
	}

	public SpType Type {
		get { return mType; }
	}

	public object Value {
		get { return mValue; }
		set { mValue = value; }
	}

	public string Dump() {
		return Dump (0);
	}

	private string Dump(int tab) {
		string str = GetTab (tab) + mType.Name + "\n";
		foreach (SpField f in mType.Fields.Values) {
			SpObject obj = GetObject (f.Tag);
			if (obj == null)
				continue;

			if (f.TypeName.Equals ("integer") || f.TypeName.Equals ("boolean") || f.TypeName.Equals ("string")) {
				str += GetTab (tab + 1) + f.Name + "(" + obj.Value + ")\n";
			}
			else {
				str += obj.Dump (tab + 1);
			}
		}
		return str;
	}

	private string GetTab(int n) {
		string str = "";
		for (int i = 0; i < n; i++)
			str += "\t";
		return str;
	}
}
