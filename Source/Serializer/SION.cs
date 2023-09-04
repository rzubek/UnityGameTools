// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SomaSim.SION
{
    /// <summary>
    /// Thrown when the parser encountered unexpected data, typically because of an invalid serialized form.
    /// </summary>
    public class SIONException : Exception
    {
        private string _context;

        public override string Message => base.Message + "\n" + _context;

        internal SIONException (Exception e, SION.ParseContext c, string message) : base(message, e) { SetContext(c); }
        internal SIONException (SION.ParseContext c, string message) : base(message) { SetContext(c); }

        internal SIONException (Exception e, SION.PrintContext c, string message) : base(message, e) { SetContext(c); }
        internal SIONException (SION.PrintContext c, string message) : base(message) { SetContext(c); }

        private void SetContext (SION.ParseContext c) {
            char[] buffer = new char[128];
            int read = c.reader.ReadBlock(buffer, 0, buffer.Length);
            _context = $"Parse Context: {((read > 0) ? new string(buffer) : "(???)")}";
        }

        private void SetContext (SION.PrintContext c) {
            string tail = c.sb.ToString();
            tail = tail[Math.Max(0, tail.Length - 20)..];
            _context = $"Print Context: {tail}";
        }
    }


    /// <summary>
    /// Parser and printer for the .sim file format
    /// </summary>
    public class SION
    {
        public class GlobalParseSettings
        {
            // processes any macro strings found inside the parsed file.
            // if not overriden by user, the default behavior is to return the input as a string.
            public Func<string, object> MacroProcessor = s => s;
        }

        public class GlobalPrintSettings
        {
            // if >= 0, specifies max number of digits allowed after the decimal point when printed.
            public int MaxFloatDoubleDecimalDigits = -1;

            // amount of intenting used when writing files out
            public int IndentSpaces = 2;
        }

        internal class ParseContext
        {
            public readonly TextReader reader;
            public readonly StringBuilder sb;

            public ParseContext (TextReader reader) {
                this.reader = reader;
                this.sb = new StringBuilder();
            }

            public string GetStringAndResetBuilder () {
                string result = sb.ToString();
                sb.Length = 0;
                return result;
            }
        }

        private const char TOKEN_LIST_START = '[';
        private const char TOKEN_LIST_END = ']';
        private const char TOKEN_DICTIONARY_START = '{';
        private const char TOKEN_DICTIONARY_END = '}';
        private const char TOKEN_MACRO_START = '(';
        private const char TOKEN_MACRO_END = ')';
        private const char TOKEN_BOOL_OR_NULL_START = '#';
        private const char TOKEN_COMMENT = ';';
        private const char TOKEN_STRING_DELIMITER = '"';
        private const char TOKEN_VERBATIM_START = '\'';

        private const string DEFAULT_LINEBREAK = "\n";

        private const char DIGIT_FIRST = '0', DIGIT_LAST = '9'; // 0..9 in chars/ascii
        private const char CHAR_PLUS = '+', CHAR_MINUS = '-', CHAR_DOT = '.', CHAR_UNDERSCORE = '_';
        private const char CHAR_UP_FIRST = 'A', CHAR_UP_LAST = 'Z'; // A..Z and a..z in chars/ascii
        private const char CHAR_LO_FIRST = 'a', CHAR_LO_LAST = 'z';

        private static bool IsStartOfNumber (char ch) => // 0..9 + -
            (ch >= DIGIT_FIRST && ch <= DIGIT_LAST) || (ch == CHAR_PLUS) || (ch == CHAR_MINUS);

        private static bool IsBodyOfNumber (char ch) => // 0..9 .
            (ch >= DIGIT_FIRST && ch <= DIGIT_LAST) || (ch == CHAR_DOT);

        private static bool IsStartOfSimpleString (char ch) => // a..z A..Z _
            (ch >= CHAR_UP_FIRST && ch <= CHAR_UP_LAST) ||
            (ch >= CHAR_LO_FIRST && ch <= CHAR_LO_LAST) ||
            (ch == CHAR_UNDERSCORE);

        private static bool IsBodyOfSimpleString (char ch) => // a..z A..Z 0..9 _ - .
            (ch >= CHAR_UP_FIRST && ch <= CHAR_UP_LAST) ||
            (ch >= CHAR_LO_FIRST && ch <= CHAR_LO_LAST) ||
            (ch >= DIGIT_FIRST && ch <= DIGIT_LAST) ||
            (ch == CHAR_UNDERSCORE) || (ch == CHAR_MINUS) || (ch == CHAR_DOT);

        private static CultureInfo CultureInfoInvariantCulture = CultureInfo.InvariantCulture;

        public static GlobalPrintSettings PrintSettings = new GlobalPrintSettings();
        public static GlobalParseSettings ParseSettings = new GlobalParseSettings();

        #region Helpers

        private static class ArrayListPool
        {
            private static List<ArrayList> _s_pool = new List<ArrayList>();

            public static ArrayList Allocate () {
                int count = _s_pool.Count;
                if (count == 0) { return new ArrayList(16); }
                var last = _s_pool[count - 1];
                _s_pool.RemoveAt(count - 1);
                return last;
            }

            public static void Free (ArrayList list) {
                list.Clear();
                _s_pool.Add(list);
            }

            public static void Flush () => _s_pool.Clear();
        }

        #endregion

        #region Parsing a serialized form

        public static object Parse (string text) {
            var result = Parse(new StringReader(text));
            ArrayListPool.Flush();
            return result;
        }

        public static object Parse (TextReader reader) {
            var result = Parse(new ParseContext(reader));
            ArrayListPool.Flush();
            return result;
        }

        private static object Parse (ParseContext c) {
            ConsumeIgnored(c);
            object value = ParseValue(c);
            ConsumeIgnored(c);
            return value;
        }

        private static object ParseValue (ParseContext c) {
            int next = c.reader.Peek();
            if (next == -1) {
                return null; // nothing to get
            }

            char ch = (char) next;

            if (ch == TOKEN_BOOL_OR_NULL_START) {
                return ParseBoolOrNull(c);

            } else if (ch == TOKEN_LIST_START) {
                return ParseList(c);

            } else if (ch == TOKEN_DICTIONARY_START) {
                return ParseDictionary(c);

            } else if (ch == TOKEN_MACRO_START) {
                return ParseMacro(c);

            } else if (ch == TOKEN_STRING_DELIMITER || ch == TOKEN_VERBATIM_START) {
                return ParseQuotedString(c);

            } else if (IsStartOfNumber(ch)) {
                return ParseNumber(c);

            } else if (IsStartOfSimpleString(ch)) {
                return ParseSimpleString(c);

            } else {
                throw new SIONException(c, string.Format("Unexpected character {0} / {1:x8}", ch, next));
            }
        }


        //
        //
        // parse primitives

        private static object ParseBoolOrNull (ParseContext c) {
            char magic = (char) c.reader.Read(); // consume the magic character
            string token = ConsumeWhile(c, IsBodyOfSimpleString, false).ToLowerInvariant(); // pull the next characters
            switch (token) {
                case "true": return true;
                case "false": return false;
                case "null": return null;
                default: return magic + token; // just return it as is
            }
        }

        private static object ParseNumber (ParseContext c) {
            string token = ConsumeWhile(c, IsBodyOfNumber, true);
            try {
                return double.Parse(token, CultureInfoInvariantCulture);
            } catch (FormatException e) {
                throw new SIONException(e, c, "Unknown number format in " + token);
            } catch (Exception e) {
                throw new SIONException(e, c, "Failed to parse as number " + token);
            }
        }

        private static object ParseSimpleString (ParseContext c) =>
            ConsumeWhile(c, IsBodyOfSimpleString, true);

        private static object ParseQuotedString (ParseContext c) {
            char quote = (char) c.reader.Read();
            bool verbatim = (quote == TOKEN_VERBATIM_START);

            do {
                if (c.reader.Peek() == -1) {
                    throw new SIONException(c, "String not terminated with value " + c.GetStringAndResetBuilder());
                }

                char ch = (char) c.reader.Read();
                if (ch == quote) {              // found a matching closing delimiter
                    return c.GetStringAndResetBuilder();
                }

                if (ch == '\\' && !verbatim && c.reader.Peek() != -1) {
                    ch = TranslateEscapedChar((char) c.reader.Read());   // take the next character and see what we need to do with it
                }

                c.sb.Append(ch);

            } while (true);
        }

        private static object ParseMacro (ParseContext c) {
            char opener = TOKEN_MACRO_START, closer = TOKEN_MACRO_END;
            int depth = 0;

            do {
                if (c.reader.Peek() == -1) {
                    throw new SIONException(c, "Macro not terminated with value " + c.GetStringAndResetBuilder());
                }

                char ch = (char) c.reader.Read();
                c.sb.Append(ch);

                if (ch == opener) { depth++; }
                if (ch == closer) { depth--; }

                if (ch == closer && depth <= 0) {
                    // found a matching closing delimiter - process the macro and return
                    var result = c.GetStringAndResetBuilder();
                    return ParseSettings.MacroProcessor(result);
                }

            } while (true);
        }

        private static char TranslateEscapedChar (char c) {
            switch (c) {
                case 'r': return (char) 0x0d; // cr
                case 'n': return (char) 0x0a; // lf
                default: return c;
            }
        }

        //
        //
        // parse collections

        private static object ParseList (ParseContext c) {
            c.reader.Read(); // consume opening token
            return ParseAsListUntil(c, TOKEN_LIST_END, new ArrayList(8));
        }

        private static object ParseDictionary (ParseContext c) {
            c.reader.Read(); // consume opening token

            ArrayList pairs = ArrayListPool.Allocate();
            ParseAsListUntil(c, TOKEN_DICTIONARY_END, pairs);
            if (pairs.Count % 2 == 1) {
                string hint = String.Concat(pairs[0]);
                throw new SIONException(c, "Dictionary needs an even number of elements (keys and values) for dictionary starting with " + hint);
            }

            // now unzip the list into a dictionary and return it
            Hashtable results = new Hashtable(pairs.Count / 2);
            for (int i = 0; i < pairs.Count; i += 2) {
                object key = pairs[i];
                object val = pairs[i + 1];
                results[key] = val;
            }

            ArrayListPool.Free(pairs);
            return results;
        }

        private static ArrayList ParseAsListUntil (ParseContext c, char terminator, ArrayList results) {
            while (true) {
                ConsumeIgnored(c);

                if (c.reader.Peek() == -1) {
                    string hint = (results.Count > 0) ? String.Concat(results[0]) : "???";
                    throw new SIONException(c, "Collection not terminated correctly, starting with " + hint);
                }

                if ((char) c.reader.Peek() == terminator) {
                    c.reader.Read(); // consume the terminating token
                    return results;
                }

                results.Add(Parse(c));
            }
        }

        //
        //
        // parse helpers

        private static string ConsumeWhile (ParseContext c, Predicate<char> pred, bool consumeFirstChar) {
            var reader = c.reader;
            var sb = c.sb;

            if (consumeFirstChar) {
                sb.Append((char) reader.Read()); // used when we've already dispatched on the first character
            }

            do {    // now process remaining ones based on the test
                int next = reader.Peek();
                if (next == -1 || !pred.Invoke((char) next)) {
                    return c.GetStringAndResetBuilder();
                }

                sb.Append((char) reader.Read());
            } while (true);
        }

        private static void ConsumeIgnored (ParseContext c) {
            do {
                int next = c.reader.Peek();
                if (next == -1) {
                    return; // no more
                }

                char ch = (char) next;
                bool isWhiteSpace = ch == ' ' || (next < 0x20); // all control codes are treated as whitespace

                if (isWhiteSpace) {
                    c.reader.Read(); // consume one whitespace character
                } else if (ch == TOKEN_COMMENT) {
                    ConsumeComment(c);
                } else {
                    return; // we're done, don't consume this character
                }
            } while (true);
        }

        private static void ConsumeComment (ParseContext c) =>
            // consume everything until the end of the line,
            // including the starting comment token, but excluding newline
            c.reader.ReadLine();

        #endregion

        #region Serializing out data structures

        internal class PrintContext
        {
            public readonly StringBuilder sb;
            public readonly bool compressed;
            public int indent;

            public PrintContext (bool compressed) {
                this.sb = new StringBuilder();
                this.compressed = compressed;
                this.indent = 0;
            }

            public void IndentMore () { if (!compressed) { this.indent += PrintSettings.IndentSpaces; } }
            public void IndentLess () { if (!compressed) { this.indent -= PrintSettings.IndentSpaces; } }
        }

        public static string Print (object data) => Print(data, false);

        public static string Print (object data, bool compressed) {
            PrintContext c = new PrintContext(compressed);
            Print(data, c);
            return c.sb.ToString();
        }

        private static void Print (object data, PrintContext c) {
            bool done = PrintString(data, c) ||
                        PrintPrimitive(data, c) ||
                        PrintList(data, c) ||
                        PrintDictionary(data, c);
            if (!done) {
                throw new SIONException(c, "Don't know how to print object: " + data);
            }
        }

        private static bool PrintString (object data, PrintContext c) {
            if (data is not string s) {
                return false;
            }

            // if it's a simple string, it'll be easy
            if (PrintSimpleString(s, c)) {
                return true;
            }

            // not a simple string, do all the escaping and stuff
            c.sb.Append(TOKEN_STRING_DELIMITER);
            foreach (char ch in s) {
                switch (ch) {
                    case '\\': c.sb.Append("\\"); break;
                    case '"': c.sb.Append("\\\""); break;
                    case '\n': c.sb.Append("\\n"); break;
                    case '\r': c.sb.Append("\\r"); break;
                    default: c.sb.Append(ch); break;
                }
            }
            c.sb.Append(TOKEN_STRING_DELIMITER);
            return true;
        }

        private static bool PrintSimpleString (string s, PrintContext c) {
            if (s.Length == 0) {
                return false;
            }

            for (int i = 0; i < s.Length; ++i) {
                //string validchars = (i == 0) ? SIMPLE_STRING_START : SIMPLE_STRING_BODY;
                //bool isNotValid = validchars.IndexOf(s[i]) < 0;
                bool validchar = (i == 0) ? IsStartOfSimpleString(s[i]) : IsBodyOfSimpleString(s[i]);
                bool isNotValid = !validchar;

                if (isNotValid) { return false; }
            }

            // it's a simple string, just output as is
            c.sb.Append(s);
            return true;
        }

        private static bool PrintPrimitive (object data, PrintContext c) {
            if (data == null) {
                c.sb.Append(TOKEN_BOOL_OR_NULL_START);
                c.sb.Append("null");
                return true;
            }

            if (!(data is ValueType)) {
                return false; // quick shortcut
            }

            if (data is bool bdata) {
                c.sb.Append(TOKEN_BOOL_OR_NULL_START);
                c.sb.Append(bdata ? "true" : "false");
                return true;
            }

            if (data is float || data is double) {
                c.sb.Append(FormatFloatingPoint(data));
                return true;
            }

            if (data is byte || data is short || data is ushort ||
                data is int || data is uint || data is long || data is ulong) {
                c.sb.Append(string.Format(CultureInfoInvariantCulture, "{0:D}", data));
                return true;
            }

            return false;
        }

        private static string FormatFloatingPoint (object data) {
            // since sion doesn't support scientific notation, we want to make sure not to print it

            string candidate = "0";

            if (data is float f) {
                candidate = f.ToString("G", CultureInfoInvariantCulture);
                if (candidate.Contains("E")) {
                    candidate = f.ToString("F7", CultureInfoInvariantCulture).TrimEnd('0'); // 7 digits per c# spec for general float format
                }
            }

            if (data is double d) {
                candidate = d.ToString("G", CultureInfoInvariantCulture);
                if (candidate.Contains("E")) {
                    candidate = d.ToString("F15", CultureInfoInvariantCulture).TrimEnd('0'); // 15 digits per c# spec for general double format
                }
            }

            // if contains too many digits after decimal, trim
            int maxdecimals = PrintSettings.MaxFloatDoubleDecimalDigits;
            if (maxdecimals >= 0) {
                int dotpos = candidate.IndexOf('.');
                if (dotpos >= 0) {
                    int decimals = candidate.Length - (1 + dotpos);
                    if (decimals > maxdecimals) {
                        int newlength = candidate.Length - (decimals - maxdecimals);
                        candidate = candidate.Substring(0, newlength).TrimEnd('0').TrimEnd('.');
                    }
                }
            }

            if (candidate.Length >= 2 && candidate[^1] == '.') { // ugly hackery
                candidate = candidate.TrimEnd('.');
            }

            return candidate;
        }

        private static bool PrintList (object data, PrintContext c) {
            if (data is not IList list) {
                return false;
            }

            bool complex = ContainsComplexHashtable(list);

            c.sb.Append(TOKEN_LIST_START);
            c.IndentMore();

            for (int i = 0, end = list.Count - 1; i <= end; ++i) {
                // maybe insert an element separator
                if (i > 0) {
                    AddNewlineOrSpaceSeparator(c, complex);
                }
                Print(list[i], c);
            }

            c.IndentLess();
            c.sb.Append(TOKEN_LIST_END);
            return true;
        }

        private static bool PrintDictionary (object data, PrintContext c) {
            if (data is not IDictionary dict) {
                return false;
            }

            bool complex = IsComplexHashtable(dict);

            c.sb.Append(TOKEN_DICTIONARY_START);
            c.IndentMore();

            if (complex) {
                AddNewlineOrSpaceSeparator(c, true);
            }

            int i = 0, end = dict.Count - 1;
            foreach (DictionaryEntry entry in dict) {
                Print(entry.Key, c);
                c.sb.Append(" ");
                Print(entry.Value, c);

                if (i++ < end) {
                    AddNewlineOrSpaceSeparator(c, complex);
                }
            }

            c.IndentLess();
            c.sb.Append(TOKEN_DICTIONARY_END);
            return true;
        }

        private static void AddNewlineOrSpaceSeparator (PrintContext c, bool line) {
            if (line) {
                c.sb.Append(DEFAULT_LINEBREAK);
                c.sb.Append(' ', c.indent);
            } else {
                c.sb.Append(' ');
            }
        }

        private static bool ContainsComplexHashtable (IList list) {
            foreach (var element in list) {
                if (IsComplexHashtable(element)) { return true; }
            }
            return false;
        }

        private static bool IsComplexHashtable (object data) {
            if (data is not IDictionary dict) {
                return false;
            }

            if (dict.Values.Count > 3) {
                return true;
            }

            foreach (var value in dict.Values) {
                if (value is IDictionary dictValue && !IsComplexHashtable(dictValue)) {
                    continue;
                }
                if (value is ValueType || value is string) {
                    continue;
                }
                return true;
            }

            return false;
        }

        #endregion
    }
}

