using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace learn_and_code
{
    public class Card
    {
        #region Enums // enums

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

        public FacetValue FacetValues()
        {
            return _facetValues;
        }

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
            return (input & andMask) ^ xorMask;
        }

        private static FacetValue Mask(FacetValue input, FacetMask andMask, FacetMask xorMask)
        {
            return (FacetValue)Mask((UInt32)input, (UInt32)andMask, (UInt32)xorMask);
        }

        private static Boolean TestPopCount(UInt32 input, int expectedBitCount)
        {
            return BitOperations.PopCount(input) == expectedBitCount;
        }

        private static Boolean TestPopCount1(UInt32 input)
        {
            return TestPopCount(input, 1);
        }

        private static Boolean TestPopCount1(FacetValue input)
        {
            return TestPopCount1((UInt32)input);
        }

        private static Boolean TestPopCount4(UInt32 input)
        {
            return TestPopCount(input, 4);
        }

        private static Boolean TestPopCount4(FacetValue input)
        {
            return TestPopCount4((UInt32)input);
        }

        private static Boolean IsValidFacetValue(FacetValue facetValue, FacetMask facetMask)
        {
            return TestPopCount1(facetValue) &&
                (0 == Mask(facetValue, facetMask, (FacetMask)facetValue));
        }

        public static Boolean IsValidQuantity(FacetValue facetValue)
        {
            return IsValidFacetValue(facetValue, FacetMask.Quantity);
        }

        public static Boolean IsValidColor(FacetValue facetValue)
        {
            return IsValidFacetValue(facetValue, FacetMask.Color);
        }

        public static Boolean IsValidShading(FacetValue facetValue)
        {
            return IsValidFacetValue(facetValue, FacetMask.Shading);
        }

        public static Boolean IsValidShape(FacetValue facetValue)
        {
            return IsValidFacetValue(facetValue, FacetMask.Shape);
        }

        private static readonly UInt32 FacetValueBaseMask = 0b__1110_0000;

        private static readonly FacetMask AllOneMask = ~(FacetMask)0;

        private static UInt32 PrepareFacetsForComparison(UInt32 facets)
        {
            // Should we move MagicXorMask closer?  Or spell out that we are combining
            //
            //     (MagicOrMask | InvertedMask)
            //
            // because it better explains what we are doing?
            UInt32 result = Mask(facets ^ (facets >> 4), (UInt32)AllOneMask, MagicXorMask);
            return result;
        }

        private static UInt32 PrepareFacetsForComparison(FacetValue facets)
        {
            return PrepareFacetsForComparison((UInt32)facets);
        }

        public static FacetValue StringToFacetValues(String input)
        {
            return input.Aggregate(0U,
                                   // XXX: TO DO - validate input character "c"
                                   (a, c) => (a << 8) | (FacetValueBase << (int)(c - '0')),
                                   u => (FacetValue)u);
        }

        #endregion // BitBanging

        public Boolean IsValid()
        {
            // XXX: TO DO - "FacetValueBaseMask << 24" is unclean
            for (UInt32 mask = FacetValueBaseMask << 24; 0 != mask; mask >>= 8)
            {
                if (!TestPopCount1(Mask(FacetValues(), AllOneMask, (FacetMask)mask)))
                {
                    return false;
                }
            }
            return TestPopCount4(FacetValues());
        }

        #region Constructors

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

        public Card (String input)
        {
            this._facetValues = StringToFacetValues(input);
        }

        #endregion // Constructors

        public FacetValue FindMatch(Card other)
        {
            UInt32 oursBeforePrep = (UInt32)FacetValues();
            UInt32 theirsBeforePrep = (UInt32)other.FacetValues();
            UInt32 ours = PrepareFacetsForComparison(oursBeforePrep);
            UInt32 theirs = PrepareFacetsForComparison(theirsBeforePrep);
            UInt32 union = (ours | theirs) & ~MagicOrMask;
            UInt32 xorMask = ((union + MagicDelta) & MagicOrMask) * 0b1110;
            UInt32 result = ((union ^ xorMask) & NonInvertedMask);
            return (FacetValue)result;
        }

        public static Boolean IsMatch(Card[] cards)
        {
            Trace.Assert(3 == cards.Length);
            UInt32 matches = cards.Aggregate((UInt32)AllOneMask,
                                             (a, card) => {
                                                 FacetValue facetValuesBeforePrep = card.FacetValues();
                                                 UInt32 facetValues = PrepareFacetsForComparison(facetValuesBeforePrep);
                                                 UInt32 result = a & facetValues;
                                                 return result;
                                             },
                                             intersection => {
                                                 UInt32 difference = intersection - MagicDelta;
                                                 UInt32 result = Mask((intersection - MagicDelta), NonInvertedMask, MagicOrMask);
                                                 return result;
                                             });
            Boolean result = TestPopCount4(matches);
            return result;
        }
    }

    class Program
    {
        static void Main(String[] args)
        {
            Boolean match;

            static void PrintCards(Card[] cards)
            {
                foreach (Card card in cards)
                {
                    Console.WriteLine("    {0:G}", card.FacetValues());
                }
            }

            while (true)
            {
                Console.WriteLine("Enter a line in the form: \"2222 1123 3211\" (no quotes)");
                Console.WriteLine("Each group of four digits represents one card.");
                Console.WriteLine("Each digit represents one facet.  Order: quantity, color, shading, shape");
                Console.WriteLine("The digit indicates how many bits left we shift the low bit of the appropriate nybble.");
                Console.WriteLine("What card combination would you like to test?");

                Card[] cards = Console.ReadLine().Split(" ").Select(x => new Card(x)).ToArray();

                Trace.Assert(3 == cards.Length);

                Console.WriteLine("You chose:");
                PrintCards(cards);

                match = Card.IsMatch(cards);
                Console.WriteLine("Match status: {0}", match);

                if (!match)
                {
                    Console.WriteLine("Ways to make a match:");
                    for (int j = 0; j < 3; ++j)
                    {
                        for (int k = j + 1; k < 3; ++k)
                        {
                            Console.WriteLine("  ({0:G}) + ({1:G}) <== ({2:G})",
                                              cards[j].FacetValues(),
                                              cards[k].FacetValues(),
                                              cards[j].FindMatch(cards[k]));
                        }
                    }
                }

                Console.WriteLine("");
            }
        }
    }
}

// XXX: TO DO -
//
// - Apply MagicOrMask JIT (not stored in this._facetValues)
