﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErrH.Tools.ErrorConstructors;
using ErrH.Tools.FormatProviders;

namespace ErrH.Tools.Extensions
{
    public static class StringExtensions
    {

        public static byte[] Base64ToBytes(this string base64encodedText)
        {
            try { return Convert.FromBase64String(base64encodedText); }
            catch (Exception ex)
            {
                var msg = "Failed to convert from Base64-encoded string:" + L.f + base64encodedText.Truncate(30, "...");
                throw new InvalidCastException(msg, ex);
            }
        }



        /// <summary>
        /// from http://stackoverflow.com/a/6237866/3973863
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string JsonIndent(this string str)
        {
            const string INDENT_STRING = "    ";
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        private static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie) action(i);
        }


        /// <summary>
        /// From: HashLib 2.1 (Dec 29, 2013) Stable
        /// http://hashlib.codeplex.com/
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SHA1(this string text)
        {
            var algo = new HashLib.Crypto.SHA1();
            var res = algo.ComputeString(text, Encoding.UTF8);
            return res.ToString().ToLower();
        }


        public static string AlignLeft(this string text, int maxChars, string trimMarker = "...")
        {
            return (text.Length > maxChars) ? text.Truncate(maxChars, trimMarker)
                                            : text.PadRight(maxChars);
        }

        public static string AlignRight(this string text, int maxChars, string trimMarker = "...")
        {
            return (text.Length > maxChars) ? text.TruncateStart(maxChars, trimMarker)
                                            : text.PadLeft(maxChars);
        }


        //public static Uri AppendUri(this string baseUri, string relativeUri)
        //{
        //	var baseU = new Uri(baseUri, UriKind.Absolute);
        //	return new Uri(baseU, relativeUri);
        //}


        public static string Between(this string fullText,
                    string firstString, string lastString,
                    bool seekLastStringFromEnd = false)
        {
            if (fullText.IsBlank()) return string.Empty;

            int pos1 = fullText.IndexOf(firstString) + firstString.Length;
            if (pos1 == -1) return fullText;

            int pos2 = seekLastStringFromEnd ?
                fullText.LastIndexOf(lastString)
                : fullText.IndexOf(lastString);
            if (pos2 == -1 || pos2 <= pos1) return fullText;

            return fullText.Substring(pos1, pos2 - pos1);
        }



        public static string f(this string format, params object[] args)
        {
            if (args == null) return format;
            if (args.Length == 0) return format;

            return string.Format(new PluralFormatProvider(), format, args);
        }



        public static string Join(this string[] stringArray, string separator)
        { return string.Join(separator, stringArray); }


        public static bool IsBlank(this string text)
        {
            if (text == null) return true;
            return string.IsNullOrWhiteSpace(text);
        }



        public static bool SameAs(this string text1, string text2, StringComparison compareMode = StringComparison.CurrentCulture)
        {
            //return text1.ToLower() == text2.ToLower(); 
            return string.Compare(text1, text2, compareMode) == 0;
        }



        public static bool IsNumeric(this string text)
        {
            bool dotFoundAlready = false;

            foreach (char c in text.ToCharArray())
            {
                if (!char.IsDigit(c))
                {
                    if (c != '.') return false;

                    if (dotFoundAlready) return false;
                    dotFoundAlready = true;
                }
            }

            return true;
        }



        public static string ToTitleCase(this string text)
        {
            return new string(CharsToTitleCase(text).ToArray());
        }

        private static IEnumerable<char> CharsToTitleCase(string s)
        {
            bool newWord = true;
            foreach (char c in s)
            {
                if (newWord) { yield return Char.ToUpper(c); newWord = false; }
                else yield return Char.ToLower(c);
                if (c == ' ') newWord = true;
            }
        }


        public static int ToInt(this string text)
        { return int.Parse(text); }


        public static string Repeat(this string text, int count)
        { return string.Concat(Enumerable.Repeat(text, count)); }



        public static string Indent(this string text, int spaces = 4)
        {
            return new string(' ', spaces) + text;
        }



        public static string Guillemet(this string text,
            string openMark = "‹", string closeMark = "›")
        {
            return openMark + text + closeMark;
        }

        public static string Guillemets(this string text,
            string openMark = "«", string closeMark = "»")
        {
            return openMark + text + closeMark;
        }

        public static string Quotify(this string text,
            string openMark = "“", string closeMark = "”")
        {
            return openMark + text + closeMark;
        }



        public static string XmlValue(this string xmlFragment)
        {
            var sansOpenTag = xmlFragment.TextAfter(">");
            return sansOpenTag.TextBefore("<");
        }

        public static string XmlAttributes(this string xmlFragment)
        {
            var sansOpenTag = xmlFragment.Trim().TextAfter(" ");
            return sansOpenTag.TextBefore(">");
        }



        public static string TextUpTo(this string text, string findThis)
        {
            var pos = text.IndexOf(findThis);
            if (pos == -1) return text;

            return text.Substring(0, pos + findThis.Length);
        }

        public static string TextBefore(this string text, string findThis)
        {
            var pos = text.IndexOf(findThis);
            if (pos == -1) return text;

            return text.Substring(0, pos);
        }

        public static string TextAfter(this string text, string findThis, bool seekFromEnd = false)
        {
            var pos = seekFromEnd ? text.LastIndexOf(findThis)
                                  : text.IndexOf(findThis);
            if (pos == -1) return text;

            return text.Substring(pos + findThis.Length);
        }



        public static string Truncate(this string value, int maxLength, string truncatedMark = null)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length <= maxLength) return value;

            if (truncatedMark == null)
                return value.Substring(0, maxLength);
            else
                return value.Substring(0, maxLength - truncatedMark.Length) + truncatedMark;
        }


        public static string TruncateStart(this string text, int maxLength, string trimMarker)
        {
            var pos = text.Length - (maxLength - trimMarker.Length);
            return trimMarker + text.Substring(pos);
        }


        public static string MaxLength(this string text, int charLimit, string propertyName)
        {
            if (text.Length > charLimit)
                throw Error.MaxLength(charLimit, propertyName, text);

            return text;
        }



        public static List<string> SplitByLine(this string multiLineText)
        {
            var list = new List<string>();

            using (StringReader sr = new StringReader(multiLineText))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }

            return list;
        }


        public static string TrimStart(this string fullStr, string subStr)
        {
            if (!fullStr.StartsWith(subStr)) return fullStr;
            return fullStr.Substring(subStr.Length);
        }



        /// <summary>
        /// Appends sub URL to base URL adding slashes as needed.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <param name="between"></param>
        /// <returns></returns>
        public static string Slash
            (this string str1, object str2, string between = "/")
        {
            return StringSandwich(str1, between, str2);
        }

        public static string Bslash
            (this string str1, object str2, string between = @"\")
        {
            return StringSandwich(str1, between, str2);
        }

        private static string StringSandwich
            (string leftLoaf, string filling, object rightLoaf)
        {
            var s2 = (rightLoaf == null) ? "" : rightLoaf.ToString();
            if (!leftLoaf.EndsWith(filling)) leftLoaf += filling;
            if (s2.StartsWith(filling)) s2 = s2.TrimStart(filling);
            return leftLoaf + s2;
        }
    }
}
