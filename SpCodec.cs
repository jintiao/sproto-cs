using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class SpCodec {
    private Stream mStream;
	private byte[] mBuffer;

    public SpCodec (Stream stream) {
        mStream = stream;
	}

	public SpCodec (Stream stream, int len) {
		mStream = stream;
		mBuffer = new byte[len];
	}

    public int Encode (SpType type, SpObject obj, bool writeLength) {
        int len = WriteBuildinObject (type, obj);
        if (len > 0)
            return len;

        long begin = mStream.Position;

        short tag = -1;
        short fn = 0;
        List<KeyValuePair<SpObject, SpType>> objs = new List<KeyValuePair<SpObject, SpType>> ();

        if (writeLength)
            WriteInt (0);
        // skip header
        WriteShort (0);

        foreach (SpField f in type.Fields.Values) {
            SpObject o = obj.Get (f.Name);
            if (o == null || IsTypeMatch (f, o) == false)
                continue;

            fn++;

            if (f.Tag <= tag) {
                // error handle
            }

            if (f.Tag - tag != 1) {
                WriteTag (f.Tag - tag - 1);
                fn++;
            }

            if (WriteEmbedObject (o) == false) {
                objs.Add (new KeyValuePair<SpObject, SpType> (o, f.Type));
                WriteShort (0);
            }

            tag = f.Tag;
        }

        foreach (KeyValuePair<SpObject, SpType> entry in objs) {
            if (entry.Key.IsArray ()) {
                WriteArray (entry.Value, entry.Key.ToArray ());
            }
            else {
                Encode (entry.Value, entry.Key, true);
            }
        }

        long end = mStream.Position;
        mStream.Position = begin;

        if (writeLength)
            WriteInt ((int)(end - begin - 4));
        WriteShort (fn);
        mStream.Position = end;
        return (int)(end - begin);
 	}

	public SpObject Decode (SpType type) {
		if (SpTypeManager.IsBuildinType (type))
			return null;

		SpObject obj = new SpObject ();

		int fn = ReadShort ();

		List<int> tags = new List<int> ();
		int tag = 0;
		for (int i = 0; i < fn; i++) {
			int v = ReadShort ();
			if (v == 0) {
				tags.Add (tag);
			}
			else {
				if (v % 2 == 0) {
					int value = v / 2 - 1;
					SpField f = type.GetFieldByTag (tag);
					if (f == null)
						return null;

					if (f.TypeName.Equals ("integer")) {
						obj.Insert (f.Name, value);
					}
					else if (f.TypeName.Equals ("boolean")) {
						bool b = (value == 0 ? false : true);
						obj.Insert (f.Name, b);
					}
					else {
						return null;
					}
				}
				else {
					tag += (v + 1) / 2 - 1;
				}
			}
			tag++;
		}

		foreach (int t in tags) {
			SpField f = type.GetFieldByTag (t);
			if (f == null)
				return null;

			if (f.Array) {
				int size = ReadInt ();

				if (f.TypeName.Equals ("integer")) {
					ReadByte ();
					size -= 1;
					
					SpObject arr = new SpObject ();
					while (size > 0) {
						arr.Append (ReadInt ());
						size -= 4;
					}
					obj.Insert (f.Name, arr);
				}
				else if (f.TypeName.Equals ("boolean")) {
					SpObject arr = new SpObject ();
					while (size > 0) {
						arr.Append (ReadBoolean ());
						size -= 1;
					}
					obj.Insert (f.Name, arr);
				}
				else if (f.TypeName.Equals ("string")) {
				
					SpObject arr = new SpObject ();

					while (size > 0) {
						int slen = ReadInt ();
						size -= 4;
						arr.Append (ReadString (slen));
						size -= slen;
					}
					obj.Insert (f.Name, arr);
				}
				else {
					if (f.Type == null) {
						mStream.Read (mBuffer, 0, size);
					}
					else {
						// TODO : nest type array
						mStream.Read (mBuffer, 0, size);
					}
				}
			}
			else {
				int len = ReadInt ();

				if (f.TypeName.Equals ("integer")) {
					switch (len) {
					case 4:
						int nnn = ReadInt ();
						obj.Insert (f.Name, nnn);
						break;
					}
				}
				else if (f.TypeName.Equals ("boolean")) {
					obj.Insert (f.Name, ReadBoolean ());
				}
				else if (f.TypeName.Equals ("string")) {
					obj.Insert (f.Name, ReadString (len));
				}
				else {
					if (f.Type == null) {
						mStream.Read (mBuffer, 0, len);
					}
					else {
						SpObject o = Decode (f.Type);
						obj.Insert (f.Name, o);
					}
				}
			}
		}

		return obj;
	}

    private int WriteArray (SpType type, List<SpObject> array) {
        long begin = mStream.Position;
        WriteInt (0);

        if (type.Name.Equals ("integer")) {
            // TODO : detect number size
            WriteByte (4);
            foreach (SpObject o in array) {
                WriteInt (o.ToInt ());
            }
        }
        else if (type.Name.Equals ("boolean")) {
            foreach (SpObject o in array) {
                WriteBoolean (o.ToBoolean ());
            }
        }
        else {
            foreach (SpObject o in array) {
                Encode (type, o, true);
            }
        }

        long end = mStream.Position;
        mStream.Position = begin;
        WriteInt ((int)(end - begin - 4));
        mStream.Position = end;
        return (int)(end - begin);
    }

    private int WriteByte (byte n) {
        mStream.WriteByte (n);
        return 1;
    }

	private int ReadByte () {
		return mStream.ReadByte ();
	}

    private int WriteShort (short n) {
        byte[] b = BitConverter.GetBytes (n);
        mStream.Write (b, 0, b.Length);
        return b.Length;
    }

	private int ReadShort () {
		mStream.Read (mBuffer, 0, 2);
		return BitConverter.ToInt16 (mBuffer, 0);
	}

    private int WriteInt (int n) {
        byte[] b = BitConverter.GetBytes (n);
        mStream.Write (b, 0, b.Length);
        return b.Length;
    }

	private int ReadInt () {
		mStream.Read (mBuffer, 0, 4);
		return BitConverter.ToInt32 (mBuffer, 0);
	}

    private int WriteString (string s) {
        byte[] b = Encoding.UTF8.GetBytes (s);
        int l = WriteInt (b.Length);
        mStream.Write (b, 0, b.Length);
        return (l + b.Length);
    }

	private string ReadString (int len) {
		if (len > mBuffer.Length)
			mBuffer = new byte[len];
		mStream.Read (mBuffer, 0, len);
		return Encoding.UTF8.GetString (mBuffer, 0, len);
	}

    private int WriteBoolean (bool b) {
        byte n = 0;
        if (b)
            n = 1;
        return WriteByte (n);
    }

	private bool ReadBoolean () {
		int n = mStream.ReadByte ();
		if (n != 0)
			return true;
		return false;
	}

    private bool WriteEmbedObject (SpObject obj) {
        if (obj.IsBoolean ()) {
            short n = 0;
            if (obj.ToBoolean ())
                n = 1;
            WriteEmbedValue (n);
            return true;
        }
        else if (obj.IsInt ()) {
            int n = obj.ToInt ();
            if (n >= 0 && n < 0x7fff) {
                WriteEmbedValue (n);
                return true;
            }
        }

        return false;
    }

    private int WriteBuildinObject (SpType type, SpObject obj) {
        int len = 0;

        if (obj.IsBoolean ()) {
            len += WriteInt (4);
            len += WriteBoolean (obj.ToBoolean ());
        }
        else if (obj.IsInt ()) {
            len += WriteInt (4);
            len += WriteInt (obj.ToInt ());
        }
        else if (obj.IsString ()) {
            len += WriteString (obj.ToString ());
        }

        return len;
    }

    private void WriteTag (int gap) {
        WriteShort ((short)(2 * gap - 1));
    }

    private void WriteEmbedValue (int n) {
        WriteShort ((short)((n + 1) * 2));
    }

    public static void Encode (string proto, SpObject obj, Stream stream) {
		SpType type = SpTypeManager.Instance.GetType (proto);
		if (type == null)
			return;

        SpCodec codec = new SpCodec (stream);
		codec.Encode (type, obj, false);
	}

	public static SpObject Decode (string proto, Stream stream) {
		SpType type = SpTypeManager.Instance.GetType (proto);
		if (type == null)
			return null;

		SpCodec codec = new SpCodec (stream, 64);
		return codec.Decode (type);
	}

    private static bool IsTypeMatch (SpField f, SpObject o) {
        // TODO : type check
        if (f.Array != o.IsArray ()) {
            return false;
        }
        return true;
    }
}
