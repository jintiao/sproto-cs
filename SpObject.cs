using System.Collections.Generic;

public class SpObject {
	private object mValue;

    public SpObject () {
    }

    public SpObject (int i) {
        mValue = i;
    }

    public SpObject (bool b) {
        mValue = b;
    }

	public SpObject (string s) {
        mValue = s;
	}

    public SpObject Get (string key) {
        if (IsTable ()) {
            Dictionary<string, SpObject> t = (Dictionary<string, SpObject>)mValue;
            if (t.ContainsKey (key))
                return t[key];
        }
        return null;
    }

    public void Insert (string key, SpObject obj) {
        if (mValue == null || mValue.GetType () != typeof (Dictionary<string, SpObject>))
            mValue = new Dictionary<string, SpObject> ();
        ((Dictionary<string, SpObject>)mValue)[key] = obj;
    }

    public void Insert (string key, int value) {
        Insert (key, new SpObject (value));
    }

    public void Insert (string key, bool value) {
        Insert (key, new SpObject (value));
    }

    public void Insert (string key, string value) {
        Insert (key, new SpObject (value));
    }

    public void Append (SpObject obj) {
        if (mValue == null || mValue.GetType () != typeof (List<SpObject>))
            mValue = new List<SpObject> ();
        ((List<SpObject>)mValue).Add (obj);
    }

    public void Append (int value) {
        Append (new SpObject (value));
    }

    public void Append (bool value) {
        Append (new SpObject (value));
    }

    public void Append (string value) {
        Append (new SpObject (value));
    }

    public bool IsTable () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (Dictionary<string, SpObject>));
    }

    public bool IsArray () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (List<SpObject>));
    }

    public bool IsInt () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (int));
    }

    public bool IsBoolean () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (bool));
    }

    public bool IsString () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (string));
    }

    public bool IsBuildinType () {
        return (IsInt () || IsBoolean () || IsString ());
    }

    public bool ToBoolean () {
        return (bool)mValue;
    }

    public int ToInt () {
        return (int)mValue;
    }

    public new string ToString () {
        return (string)mValue;
    }

    public List<SpObject> ToArray () {
        return (List<SpObject>)mValue;
    }

	public bool Match (SpObject obj) {
		return true;
	}
    
	public string Dump() {
		return Dump (0);
	}

	private string Dump(int tab) {
        string str = "";

        if (IsTable ()) {
            str = GetTab (tab) + "<table>\n";
            foreach (KeyValuePair<string, SpObject> entry in (Dictionary<string, SpObject>)mValue) {
                str += GetTab (tab + 1) + "<key : " + entry.Key + ">\n";
                str += entry.Value.Dump (tab + 1);
            }
        }
        else if (IsArray ()) {
            str = GetTab (tab) + "<array>\n";
            foreach (SpObject obj in (List<SpObject>)mValue) {
                str += obj.Dump (tab + 1);
            }
        }
        else if (IsBuildinType ()) {
            str = GetTab (tab) + mValue.ToString () + "\n";
        }

        return str;
    }

	private static string GetTab(int n) {
		string str = "";
		for (int i = 0; i < n; i++)
			str += "\t";
		return str;
	}
}
