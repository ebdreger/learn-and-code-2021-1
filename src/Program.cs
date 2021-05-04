using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace learn_and_code
{
    /// <summary>
    ///   XXX
    /// </summary>
    public class Card
    {
        /// <summary>
        ///   Magical numbers that, when combined properly, describe any card.
        /// </summary>
        [Flags]
        public enum FacetValue : UInt32
        {
            // quantities
            DifferentQuantities = 0b__0001_0000__0000_0000__0000_0000__0000_0000,
            One                 = (DifferentQuantities << 1),
            Two                 = (DifferentQuantities << 2),
            Three               = (DifferentQuantities << 3),
            // colors
            DifferentColors     = 0b__0000_0000__0001_0000__0000_0000__0000_0000,
            Red                 = (DifferentColors << 1),
            Purple              = (DifferentColors << 2),
            Green               = (DifferentColors << 3),
            // shading
            DifferentShadings   = 0b__0000_0000__0000_0000__0001_0000__0000_0000,
            Solid               = (DifferentShadings << 1),
            Striped             = (DifferentShadings << 2),
            Outlined            = (DifferentShadings << 3),
            // shapes
            DifferentShapes     = 0b__0000_0000__0000_0000__0000_0000__0001_0000,
            Oval                = (DifferentShapes << 1),
            Squiggle            = (DifferentShapes << 2),
            Diamond             = (DifferentShapes << 3),
        }

        /// <summary>
        ///   XXX
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     <strong>NOTA BENE:</strong> These masks deliberately exclude the "DifferentFoo"
        ///     values, as said "DifferentFoo" refer to <em>matches</em> instead of <em>cards</em>.
        ///   </para>
        /// </remarks>
        private enum FacetMask : UInt32
        {
            Quantity            = 0b__1110_0000__0000_0000__0000_0000__0000_0000,
            Color               = 0b__0000_0000__1110_0000__0000_0000__0000_0000,
            Shading             = 0b__0000_0000__0000_0000__1110_0000__0000_0000,
            Shape               = 0b__0000_0000__0000_0000__0000_0000__1110_0000,
        }

        /// <summary>
        ///   XXX
        /// </summary>
        private static Boolean IsValidFacetValue(FacetValue facetValue, FacetMask facetMask)
        {
            // NOTA BENE: These typecasts are carefully structured: Were we to flub a cast and lose
            // precision, the return result would be "false".  Although there may be confusion
            // around the apparent error locus -- which most likely would appear to be our caller --
            // this would be preferable to an erroneous "true" return.
            return ((1 == BitOperations.PopCount((UInt32)facetValue)) &&
                    ((UInt32)facetValue == ((UInt32)facetValue & (UInt32)facetMask)));
        }

        /// <summary>
        ///   XXX
        /// </summary>
        public static Boolean IsValidQuantity(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Quantity));
        }

        /// <summary>
        ///   XXX
        /// </summary>
        public static Boolean IsValidColor(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Color));
        }

        /// <summary>
        ///   XXX
        /// </summary>
        public static Boolean IsValidShading(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Shading));
        }

        /// <summary>
        ///   XXX
        /// </summary>
        public static Boolean IsValidShape(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Shape));
        }

        /// <summary>
        ///   XXX
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     <strong>MUST have one bit set per FacetMask group.</strong>
        ///   </para>
        /// </remarks>
        private FacetValue _facetValues;

        /// <summary>
        ///   XXX
        /// </summary>
        private static readonly UInt32 FacetValueBaseMask = 0b__1110_0000;

        /// <summary>
        ///   XXX
        /// </summary>
        public Boolean IsValid()
        {
            for (UInt32 mask = FacetValueBaseMask << 24; 0 != mask; mask >>= 8)
            {
                if (1 != BitOperations.PopCount((UInt32)this._facetValues & mask))
                {
                    return false;
                }
            }
            return (4 == BitOperations.PopCount((UInt32)this._facetValues));
        }

        /// <summary>
        ///   XXX
        /// </summary>
        public Card (FacetValue facetValues)
        {
            this._facetValues = facetValues;
        }

        /// <summary>
        ///   XXX
        /// </summary>
        public Card (FacetValue quantity, FacetValue color, FacetValue shading, FacetValue shape)
        {
            Trace.Assert(IsValidQuantity(quantity) &&
                         IsValidColor(color) &&
                         IsValidShading(shading) &&
                         IsValidShape(shape));
            this._facetValues = quantity | color | shading | shape;
        }

        // special
        // XXX: BUG - s/public/private/g
        // XXX: TO DO - refactor in terms of one another
        public static readonly UInt32 MagicOrMask     = 0b__0001_0000__0001_0000__0001_0000__0001_0000;
        public static readonly UInt32 MagicDelta      = 0b__0000_0010__0000_0010__0000_0010__0000_0010;
        public static readonly UInt32 NonInvertedMask = 0b__1111_0000__1111_0000__1111_0000__1111_0000;
        public static readonly UInt32 InvertedMask    = 0b__0000_1111__0000_1111__0000_1111__0000_1111;
        public static readonly UInt32 MagicXorMask    = (MagicOrMask | InvertedMask);

        private static readonly UInt32 FacetValueBase = 0b__0001_0000;

        /// <summary>
        ///   XXX
        /// </summary>
        public static FacetValue StringToFacetValue(string input)
        {
            // XXX: TO DO - add validity checks
            UInt32 accumulator = 0;
            foreach (char c in input)
            {
                accumulator <<= 8;
                accumulator |= FacetValueBase << (c - '0');
            }
            accumulator = (accumulator ^ (accumulator >> 4) ^ MagicXorMask);
            // Trace.Assert(input.Cast<char>().Aggregate<string, UInt32, FacetValue>
            //              (input,
            //               (UInt32)0,
            //               (accumulator, c) => ((accumulator << 8) | (FacetValueBase << (c - '0'))),
            //               accumulator => (FacetValue)accumulator)
            //              == accumulator);

            // IEnumerable<UInt32> fubar = input.ToCharArray().Select(c => (UInt32)(c - '0'));
            // fubar.Aggregate(0L, (accumulator, value) => (accumulator << 8) | (UInt32)value, u => u);

            Console.WriteLine("{0:x} -vs- {1:x}",
                              accumulator,
                              input.ToCharArray().Aggregate(0L, (accumulator, value) => (accumulator << 8) | (UInt32)(1 << (value - '0')), u => u)
                              );

            return (FacetValue)accumulator;
        }

        /// <summary>
        ///   XXX
        /// </summary>
        public Card (string input)
        {
            this._facetValues = StringToFacetValue(input);
        }

        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///   XXX
        /// </summary>
        public static FacetValue FindMatch(FacetValue[] facetValues)
        {
            // Trace.Assert(2 == facetValues.Length);
            UInt32
                union = (UInt32)(facetValues[0] | facetValues[1]) & ~MagicOrMask,
                xorMask = ((union + MagicDelta) & MagicOrMask) * 0b1110;
            return (FacetValue)((union ^ xorMask) & NonInvertedMask);
        }

        public static Boolean IsMatch(Card[] cards)
        /// <summary>
        ///   XXX
        /// </summary>
        {
            Trace.Assert(3 == cards.Length);
            UInt32
                intersection = (UInt32)(cards[0]._facetValues & cards[1]._facetValues & cards[2]._facetValues),
                allDifferentCheck = intersection - MagicDelta,
                matches = (allDifferentCheck & NonInvertedMask) ^ MagicOrMask;
            return (4 == BitOperations.PopCount(matches));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Card[] cards = {
                new Card("1212"),
                new Card("2311"),
                new Card("3113")
            };

            // Console.WriteLine("{0:G} / {1:G} / {2:G}", facetValues[0], facetValues[1], Card.FindMatch(facetValues));
            Console.WriteLine("Match status: {0}", Card.IsMatch(cards));
        }
    }
}

// XXX: TO DO -
//
// - Apply MagicOrMask JIT (not stored in this._facetValues)
