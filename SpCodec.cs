using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public enum SpCodecMode {
	Read,
	Write,
}

public class SpCodec {
	private BinaryReader mReader;
	private BinaryWriter mWriter;
	private Stream mStream;

	public SpCodec (Stream stream, SpCodecMode mode) {
        mStream = stream;
		switch (mode) {
		case SpCodecMode.Read:
			mReader = new BinaryReader (stream);
			break;
		case SpCodecMode.Write:
			mWriter = new BinaryWriter (stream);
			break;
		}
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
		if (mReader == null)
			return null;

		// buildin type decoding should not be here
		if (SpTypeManager.IsBuildinType (type))
			return null;

		SpObject obj = new SpObject ();

		List<int> tags = new List<int> ();
		int current_tag = 0;

		short fn = mReader.ReadInt16 ();
		for (short i = 0; i < fn; i++) {
			int value = (int)mReader.ReadInt16 ();

			if (value == 0) {
				tags.Add (current_tag);
				current_tag++;
			}
			else {
				if (value % 2 == 0) {
					SpField f = type.GetFieldByTag (current_tag);
					if (f == null)
						return null;

					value = value / 2 - 1;
					if (f.Type == SpTypeManager.Instance.Integer) {
						obj.Insert (f.Name, value);
					}
					else if (f.Type == SpTypeManager.Instance.Boolean) {
						 obj.Insert (f.Name, (value == 0 ? false : true));
					}
					else {
						return null;
					}
					current_tag++;
				}
				else {
					current_tag += (value + 1) / 2;
				}
			}
		}

		foreach (int tag in tags) {
			SpField f = type.GetFieldByTag (tag);
			if (f == null)
				return null;

			if (f.IsArray) {
				int size = mReader.ReadInt32 ();

				if (f.Type == SpTypeManager.Instance.Integer) {
					byte n = mReader.ReadByte ();
					int count = (size - 1) / n;
					
					SpObject arr = new SpObject ();
					for (int i = 0; i < count; i++) {
						switch (n) {
						case 4:
							arr.Append (mReader.ReadInt32 ());
							break;
						case 8:
							arr.Append (mReader.ReadInt64 ());
							break;
						default:
							return null;
						}
					}
					obj.Insert (f.Name, arr);
				}
				else if (f.Type == SpTypeManager.Instance.Boolean) {
					SpObject arr = new SpObject ();
					for (int i = 0; i < size; i++) {
						arr.Append (mReader.ReadBoolean ());
					}
					obj.Insert (f.Name, arr);
				}
				else if (f.Type == SpTypeManager.Instance.String) {
					SpObject arr = new SpObject ();
					while (size > 0) {
						int str_len = mReader.ReadInt32 ();
						size -= 4;
						arr.Append (Encoding.UTF8.GetString(mReader.ReadBytes (str_len), 0, str_len));
						size -= str_len;
					}
					obj.Insert (f.Name, arr);
				}
				else if (f.Type == null) {
					// unknown type
					mReader.ReadBytes (size);
				}
				else {
					// TODO : nest type array
					mReader.ReadBytes (size);
				}
			}
			else {
				int size = mReader.ReadInt32 ();

				if (f.Type == SpTypeManager.Instance.Integer) {
					switch (size) {
					case 4:
						obj.Insert (f.Name, mReader.ReadInt32 ());
						break;
					case 8:
						obj.Insert (f.Name, mReader.ReadInt64 ());
						break;
					default:
						return null;
					}
				}
				else if (f.Type == SpTypeManager.Instance.Boolean) {
					// boolean should not be here
					return null;
				}
				else if (f.Type == SpTypeManager.Instance.String) {
					obj.Insert (f.Name, Encoding.UTF8.GetString(mReader.ReadBytes (size), 0, size));
				}
				else if (f.Type == null) {
					// unknown type
					mReader.ReadBytes (size);
				}
				else {
					obj.Insert (f.Name, Decode (f.Type));
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

    private int WriteInt (int n) {
        byte[] b = BitConverter.GetBytes (n);
        mStream.Write (b, 0, b.Length);
        return b.Length;
    }

    private int WriteString (string s) {
        byte[] b = Encoding.UTF8.GetBytes (s);
        int l = WriteInt (b.Length);
        mStream.Write (b, 0, b.Length);
        return (l + b.Length);
    }

    private int WriteBoolean (bool b) {
        byte n = 0;
        if (b)
            n = 1;
        return WriteByte (n);
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

		SpCodec codec = new SpCodec (stream, SpCodecMode.Write);
		codec.Encode (type, obj, false);
	}

	public static SpObject Decode (string proto, Stream stream) {
		SpType type = SpTypeManager.Instance.GetType (proto);
		if (type == null || stream == null)
			return null;

		SpCodec codec = new SpCodec (stream, SpCodecMode.Read);
		return codec.Decode (type);
	}

    private static bool IsTypeMatch (SpField f, SpObject o) {
        // TODO : type check
        if (f.IsArray != o.IsArray ()) {
            return false;
        }
        return true;
    }
}
