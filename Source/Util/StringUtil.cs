// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System.Linq;
using System.Text;

namespace SomaSim.Util
{
    /// <summary>
    /// Helper functions for strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Generates a stable string hash. Unlike String.GetHashCode, this one will
        /// produce the same results across platforms and 32 vs 64 bit architectures,
        /// however it is slower than String.GetHashCode.
        /// 
        /// The implementation is a variant of 32-bit FNV-1a that only uses
        /// the lower 8 bits of each 16-bit character: 
        /// http://isthe.com/chongo/tech/comp/fnv/#FNV-1a
        /// </summary>
        public static uint GetStableHashCode (this string str) {
            uint hash = 2166136261;
            foreach (var ch in str) {
                hash = hash ^ (byte) ch;
                hash = hash * 16777619;
            }
            return hash;
        }

        /// <summary> Either trims or pads the current string, to reach the desired length.
        /// If the string is already of correct length, it is returned unchanged. </summary>
        public static string TrimOrPad (this string str, int length, char paddingChar = ' ') =>
            length == 0 ? string.Empty :
            str.Length < length ? str.PadRight(length, paddingChar) :
            str.Length > length ? str.Substring(0, length) :
            str;

        /// <summary> Creates a new string array by appending an arbitrary number of additional strings </summary>
        public static string[] Append (this string[] arr, params string[] rest) =>
            arr.Concat(rest).ToArray();

        /// <summary> Works like String.Substring(0, len) but doesn't freak out when
        /// len is larger than actual string length. Optionally can add an ellipsis
        /// character to the trimmed string. </summary>
        public static string TrimToLength (this string str, int length, bool addEllipsis = false) {
            if (str.Length <= length) { return str; }
            var trimmed = str.Substring(0, length);
            if (addEllipsis) { trimmed += "…"; }
            return trimmed;
        }
    }

    /// <summary>
    /// Helper functions for string builders
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Returns the string value of the string builder, and also resets its length to zero.
        /// </summary>
        public static string ToStringAndReset (this StringBuilder sb) {
            var result = sb.ToString();
            sb.Length = 0;
            return result;
        }

        /// <summary>
        /// If the builder is not empty, appends specified string, otherwise doesn't do anything
        /// </summary>
        public static void AppendIfNotEmpty (this StringBuilder sb, string text) {
            if (sb.Length > 0) { sb.Append(text); }
        }

        /// <summary>
        /// If the builder is empty, appends specified string, otherwise doesn't do anything
        /// </summary>
        public static void AppendIfEmpty (this StringBuilder sb, string text) {
            if (sb.Length == 0) { sb.Append(text); }
        }

        /// <summary>
        /// If the builder is not empty, appends specified number of newlines, otherwise doesn't do anything
        /// </summary>
        public static void AppendLinesIfNotEmpty (this StringBuilder sb, int count) {
            if (sb.Length > 0) {
                for (int i = 0; i < count; i++) {
                    sb.AppendLine();
                }
            }
        }

        /// <summary>
        /// Appends given text using Append, followed by specified number of newlines.
        /// If the count == 1, this works just like regular AppendLine.
        /// </summary>
        public static void AppendLine (this StringBuilder sb, string text, int countNewlines) {
            sb.Append(text);
            sb.AppendLinesIfNotEmpty(countNewlines);
        }

        /// <summary>
        /// Returns the string value of the string builder and returns it to the pool.
        /// WARNING! This should only be used with builders acquired from the StringBuilderPool!
        /// Otherwise the behavior could be unpredictable.
        /// </summary>
        public static string ToStringAndReturnToPool (this StringBuilder sb) =>
            StringBuilderPool.Instance.ProduceValueAndFree(sb);
    }


    /// <summary>
    /// Canonical implementation of a FactoryPool that keeps a pool of StringBuilder instances for reuse.
    /// </summary>
    public sealed class StringBuilderPool
    {
        private FactoryPool<string, StringBuilder> _pool = new FactoryPool<string, StringBuilder>();

        static StringBuilderPool () {
            Reset();
        }

        public static StringBuilderPool Instance { get; private set; }

        public static StringBuilder AllocateInstance () => Instance.Allocate();

        public StringBuilder Allocate () {
            lock (_pool) { return _pool.Allocate(); }
        }

        public string ProduceValueAndFree (StringBuilder sb) {
            lock (_pool) { return _pool.ProduceValueAndFree(sb); }
        }

        public void Release () {
            lock (_pool) { _pool.Release(); }
        }

        public int UsedListSize => _pool.UsedListSize;
        public int FreeListSize => _pool.FreeListSize;

        public static void Reset () {
            Instance = new StringBuilderPool();
            Instance._pool.Initialize(
                () => new StringBuilder(),
                sb => sb.ToStringAndReset());
        }
    }

}
