using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class SpCodec {
    private Stream mStream;

    public SpCodec (Stream stream) {
        mStream = stream;
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

    private int WriteArray (SpType type, List<SpObject> array) {
        long begin = mStream.Position;
        WriteInt (0);

        foreach (SpObject o in array) {
            Encode (type, o, true);
        }

        long end = mStream.Position;
        mStream.Position = begin;
        WriteInt ((int)(end - begin - 4));
        mStream.Position = end;
        return (int)(end - begin);
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
        int n = 0;
        if (b)
            n = 1;
        return WriteInt (n);
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
        SpCodec codec = new SpCodec (stream);
        codec.Encode (SpTypeManager.Instance.GetType (proto), obj, false);
	}

    private static bool IsTypeMatch (SpField f, SpObject o) {
        // TODO : type check
        return true;
    }
}
