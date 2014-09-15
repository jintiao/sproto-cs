using System.Collections;
using System.IO;
using System.Diagnostics;
using System;

public class SpTest {
    public static void Run () {
        //string path = Application.dataPath + "/foobar.sproto";
        string path = "test/foobar.sproto";

        using (FileStream stream = new FileStream (path, FileMode.Open)) {
            SpTypeManager.Import (stream);
        }

        SpObject obj = new SpObject ();
        obj.Insert ("a", "hello");
        obj.Insert ("b", 1000000);
        obj.Insert ("c", true);

        SpObject d = new SpObject ();
        d.Insert ("a", "world");
        d.Insert ("c", -1);
        obj.Insert ("d", d);
        
        SpObject e = new SpObject ();
        e.Append("ABC");
        e.Append("def");
        obj.Insert ("e", e);
        
        SpObject f = new SpObject ();
        f.Append(-3);
        f.Append(-2);
        f.Append(-1);
        f.Append(0);
        f.Append(1);
        f.Append(2);
        obj.Insert ("f", f);

        SpObject g = new SpObject ();
        g.Append (true);
        g.Append (false);
        g.Append (true);
        obj.Insert ("g", g);

        SpObject h = new SpObject ();
        {
            SpObject t = new SpObject ();
            t.Insert ("b", 100);
            h.Append (t);
        }
        {
            SpObject t = new SpObject ();
            h.Append (t);
        }
        {
            SpObject t = new SpObject ();
            t.Insert ("b", -100);
            t.Insert ("c", false);
            h.Append (t);
        }
        {
            SpObject t = new SpObject ();
            t.Insert ("b", 0);

            SpObject he = new SpObject ();
            he.Append ("test");
            t.Insert ("e", he);
            h.Append (t);
        }
        obj.Insert ("h", h);

        Debug.Write (obj.Dump ());

        using (MemoryStream stream = new MemoryStream ()) {
            long pos = stream.Position;
            SpCodec.Encode ("foobar", obj, stream);
            stream.Position = pos;
            Dump (stream);
        }
    }

    private static void Dump(Stream stream) {
        byte[] buf = new byte[16];
        int count;

        while ((count = stream.Read(buf, 0, buf.Length)) > 0) {
            WriteLine(buf, count);
        }
    }

    private static void WriteLine (byte[] buf, int count) {
        for (int i = 0; i < count; i++) {
            Debug.Write ((i < count) ? String.Format ("{0:X2}", buf[i]) : "  ");
            Debug.Write ((i > 0) && (i < count - 1) &&((i + 1) % 8 == 0) ? " - " : " ");
        }
        Debug.WriteLine (" ");
    }
}
