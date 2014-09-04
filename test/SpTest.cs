using System.Collections;
using System.IO;
using System.Diagnostics;
using System;

public class SpTest {
    public static void Run() {
        //string path = Application.dataPath + "/foobar.sproto";
        string path = "test/foobar.sproto";

        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
			SpTypeManager.Import (stream);
		}

		SpObject obj = new SpObject ("foobar");
		SpObject a = obj.GetOrCreateObject ("a");
		if (a != null)
			a.Value = "hello world";

		obj.Set (1, 999);

        Debug.Write(obj.Dump());

		using (MemoryStream stream = new MemoryStream ()) {
            long pos = stream.Position;
			SpCodec.Encode (obj, stream);
            stream.Position = pos;
            Dump (stream);
        }
	}

    private static void Dump(Stream stream) {
        byte[] buf = new byte[4];
        int count;

        while ((count = stream.Read(buf, 0, buf.Length)) > 0) {
            WriteLine(buf, count);
        }
    }

    private static void WriteLine (byte[] buf, int count) {
        for (int i = 0; i < count; i++) {
            Debug.Write ((i < count) ? String.Format ("{0:X2}", buf[i]) : "  ");
            Debug.Write (" ");
        }
        Debug.WriteLine (" ");
    }
}
