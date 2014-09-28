using System.Collections;

public class SpField {
	public string Name;
	public SpType Type;
    public string TypeName;
    public short Tag;
	public bool IsArray;

    private SpTypeManager mTypeManager;

	public SpField (string name, short tag, string type, bool array, SpTypeManager m) {
		Name = name;
		Tag = tag;
		TypeName = type;
		IsArray = array;

        mTypeManager = m;
	}

	public bool CheckAndUpdate () {
		if (Type != null)
			return true;

		// use GetTypeNoCheck instead of GetType, to prevent infinit GetType call
		// when a type reference itself like : foobar { a 0 : foobar }
        Type = mTypeManager.GetTypeNoCheck (TypeName);
		return (Type != null);
	}
}
