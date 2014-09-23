using System.Collections.Generic;
using System;

public class SpObject {
	private object mValue;

    public SpObject () {
    }

	public SpObject (long l) {
		mValue = l;
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
	
	public void Insert (string key, long value) {
		Insert (key, new SpObject (value));
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

	public void Append (long value) {
		Append (new SpObject (value));
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

    public bool IsLong () {
        if (mValue == null)
            return false;
        return (mValue.GetType () == typeof (long));
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
        return (IsLong () || IsInt () || IsBoolean () || IsString ());
    }

    public bool ToBoolean () {
        return (bool)mValue;
    }

    public int ToInt () {
        return (int)mValue;
    }

    public long ToLong () {
        if (IsLong ())
            return (long)mValue;

        return Convert.ToInt64 (mValue);
    }

    public new string ToString () {
        return (string)mValue;
    }

    public List<SpObject> ToArray () {
        return (List<SpObject>)mValue;
    }

    public Dictionary<string, SpObject> ToTable () {
        return (Dictionary<string, SpObject>)mValue;
    }

    public object Value { get { return mValue; } }
}
