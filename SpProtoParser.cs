using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public interface SpProtoParserListener {
	void OnNewType(SpType type);
}

public class SpProtoParser {
	private SpProtoParserListener mListener;
	private static char[] sDelimiters = new char[] {'{', '}', '\n'};
	private static char[] sSpace = new char[] {' ', '\t', '\n'};

	public SpProtoParser (SpProtoParserListener linstener) {
		mListener = linstener;
	}

	public void Parse (Stream stream) {
		string str = ReadAll (stream);
		str = PreProcess (str);
		Scan (str, 0, null);
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
		return str.Replace ("\r", string.Empty).Trim ();
	}

	private void Scan (string str, int start, SpType scope) {
		int pos = str.IndexOfAny (sDelimiters, start);
		if (pos < 0)
			return;

		switch (str[pos]) {
		case '{':
			string title = str.Substring (start, pos - start);
			if (IsProtocol (title)) {
			}
			else {
				SpType t = NewType (title, scope);
				scope = t;
			}
			break;
		case '}':
			if (scope != null) {
				mListener.OnNewType (scope);
				scope = scope.ParentScope;
			}
			break;
		case '\n':
			SpField f = NewField (str.Substring(start, pos - start));
			if (f != null && scope != null) {
				scope.AddField (f);
			}
			break;
		}
		
		start = pos + 1;
		Scan (str, start, scope);
	}

	private bool IsProtocol (string str) {
		return false;
		//return (str.IndexOfAny (sSpace) >= 0);
	}

	private SpType NewType (string str, SpType scope) {
		str = str.Trim ();
		if (str[0] == '.')
			str = str.Substring (1);

		SpType t = new SpType (str, scope);
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
		bool array = false;
		if (type[0] == '*') {
			array = true;
			type = type.Substring (1);
		}
		SpField f = new SpField (name, tag, type, array);
		return f;
	}
}
