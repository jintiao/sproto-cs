using System.Collections.Generic;
using System;

public class SpObject {
	private object mValue;

    public SpObject (params object[] args) {
        if (args.Length == 0) {
            mValue = null;
        }
        else if (args.Length == 1) {
            mValue = args[0];
        }
        else {
            foreach (object arg in args) {
                Append (arg);
            }
        }
    }

    public bool IsTable () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (Dictionary<string, SpObject>));
    }

    public Dictionary<string, SpObject> AsTable () {
        return (Dictionary<string, SpObject>)mValue;
    }

    public SpObject Get (string key) {
        if (IsTable ()) {
            if (AsTable ().ContainsKey (key))
                return AsTable ()[key];
        }
        return null;
    }

    public void Insert (string key, SpObject obj) {
        if (IsTable () == false)
            mValue = new Dictionary<string, SpObject> ();
        AsTable ()[key] = obj;
	}
	
	public void Insert (string key, object value) {
		Insert (key, new SpObject (value));
	}

    public bool IsArray () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (List<SpObject>));
    }

    public List<SpObject> AsArray () {
        return (List<SpObject>)mValue;
    }

    public void Append (SpObject obj) {
        if (IsArray () == false)
            mValue = new List<SpObject> ();
        AsArray ().Add (obj);
    }

	public void Append (object value) {
		Append (new SpObject (value));
	}

    public bool IsLong () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (long));
    }

    public long AsLong () {
        if (IsLong ())
            return (long)mValue;
        return Convert.ToInt64 (mValue);
    }

    public bool IsInt () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (int));
    }

    public int AsInt () {
        return (int)mValue;
    }

    public bool IsBoolean () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (bool));
    }

    public bool AsBoolean () {
        return (bool)mValue;
    }

    public bool IsString () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (string));
    }

    public string AsString () {
        return (string)mValue;
    }

    public bool IsBuildinType () {
        return (IsLong () || IsInt () || IsBoolean () || IsString ());
    }

    public object Value { get { return mValue; } }
}
