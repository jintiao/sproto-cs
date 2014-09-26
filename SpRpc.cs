using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class SpRpcDispatchInfo {
    public int Session;
    public SpType Type;
    public SpObject Object;

    public SpRpcDispatchInfo (int s, SpType t, SpObject o) {
        Session = s;
        Type = t;
        Object = o;
    }
}

public class SpRpc {
    private SpType mHeaderType;
    private Dictionary<int, SpType> mSessions = new Dictionary<int, SpType> ();

    public SpRpc (SpType header) {
        mHeaderType = header;
    }

	public SpStream Request (string proto) {
        return Request (proto, null);
    }

	public SpStream Request (string proto, SpObject args) {
        return Request (proto, args, 0);
    }

    public SpStream Request (string proto, SpObject args, int session) {
        SpProtocol protocol = SpTypeManager.Instance.GetProtocolByName (proto);
        if (protocol == null)
            return null;

        SpObject header = new SpObject ();
        header.Insert ("type", protocol.Tag);
        if (session != 0)
            header.Insert ("session", session);

        SpStream encode_stream = new SpStream ();
        SpCodec.Encode (mHeaderType, header, encode_stream);

        if (session != 0) {
            mSessions[session] = protocol.Response;
        }

        if (args != null) {
            SpCodec.Encode (protocol.Request, args, encode_stream);
        }

        SpStream pack_stream = new SpStream ();
        encode_stream.Position = 0;
        SpPacker.Pack (encode_stream, pack_stream);

        pack_stream.Position = 0;
        return pack_stream;
    }

	public SpStream Response (int session, SpObject args) {
        SpObject header = new SpObject ();
        header.Insert ("session", session);

        SpStream encode_stream = new SpStream ();
        SpCodec.Encode (mHeaderType, header, encode_stream);

        if (session != 0 && mSessions.ContainsKey (session)) {
            SpCodec.Encode (mSessions[session], args, encode_stream);
        }

        SpStream pack_stream = new SpStream ();
        encode_stream.Position = 0;
        SpPacker.Pack (encode_stream, pack_stream);

        pack_stream.Position = 0;
        return pack_stream;
    }

	public SpRpcDispatchInfo Dispatch (SpStream stream) {
        SpStream unpack_stream = new SpStream ();
        SpPacker.Unpack (stream, unpack_stream);

        unpack_stream.Position = 0;
        SpObject header = SpCodec.Decode (mHeaderType, unpack_stream);

        int session = 0;
        if (header["session"] != null)
            session = header["session"].AsInt ();

        if (header["type"] != null) {
            SpProtocol protocol = SpTypeManager.Instance.GetProtocolByTag (header["type"].AsInt ());
            if (session != 0) {
                mSessions[session] = protocol.Response;
            }
            SpObject obj = SpCodec.Decode (protocol.Request, unpack_stream);

            return new SpRpcDispatchInfo (session, protocol.Request, obj);
        }

        SpType response = mSessions[session];
        SpObject response_obj = null; ;
        if (response != null) {
            response_obj = SpCodec.Decode (response, unpack_stream);
        }

        return new SpRpcDispatchInfo (session, response, response_obj);
    }

    public static SpRpc Create (string proto) {
        SpType type = SpTypeManager.Instance.GetType (proto);
        if (type == null)
            return null;

        SpRpc rpc = new SpRpc (type);
        return rpc;
    }
}
