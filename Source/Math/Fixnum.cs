// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SomaSim.Util
{
    /// <summary> Fixnum implementation that holds two digits after the decimal (so e.g. dollars and cents) </summary>
    public struct Fixnum : IEquatable<Fixnum>, IComparable<Fixnum>
    {
        public static readonly Fixnum ZERO = new Fixnum();
        public static readonly Fixnum ONE = new Fixnum(1);
        public static readonly Fixnum MAX_VALUE = new Fixnum() { scaled = int.MaxValue };
        public static readonly Fixnum MIN_VALUE = new Fixnum() { scaled = int.MinValue };

        public const int SCALE = 100; // 2 digits = 10^2 = 100;

        public int scaled; // raw representation without the decimal point

        public Fixnum (int value) { scaled = value * SCALE; }
        public Fixnum (float value) { scaled = (int) Math.Round(value * SCALE); }

        public static bool Equals (Fixnum a, Fixnum b) => a.scaled == b.scaled;
        public bool Equals (Fixnum other) => this.scaled == other.scaled;
        public bool IsZero => scaled == 0;
        public bool IsNotZero => scaled != 0;
        public bool IsPositive => scaled > 0;
        public bool IsNegative => scaled < 0;
        public bool IsNotPositive => scaled <= 0;
        public bool IsNotNegative => scaled >= 0;
        public bool IsNegativeOrZero => scaled <= 0;
        public bool IsPositiveOrZero => scaled >= 0;

        public Fixnum Abs => (scaled >= 0) ? this : new Fixnum() { scaled = -scaled };
        public Fixnum Floor () => new Fixnum() { scaled = SCALE * (int) Math.Floor((double) scaled / SCALE) };
        public Fixnum Ceiling () => new Fixnum() { scaled = SCALE * (int) Math.Ceiling((double) scaled / SCALE) };
        public Fixnum Round () => new Fixnum() { scaled = SCALE * (int) Math.Round((double) scaled / SCALE) };

        public Fixnum PosCeilingNegFloor () => scaled >= 0 ? Ceiling() : Floor();
        public Fixnum PosFloorNegCeiling () => scaled >= 0 ? Floor() : Ceiling();

        // for numbers >= 100, round to the nearest 10, otherwise just round
        public int RoundToOrderOfMagnitude () {
            int scale =
                (this < 100 && this > -100) ? 1 :
                (this < 1000 && this > -1000) ? 10 :
                100;

            float scaled = ((float) this) / scale;
            var rounded = (int) Math.Round(scaled);
            var unscaled = rounded * scale;
            return unscaled;
        }

        public int IntFloor () => (int) Math.Floor((double) scaled / SCALE);
        public int IntCeiling () => (int) Math.Ceiling((double) scaled / SCALE);

        public override bool Equals (object obj) => obj is Fixnum fixnum && scaled == fixnum.scaled;
        public override int GetHashCode () => scaled.GetHashCode();

        public int CompareTo (Fixnum other) => scaled.CompareTo(other.scaled);

        public static implicit operator Fixnum (int value) => new Fixnum(value);
        public static explicit operator Fixnum (float value) => new Fixnum(value);
        public static explicit operator Fixnum (double value) => new Fixnum((float) value);

        public static explicit operator int (Fixnum fixnum) => fixnum.scaled / SCALE;
        public static explicit operator float (Fixnum fixnum) => fixnum.scaled / (float) SCALE;

        public static bool operator == (Fixnum a, Fixnum b) => a.scaled == b.scaled;
        public static bool operator != (Fixnum a, Fixnum b) => a.scaled != b.scaled;
        public static bool operator < (Fixnum a, Fixnum b) => a.scaled < b.scaled;
        public static bool operator > (Fixnum a, Fixnum b) => a.scaled > b.scaled;
        public static bool operator <= (Fixnum a, Fixnum b) => a.scaled <= b.scaled;
        public static bool operator >= (Fixnum a, Fixnum b) => a.scaled >= b.scaled;

        public static Fixnum operator + (Fixnum f) => f;
        public static Fixnum operator - (Fixnum f) => new Fixnum() { scaled = -f.scaled };

        public static Fixnum operator + (Fixnum a, Fixnum b) => new Fixnum() { scaled = a.scaled + b.scaled };
        public static Fixnum operator - (Fixnum a, Fixnum b) => new Fixnum() { scaled = a.scaled - b.scaled };

        public static Fixnum operator ++ (Fixnum f) => f + 1;
        public static Fixnum operator -- (Fixnum f) => f - 1;

        public static Fixnum operator * (Fixnum a, Fixnum b) {
            long result = ((long) a.scaled * b.scaled) / SCALE;
            return new Fixnum() { scaled = (int) result };
        }

        public static Fixnum operator / (Fixnum a, Fixnum b) {
            long result = ((long) a.scaled * SCALE) / b.scaled;
            return new Fixnum() { scaled = (int) result };
        }

        public static Fixnum Clamp (Fixnum value, Fixnum min, Fixnum max) {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        public static Fixnum Max (Fixnum a, Fixnum b) => (a.scaled > b.scaled) ? a : b;
        public static Fixnum Min (Fixnum a, Fixnum b) => (a.scaled < b.scaled) ? a : b;
        public static Fixnum CloserToZero (Fixnum a, Fixnum b) => (a.Abs < b.Abs) ? a : b;

        // serialization helpers

        public static object Serialize (Fixnum f, SION.Serializer s) => s.Serialize((float) f);
        public static Fixnum Deserialize (object value, SION.Serializer s) => new Fixnum(s.Deserialize<float>(value));

        public override string ToString () => ((float) this).ToString(CultureInfo.InvariantCulture);
    }

    public sealed class FixnumEqualityComparer : IEqualityComparer<Fixnum>
    {
        public bool Equals (Fixnum x, Fixnum y) => x == y;
        public int GetHashCode (Fixnum obj) => obj.GetHashCode();
    }
}
