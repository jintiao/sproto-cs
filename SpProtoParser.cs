using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public interface SpProtoParserListener {
	void OnNewType(SpType type);
    void OnNewProtocol (SpProtocol protocol);
}

public class SpProtoParser {
	private SpProtoParserListener mListener;
	private static char[] sDelimiters = new char[] {'{', '}', '\n'};
	private static char[] sSpace = new char[] {' ', '\t', '\n'};

    private SpProtocol mCurrentProtocol;
    private SpType mCurrentType;
    private SpType mLastType;

	public SpProtoParser (SpProtoParserListener linstener) {
		mListener = linstener;
	}

	public void Parse (Stream stream) {
		Parse (ReadAll (stream));
	}

	public void Parse (string str) {
		mCurrentProtocol = null;
		mCurrentType = null;
		mLastType = null;

		str = PreProcess (str);
		Scan (str, 0);
	}

	private string ReadAll (Stream stream) {
		string str = "";

		byte[] buf = new byte[1024];
		int len = stream.Read (buf, 0, buf.Length);
		while (len > 0)	{
			str += Encoding.UTF8.GetString (buf, 0, len);
			len = stream.Read (buf, 0, buf.Length);
		}

		return str;
	}

	private string PreProcess (string str) {
		// TODO : trim comment
		return str.Replace ("\r", string.Empty).Trim ();
	}

	private void Scan (string str, int start) {
		int pos = str.IndexOfAny (sDelimiters, start);
		if (pos < 0)
			return;

		switch (str[pos]) {
		case '{':
			string title = str.Substring (start, pos - start).Trim ();
            if (IsProtocol (title)) {
                mCurrentProtocol = NewProtocol (title);
			}
			else {
                mLastType = mCurrentType;
                mCurrentType = NewType (title);
			}
			break;
		case '}':
            if (mCurrentType != null) {
                mListener.OnNewType (mCurrentType);
                if (mCurrentProtocol != null)
                    mCurrentProtocol.AddType (mCurrentType);
                mCurrentType = mLastType;
                mLastType = null;
			}
            else if (mCurrentProtocol != null) {
                mListener.OnNewProtocol (mCurrentProtocol);
                mCurrentProtocol = null;
            }
			break;
		case '\n':
			SpField f = NewField (str.Substring(start, pos - start));
            if (f != null && mCurrentType != null) {
                mCurrentType.AddField (f);
			}
			break;
		}
		
		start = pos + 1;
		Scan (str, start);
	}

	private bool IsProtocol (string str) {
		return (str.IndexOfAny (sSpace) >= 0);
	}

    private SpProtocol NewProtocol (string str) {
        string[] words = str.Split (sSpace);
        if (words.Length != 2)
            return null;

        SpProtocol protocol = new SpProtocol (words[0], int.Parse (words[1]));
        return protocol;
    }

	private SpType NewType (string str) {
        if (str[0] == '.') {
            str = str.Substring (1);
        }
        else {
            if (mLastType != null)
                str = mLastType.Name + "." + str;

            if (mCurrentProtocol != null)
                str = mCurrentProtocol.Name + "." + str;
        }

		SpType t = new SpType (str);
		return t;
	}

	private SpField NewField (string str) {
		str = Regex.Replace (str, @"[\s:]+", " ").Trim ();
		string[] words = str.Split (' ');

		if (words.Length != 3)
			return null;

		string name = words[0];
        short tag = short.Parse (words[1]);
		string type = words[2];
		bool table = false;
		string key = null;
		if (type[0] == '*') {
			table = true;
			type = type.Substring (1);

			int b = type.IndexOf ('(');
			if (b > 0) {
				int e = type.IndexOf (')');
				key = type.Substring (b + 1, e - b - 1);
				type = type.Substring (0, b);
			}
		}
		SpField f = new SpField (name, tag, type, table, key, (SpTypeManager)mListener);
		return f;
	}
}
