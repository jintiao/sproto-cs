using System.Collections.Generic;
using System.IO;

public class SpTypeManager : SpProtoParserListener {
	private static SpTypeManager sInstance = new SpTypeManager ();

    private Dictionary<string, SpProtocol> mProtocols = new Dictionary<string, SpProtocol> ();
    private Dictionary<int, SpProtocol> mProtocolsByTag = new Dictionary<int, SpProtocol> ();

	private Dictionary<string, SpType> mTypes = new Dictionary<string, SpType> ();
	private Dictionary<string, SpType> mIncompleteTypes = new Dictionary<string, SpType> ();
	private SpType mTypeInteger;
	private SpType mTypeString;
	private SpType mTypeBoolean;

	public SpTypeManager () {
		mTypeInteger = new SpType ("integer");
		mTypeBoolean = new SpType ("boolean");
        mTypeString = new SpType ("string");

		OnNewType (mTypeInteger);
		OnNewType (mTypeString);
		OnNewType (mTypeBoolean);
	}

	public void OnNewType (SpType type) {
		if (GetType (type.Name) != null)
			return;

		if (IsTypeComplete (type))
			mTypes.Add (type.Name, type);
		else
			mIncompleteTypes.Add (type.Name, type);
	}

	public void OnNewProtocol (SpProtocol protocol) {
		if (GetProtocolByName (protocol.Name) != null)
			return;

        mProtocols.Add (protocol.Name, protocol);
        mProtocolsByTag.Add (protocol.Tag, protocol);
    }

	private bool IsTypeComplete (SpType type) {
		return type.CheckAndUpdate ();
	}

    public SpProtocol GetProtocolByName (string name) {
        if (mProtocols.ContainsKey (name)) {
            return mProtocols[name];
        }
        return null;
    }

    public SpProtocol GetProtocolByTag (int tag) {
        if (mProtocolsByTag.ContainsKey (tag)) {
            return mProtocolsByTag[tag];
        }
        return null;
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

	public SpType Integer { get { return mTypeInteger; } }
	public SpType String { get { return mTypeString; } }
	public SpType Boolean { get { return mTypeBoolean; } }

	public static SpTypeManager Instance {
		get { return sInstance; }
	}

	public static void Import (Stream stream) {
		new SpProtoParser (sInstance).Parse (stream);
	}

	public static void Import (string proto) {
		new SpProtoParser (sInstance).Parse (proto);
	}

	public static bool IsBuildinType (SpType type) {
		if (type == sInstance.mTypeInteger || type == sInstance.mTypeBoolean || type == sInstance.mTypeString)
			return true;
		return false;
	}
}
