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

    public bool Encode (SpType type, SpObject obj) {
        if (mWriter == null)
            return false;

        // buildin type decoding should not be here
        if (SpTypeManager.IsBuildinType (type))
            return false;

        long begin = mStream.Position;

        // fn. will be update later
        short fn = 0;
        mWriter.Write (fn);

        List<KeyValuePair<SpObject, SpType>> objs = new List<KeyValuePair<SpObject, SpType>> ();
        int current_tag = -1;

        foreach (SpField f in type.Fields.Values) {
            if (f == null)
                return false;

            SpObject o = obj.Get (f.Name);
            if (o == null || IsTypeMatch (f, o) == false)
                continue;

            if (f.Tag <= current_tag)
                return false;

            if (f.Tag - current_tag > 1) {
                mWriter.Write ((short)(2 * (f.Tag - current_tag - 1) - 1));
                fn++;
            }

            bool standalone = true;
            if (f.IsArray == false) {
                if (f.Type == SpTypeManager.Instance.Boolean) {
                    int value = o.AsBoolean () ? 1 : 0;
                    mWriter.Write ((short)((value + 1) * 2));
                    standalone = false;
                }
                else if (f.Type == SpTypeManager.Instance.Integer) {
                    int value = o.AsInt ();
                    if (value >= 0 && value < 0x7fff) {
                        mWriter.Write ((short)((value + 1) * 2));
                        standalone = false;
                    }
                }
            }

            if (standalone) {
                objs.Add (new KeyValuePair<SpObject, SpType> (o, f.Type));
                mWriter.Write ((short)0);
            }

            fn++;
            current_tag = f.Tag;
        }

        foreach (KeyValuePair<SpObject, SpType> entry in objs) {
            if (entry.Key.IsArray ()) {
                long array_begin = mStream.Position;
                int size = 0;
                mWriter.Write (size);

                if (entry.Value == SpTypeManager.Instance.Integer) {
                    byte len = 4;
                    foreach (SpObject o in entry.Key.AsArray ()) {
                        if (o.IsLong ()) {
                            len = 8;
                            break;
                        }
                    }

                    mWriter.Write (len);
                    foreach (SpObject o in entry.Key.AsArray ()) {
                        if (len == 4) {
                            mWriter.Write (o.AsInt ());
                        }
                        else {
                            mWriter.Write (o.AsLong ());
                        }
                    }
                }
                else if (entry.Value == SpTypeManager.Instance.Boolean) {
                    foreach (SpObject o in entry.Key.AsArray ()) {
                         mWriter.Write ((byte)(o.AsBoolean () ? 1 : 0));
                    }
                }
                else if (entry.Value == SpTypeManager.Instance.String) {
                    foreach (SpObject o in entry.Key.AsArray ()) {
                        byte[] b = Encoding.UTF8.GetBytes (o.AsString ());
                        mWriter.Write (b.Length);
                        mWriter.Write (b, 0, b.Length);
                    }
                }
                else {
                    foreach (SpObject o in entry.Key.AsArray ()) {
                        long obj_begin = mStream.Position;
                        int obj_size = 0;
                        mWriter.Write (obj_size);

                        if (Encode (entry.Value, o) == false)
                            return false;

                        long obj_end = mStream.Position;
                        obj_size = (int)(obj_end - obj_begin - 4);
                        mStream.Position = obj_begin;
                        mWriter.Write (obj_size);
                        mStream.Position = obj_end;
                    }
                }

                long array_end = mStream.Position;
                size = (int)(array_end - array_begin - 4);
                mStream.Position = array_begin;
                mWriter.Write (size);
                mStream.Position = array_end;
            }
            else {
                if (entry.Key.IsString ()) {
                    byte[] b = Encoding.UTF8.GetBytes (entry.Key.AsString ());
                    mWriter.Write (b.Length);
                    mWriter.Write (b, 0, b.Length);
                }
                else if (entry.Key.IsInt ()) {
                    mWriter.Write ((int)4);
                    mWriter.Write (entry.Key.AsInt ());
                }
                else if (entry.Key.IsLong ()) {
                    mWriter.Write ((int)8);
                    mWriter.Write (entry.Key.AsLong ());
                }
                else if (entry.Key.IsBoolean ()) {
                    // boolean should not be here
                    return false;
                }
                else {
                    long obj_begin = mStream.Position;
                    int obj_size = 0;
                    mWriter.Write (obj_size);

                    if (Encode (entry.Value, entry.Key) == false)
                        return false;

                    long obj_end = mStream.Position;
                    obj_size = (int)(obj_end - obj_begin - 4);
                    mStream.Position = obj_begin;
                    mWriter.Write (obj_size);
                    mStream.Position = obj_end;
                }
            }
        }

        long end = mStream.Position;
        mStream.Position = begin;
        mWriter.Write (fn);
        mStream.Position = end;

        return true;
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
            int value = (int)mReader.ReadUInt16 ();

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
                    SpObject arr = new SpObject ();
                    while (size > 0) {
                        int obj_len = mReader.ReadInt32 ();
                        size -= 4;
                        arr.Append (Decode (f.Type));
                        size -= obj_len;
                    }
                    obj.Insert (f.Name, arr);
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

    private static bool IsTypeMatch (SpField f, SpObject o) {
        if (f == null || f.Type == null || o == null)
            return false;

        if (f.IsArray) {
            if (o.IsArray ())
                return true;
        }
        else if (f.Type == SpTypeManager.Instance.String) {
            if (o.IsString ())
                return true;
        }
        else if (f.Type == SpTypeManager.Instance.Boolean) {
            if (o.IsBoolean ())
                return true;
        }
        else if (f.Type == SpTypeManager.Instance.Integer) {
            if (o.IsInt () || o.IsLong ())
                return true;
        }
        else {
            if (o.IsTable ())
                return true;
        }

        return false;
    }

    public static bool Encode (string proto, SpObject obj, Stream stream) {
        return Encode (SpTypeManager.Instance.GetType (proto), obj, stream);
	}

    public static bool Encode (SpType type, SpObject obj, Stream stream) {
        if (type == null || obj == null|| stream == null)
            return false;

        SpCodec codec = new SpCodec (stream, SpCodecMode.Write);
        return codec.Encode (type, obj);
    }

	public static SpObject Decode (string proto, Stream stream) {
        return Decode (SpTypeManager.Instance.GetType (proto), stream);
	}

    public static SpObject Decode (SpType type, Stream stream) {
        if (type == null || stream == null)
            return null;

        SpCodec codec = new SpCodec (stream, SpCodecMode.Read);
        return codec.Decode (type);
    }
}
