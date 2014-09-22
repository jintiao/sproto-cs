using System.Collections.Generic;
using System.IO;

public class SpTypeManager : SpProtoParserListener {
	private static SpTypeManager sInstance = new SpTypeManager ();

	private Dictionary<string, SpType> mTypes = new Dictionary<string, SpType> ();
	private Dictionary<string, SpType> mIncompleteTypes = new Dictionary<string, SpType> ();

	public SpTypeManager () {
		OnNewType (new SpType ("integer", null));
		OnNewType (new SpType ("boolean", null));
		OnNewType (new SpType ("string", null));
	}

	public void OnNewType (SpType type) {
		if (IsTypeComplete (type))
			mTypes.Add (type.Name, type);
		else
			mIncompleteTypes.Add (type.Name, type);
	}

	private bool IsTypeComplete (SpType type) {
		return type.CheckAndUpdate ();
	}

	public SpType GetType (string name) {
		if (mTypes.ContainsKey (name)) {
			return mTypes[name];
		}

		if (mIncompleteTypes.ContainsKey (name)) {
			SpType t = mIncompleteTypes[name];
			if (IsTypeComplete (t)) {
				mIncompleteTypes.Remove (name);
				mTypes.Add (name, t);
			}
			return t;
		}

		return null;
	}

	public SpType GetTypeNoCheck (string name) {
		if (mTypes.ContainsKey (name)) {
			return mTypes[name];
		}

		if (mIncompleteTypes.ContainsKey (name)) {
			return mIncompleteTypes[name];
		}

		return null;
	}

	public static SpTypeManager Instance {
		get { return sInstance; }
	}

	public static void Import (Stream stream) {
		new SpProtoParser (sInstance).Parse (stream);
	}

    public static bool IsBuildinType (string type) {
        if (type.Equals ("integer") || type.Equals ("boolean") || type.Equals ("string"))
            return true;
        return false;
    }

	public static bool IsBuildinType (SpType type) {
		return IsBuildinType (type.Name);
	}
}
