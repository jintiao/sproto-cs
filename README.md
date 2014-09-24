sproto-cs
============

# Introduction
[sproto-cs](https://github.com/jintiao/sproto-cs) is a [sproto](https://github.com/cloudwu/sproto) library writen in c#.

# usage

### import `sproto` file

```c#
using (FileStream stream = new FileStream ("foobar.sproto", FileMode.Open)) {
  SpTypeManager.Import (stream);
}
```

### create `SpObject` and set data

```c#
SpObject obj = new SpObject (SpObject.ArgType.Table, 
    "a", "hello",
    "b", 1000000,
    "c", true,
    "d", new SpObject (SpObject.ArgType.Table,
            "a", "world",
            "c", -1),
    "e", new SpObject (SpObject.ArgType.Array, "ABC", "def"),
    "f", new SpObject (SpObject.ArgType.Array, -3, -2, -1, 0, 1, 2),
    "g", new SpObject (SpObject.ArgType.Array, true, false, true),
    "h", new SpObject (SpObject.ArgType.Array,
            new SpObject (SpObject.ArgType.Table, "b", 100),
            new SpObject (),
            new SpObject (SpObject.ArgType.Table, "b", -100, "c", false),
            new SpObject (SpObject.ArgType.Table, "b", 0, "e", new SpObject (SpObject.ArgType.Array, "test")))
		);
```

### encode

```c#
MemoryStream encode_stream = new MemoryStream ();
SpCodec.Encode ("foobar", obj, encode_stream);
```

### pack

```c#
MemoryStream pack_stream = new MemoryStream ();
SpPacker.Pack (encode_stream, pack_stream);
```

### unpack

```c#
MemoryStream decode_stream = new MemoryStream ();
SpPacker.Unpack (pack_stream, decode_stream);
```

### decode

```c#
SpObject newObj = SpCodec.Decode ("foobar", decode_stream);
```

### data operation

```c#
Util.Assert (obj["a"].AsString ().Equals ("hello"));
Util.Assert (obj["b"].AsInt () == 1000000);
Util.Assert (obj["c"].AsBoolean () == true);
Util.Assert (obj["d"]["a"].AsString ().Equals ("world"));
Util.Assert (obj["d"]["c"].AsInt () == -1);
Util.Assert (obj["e"][0].AsString ().Equals ("ABC"));
Util.Assert (obj["e"][1].AsString ().Equals ("def"));
Util.Assert (obj["f"][0].AsInt () == -3);
Util.Assert (obj["f"][1].AsInt () == -2);
Util.Assert (obj["f"][2].AsInt () == -1);
Util.Assert (obj["f"][3].AsInt () == 0);
Util.Assert (obj["f"][4].AsInt () == 1);
Util.Assert (obj["f"][5].AsInt () == 2);
Util.Assert (obj["g"][0].AsBoolean () == true);
Util.Assert (obj["g"][1].AsBoolean () == false);
Util.Assert (obj["g"][2].AsBoolean () == true);
Util.Assert (obj["h"][0]["b"].AsInt () == 100);
Util.Assert (obj["h"][1].Value == null);
Util.Assert (obj["h"][2]["b"].AsInt () == -100);
Util.Assert (obj["h"][2]["c"].AsBoolean () == false);
Util.Assert (obj["h"][3]["b"].AsInt () == 0);
Util.Assert (obj["h"][3]["e"][0].AsString ().Equals ("test"));
```

# more examples
[sproto-u3d](https://github.com/jintiao/sproto-u3d)
[sproto-vcs](https://github.com/jintiao/sproto-vcs)
