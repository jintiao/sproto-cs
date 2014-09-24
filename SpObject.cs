using System.Collections.Generic;
using System;

public class SpObject {
	public enum ArgType {
		Table,
		Array,
	}

	private object mValue;

	public SpObject () {
		mValue = null;
	}

	public SpObject (object arg) {
		mValue = arg;
	}

	public SpObject (ArgType type, params object[] args) {
		switch (type) {
		case ArgType.Array:
			foreach (object arg in args) {
				Append (arg);
			}
			break;
		case ArgType.Table:
			for (int i = 0; i < args.Length; i += 2) {
				Insert ((string)args[i], args[i + 1]);
			}
			break;
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

    public void Insert (string key, SpObject obj) {
        if (IsTable () == false)
            mValue = new Dictionary<string, SpObject> ();
        AsTable ()[key] = obj;
	}
	
	public void Insert (string key, object value) {
		if (value.GetType () == typeof (SpObject))
			Insert (key, (SpObject)value);
		else
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
		if (value.GetType () == typeof (SpObject))
			Append ((SpObject)value);
		else
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

	public SpObject this[string key] {
		get {
			if (IsTable () == false)
				return null;
			Dictionary<string, SpObject> t = AsTable ();
			if (t.ContainsKey (key) == false)
				return null;
			return t[key];
		}
		set {
			if (IsTable () == false)
				return;
			Dictionary<string, SpObject> t = AsTable ();
			t[key] = value;
		}
	}

	public SpObject this[int index] {
		get {
			if (IsArray () == false)
				return null;

			if (index < 0)
				return null;

			if (AsArray ().Count <= index)
				return null;
			return AsArray ()[index];
		}
		set {
			if (IsArray () == false)
				return;
			
			if (index < 0)
				return;

			if (AsArray ().Count <= index)
				return;
			AsArray ()[index] = value;
		}
	}
}
