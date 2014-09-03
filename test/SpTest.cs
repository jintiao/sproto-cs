using UnityEngine;
using System.Collections;
using System.IO;

public class SpTest {
	public static void Run () {
		using (FileStream stream = new FileStream (Application.dataPath + "/foobar.sproto", FileMode.Open)) {
			SpTypeManager.Import (stream);
		}

		SpObject obj = new SpObject ("foobar");
		SpObject a = obj.GetOrCreateObject ("a");
		if (a != null)
			a.Value = "hello world";

		obj.Set (1, 999);

		Debug.Log (obj.Dump ());

		using (MemoryStream stream = new MemoryStream ()) {
			SpCodec.Encode (obj, stream);
		}
	}
}
