using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class SpCodec {
    private Stream mStream;

    public SpCodec (Stream stream) {
        mStream = stream;
    }

    public void Encode (SpObject obj) {
        if (WriteBuildinObject (obj))
            return;

        long begin = mStream.Position;

        short tag = 0;
        short fn = 0;
        short dn = 0;
        List<SpObject> Objs = new List<SpObject> ();

        WriteInt (0);

        foreach (SpField f in obj.Type.Fields.Values) {
            SpObject o = obj.GetObject (f.Tag);
            if (o == null)
                continue;

            fn++;
            WriteShort ((short)(f.Tag - tag));
            tag = f.Tag;

            if (WriteEmbedObject (o) == false) {
                dn++;
                Objs.Add (o);
            }
        }

        foreach (SpObject o in Objs) {
            Encode (o);
        }

        long end = mStream.Position;
        mStream.Position = begin;
        WriteShort (fn);
        WriteShort (dn);
        mStream.Position = end;
 	}

    private void WriteShort (short n) {
        byte[] b = BitConverter.GetBytes (n);
        mStream.Write (b, 0, b.Length);
    }

    private void WriteInt (int n) {
        byte[] b = BitConverter.GetBytes (n);
        mStream.Write (b, 0, b.Length);
    }

    private void WriteString (string s) {
        WriteInt (s.Length);

        byte[] b = Encoding.UTF8.GetBytes (s);
        mStream.Write (b, 0, b.Length);
    }

    private void WriteBoolean (bool b) {
        int n = 1;
        if (b)
            n = 2;
        WriteInt (n);
    }

    private bool WriteEmbedObject (SpObject obj) {
        if (obj.Type.Name.Equals ("boolean")) {
            short n = 1;
            if (obj.GetBoolean ())
                n = 2;
            WriteShort (n);
            return true;
        }
        else if (obj.Type.Name.Equals ("integer")) {
            int i = obj.GetInt ();
            short n = (short)i;
            if (i == n) {
                WriteShort (n);
                return true;
            }
        }

        WriteShort (0);
        return false;
    }

    private bool WriteBuildinObject (SpObject obj) {
        if (obj.Type.Name.Equals ("boolean")) {
            WriteBoolean (obj.GetBoolean ());
        }
        else if (obj.Type.Name.Equals ("integer")) {
            WriteInt (obj.GetInt ());
        }
        else if (obj.Type.Name.Equals ("string")) {
            WriteString (obj.GetString ());
        }
        else {
            return false;
        }

        return true;
    }

    public static void Encode (SpObject obj, Stream stream) {
        SpCodec codec = new SpCodec (stream);
        codec.Encode (obj);
	}
}
