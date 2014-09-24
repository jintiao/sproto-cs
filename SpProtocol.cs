using System.Collections.Generic;

public class SpProtocol {
    public string Name;
    public int Tag;
    public Dictionary<string, SpType> mTypes = new Dictionary<string, SpType> ();

    public SpProtocol (string name, int tag) {
        Name = name;
        Tag = tag;
    }

    public void AddType (SpType type) {
        mTypes.Add (type.Name, type);
    }
}
