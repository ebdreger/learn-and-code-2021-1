using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace learn_and_code
{
    public class Card
    {
        #region Enums

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

        #endregion // Enums

        /// <remarks>
        ///   <para>
        ///     <strong>MUST have one bit set per FacetMask group.</strong>
        ///   </para>
        /// </remarks>
        private FacetValue _facetValues;

        #region BitBanging

        // special
        // XXX: TO DO - refactor in terms of one another
        // XXX: TO DO - once below "MagicXorMask" is factored elsewhere, relocate these declarations to be ALAP
        // XXX: TO DO - ^^^ on second thought, ASAP seems to flow better
        private static readonly UInt32 MagicOrMask     = 0b__0001_0000__0001_0000__0001_0000__0001_0000;
        private static readonly UInt32 MagicDelta      = 0b__0000_0010__0000_0010__0000_0010__0000_0010;
        private static readonly UInt32 NonInvertedMask = 0b__1111_0000__1111_0000__1111_0000__1111_0000;
        private static readonly UInt32 InvertedMask    = 0b__0000_1111__0000_1111__0000_1111__0000_1111;
        private static readonly UInt32 MagicXorMask    = (MagicOrMask | InvertedMask);
        private static readonly UInt32 FacetValueBase  = 0b__0001_0000;

        // XXX: TO DO
        // private delegate UInt32 MaskDelegate(UInt32 andMask, UInt32 xorMask);

        private static UInt32 Mask(UInt32 input, UInt32 andMask, UInt32 xorMask)
        {
            return ((input & andMask) ^ xorMask);
        }

        private static FacetValue Mask(FacetValue input, FacetMask andMask, FacetMask xorMask)
        {
            return ((FacetValue)Mask((UInt32)input, (UInt32)andMask, (UInt32)xorMask));
        }

        private static Boolean TestPopCount(UInt32 input, int expectedBitCount)
        {
            return (BitOperations.PopCount(input) == expectedBitCount);
        }

        private static Boolean TestPopCount1(FacetValue input)
        {
            return (TestPopCount((UInt32)input, 1));
        }

        private static Boolean TestPopCount4(FacetValue input)
        {
            return (TestPopCount((UInt32)input, 4));
        }

        private static Boolean IsValidFacetValue(FacetValue facetValue, FacetMask facetMask)
        {
            return (TestPopCount1(facetValue) &&
                    (0 == Mask(facetValue, facetMask, (FacetMask)facetValue)));
        }

        public static Boolean IsValidQuantity(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Quantity));
        }

        public static Boolean IsValidColor(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Color));
        }

        public static Boolean IsValidShading(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Shading));
        }

        public static Boolean IsValidShape(FacetValue facetValue)
        {
            return (IsValidFacetValue(facetValue, FacetMask.Shape));
        }

        private static readonly UInt32 FacetValueBaseMask = 0b__1110_0000;

        private static FacetMask AllOneMask = ~(FacetMask)0;

        #endregion // BitBanging

        public Boolean IsValid()
        {
            // XXX: TO DO - "FacetValueBaseMask << 24" is unclean
            for (UInt32 mask = FacetValueBaseMask << 24; 0 != mask; mask >>= 8)
            {
                if (!TestPopCount1(Mask(this._facetValues, AllOneMask, (FacetMask)mask)))
                {
                    return false;
                }
            }
            return (TestPopCount4(this._facetValues));
        }

        public Card (FacetValue facetValues)
        {
            this._facetValues = facetValues;
            Debug.Assert(IsValid());
        }

        public Card (FacetValue quantity, FacetValue color, FacetValue shading, FacetValue shape)
        {
            Trace.Assert(IsValidQuantity(quantity) &&
                         IsValidColor(color) &&
                         IsValidShading(shading) &&
                         IsValidShape(shape));
            this._facetValues = quantity | color | shading | shape;
            Debug.Assert(IsValid());
        }

        private static UInt32 PrepareFacetsForComparison(UInt32 facets)
        {
            // Should we move MagicXorMask closer?  Or spell out that we are combining
            //
            //     (MagicOrMask | InvertedMask)
            //
            // because it better explains what we are doing?
            return (Mask(facets ^ (facets >> 4), (UInt32)AllOneMask, MagicXorMask));
        }

        private static UInt32 PrepareFacetsForComparison(FacetValue facets)
        {
            return (PrepareFacetsForComparison((UInt32)facets));
        }

        public static FacetValue StringToFacetValues(String input)
        {
            return (input.Aggregate(0U,
                                    // XXX: TO DO - validate input character "c"
                                    (a, c) => (a << 8) | (FacetValueBase << (int)(c - '0')),
                                    u => (FacetValue)u));
        }

        public Card (String input)
        {
            this._facetValues = StringToFacetValues(input);
        }

        ////////////////////////////////////////////////////////////////////////

        // public static FacetValue FindMatch(FacetValue[] facetValues)
        // {
        //     // Trace.Assert(2 == facetValues.Length);
        //     UInt32
        //         union = (UInt32)(facetValues[0] | facetValues[1]) & ~MagicOrMask,
        //         xorMask = ((union + MagicDelta) & MagicOrMask) * 0b1110;
        //     return (FacetValue)((union ^ xorMask) & NonInvertedMask);
        // }
        public FacetValue FindMatch(Card other)
        {
            UInt32
                union = (PrepareFacetsForComparison(this._facetValues) |
                         PrepareFacetsForComparison(other._facetValues)) & ~MagicOrMask,
                xorMask = ((union + MagicDelta) & MagicOrMask) * 0b1110;
            return (FacetValue)((union ^ xorMask) & NonInvertedMask);
        }

        public static Boolean IsMatch(Card[] cards)
        {
            Trace.Assert(3 == cards.Length);
            UInt32 matches = cards.Aggregate((UInt32)AllOneMask,
                                             (a, card) => a & PrepareFacetsForComparison(card._facetValues),
                                             intersection => Mask((intersection - MagicDelta), NonInvertedMask, MagicOrMask));
            return (TestPopCount4((FacetValue)matches));
        }
    }

    class Program
    {
        static void Main(String[] args)
        {
            Card[] cards = (new String[] {
                    "1212",
                    "2311",
                    "3113"})
                .Select(x => new Card(x)).ToArray();

            // foreach (Card card in cards)
            // {
            //     Console.WriteLine("card = {0:x}", card._facetValues);
            // }

            // Console.WriteLine("{0:G} / {1:G} / {2:G}", facetValues[0], facetValues[1], Card.FindMatch(facetValues));
            Console.WriteLine("Match status: {0}", Card.IsMatch(cards));
        }
    }
}

// XXX: TO DO -
//
// - Apply MagicOrMask JIT (not stored in this._facetValues)
