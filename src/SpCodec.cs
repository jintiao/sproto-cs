using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SpCodec {
	private List<SpObject> mEmbedObjects = new List<SpObject>();
	private List<SpObject> mStandAloneObjects = new List<SpObject>();

	public void EncodeInternal(SpObject obj, Stream output) {
		/*
		foreach (SpField f in obj.Type.mFields.Values) {
			if (f.Type.Equals("integer")) {
			}
			else if (f.Type.Equals("string")) {
			}
			else if (f.Type.Equals("boolean")) {
			}
			else {
			}
		}*/
	}

	public static void Encode(SpObject obj, Stream output) {
		//new SpCodec().EncodeInternal(obj, output);
	}
}
