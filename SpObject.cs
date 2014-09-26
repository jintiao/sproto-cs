using System.Collections.Generic;
using System;

public class SpObject {
	public enum ArgType {
		Table,
		Array,
        Boolean,
        String,
        Int,
        Long,
        Null,
	}

	private object mValue;
    private ArgType mType;

	public SpObject () {
		mValue = null;
        mType = ArgType.Null;
	}

	public SpObject (object arg) {
        mValue = arg;
        mType = ArgType.Null;

        if (mValue != null) {
            Type t = mValue.GetType ();
            if (t == typeof (long)) {
                mType = ArgType.Long;
            }
            else if (t == typeof (int)) {
                mType = ArgType.Int;
            }
            else if (t == typeof (string)) {
                mType = ArgType.String;
            }
            else if (t == typeof (bool)) {
                mType = ArgType.Boolean;
            }
        }
	}

    public SpObject (ArgType type, params object[] args) {
        mType = ArgType.Null;

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
        return (mType == ArgType.Table);
    }

    public Dictionary<string, SpObject> AsTable () {
        return mValue as Dictionary<string, SpObject>;
    }

    public void Insert (string key, SpObject obj) {
        if (IsTable () == false) {
            mType = ArgType.Table;
            mValue = new Dictionary<string, SpObject> ();
        }
        AsTable ()[key] = obj;
	}
	
	public void Insert (string key, object value) {
		if (value.GetType () == typeof (SpObject))
			Insert (key, (SpObject)value);
		else
			Insert (key, new SpObject (value));
	}

    public bool IsArray () {
        return (mType == ArgType.Array);
    }

    public List<SpObject> AsArray () {
        return mValue as List<SpObject>;
    }

    public void Append (SpObject obj) {
        if (IsArray () == false) {
            mType = ArgType.Array;
            mValue = new List<SpObject> ();
        }
        AsArray ().Add (obj);
    }

	public void Append (object value) {
		if (value.GetType () == typeof (SpObject))
			Append ((SpObject)value);
		else
			Append (new SpObject (value));
	}

    public bool IsLong () {
        return (mType == ArgType.Long);
    }

    public long AsLong () {
        if (IsLong ())
            return (long)mValue;
        return Convert.ToInt64 (mValue);
    }

    public bool IsInt () {
        return (mType == ArgType.Int);
    }

    public int AsInt () {
        return (int)mValue;
    }

    public bool IsBoolean () {
        return (mType == ArgType.Boolean);
    }

    public bool AsBoolean () {
        return (bool)mValue;
    }

    public bool IsString () {
        return (mType == ArgType.String);
    }

    public string AsString () {
        return mValue as string;
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

            List<SpObject> a = AsArray ();
			if (a.Count <= index)
				return null;
			return a[index];
		}
		set {
			if (IsArray () == false)
				return;
			
			if (index < 0)
				return;

            List<SpObject> a = AsArray ();
			if (a.Count <= index)
				return;
			a[index] = value;
		}
	}
}
