using System.Collections.Generic;
using System;

public class SpObject {
	public enum ArgType {
		Array,
		Table,
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
			for (int i = 0; i < args.Length; i++) {
				Insert (i, args[i]);
			}
			break;
		case ArgType.Table:
			for (int i = 0; i < args.Length; i += 2) {
				Insert (args[i], args[i + 1]);
			}
			break;
		}
    }

    public bool IsTable () {
        return (mType == ArgType.Table);
    }

	public Dictionary<object, SpObject> AsTable () {
		return mValue as Dictionary<object, SpObject>;
    }

	public void Insert (object key, SpObject obj) {
        if (IsTable () == false) {
            mType = ArgType.Table;
			mValue = new Dictionary<object, SpObject> ();
        }
        AsTable ()[key] = obj;
	}
	
	public void Insert (object key, object value) {
		if (value.GetType () == typeof (SpObject))
			Insert (key, (SpObject)value);
		else
			Insert (key, new SpObject (value));
	}

	public void Append (SpObject obj) {
		if (IsTable () == false) {
			mType = ArgType.Table;
			mValue = new Dictionary<object, SpObject> ();
		}
		Insert (AsTable ().Count, obj);
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

    public object Value { get { return mValue; } }

	public SpObject this[object key] {
		get {
			if (IsTable () == false)
				return null;
			Dictionary<object, SpObject> t = AsTable ();
			if (t.ContainsKey (key) == false)
				return null;
			return t[key];
		}
		set {
			if (IsTable () == false)
				return;
			Dictionary<object, SpObject> t = AsTable ();
			t[key] = value;
		}
	}
}
