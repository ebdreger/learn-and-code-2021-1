using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static System.Runtime.Intrinsics.X86.Bmi2;
using System.Text;

namespace learn_and_code
{
    internal class Bonus
    {
        private static Int32[] _uint32ByteShift = { 24, 16, 8, 0 };

        /// <summary>
        ///   Format a UInt32 as a nybble-separated value.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     AFAICT, .NET lacks a built-in way to do this.  I didn't write this for readability,
        ///     but am happy to explain it if anyone likes.  Feel free just to use it as a black
        ///     box.
        ///   </para>
        /// </remarks>
        public static String FormatBinary(UInt32 value)
        {
            StringBuilder sb = new StringBuilder("0b", 46);
            foreach (Int32 shift in _uint32ByteShift)
            {
                UInt32 u = ((value >> shift) & 0xff);
                u = ParallelBitDeposit(u, 0x5555);
                u = ParallelBitDeposit(u, 0x55555555);
                sb.AppendFormat("__{0:X4}_{1:X4}", (int)(u >> 16), (int)(u & 0xffff));
            }
            return sb.ToString();
        }
    }

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

        public FacetValue FacetValues()
        {
            Console.WriteLine("");
            Console.WriteLine("       [Quantity.][Color....][Shading..][Shape....]");
            Console.WriteLine("    {0} <== 0x{1:X8} <== FacetValues()", Bonus.FormatBinary((UInt32)_facetValues), (UInt32)_facetValues);
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
            Console.WriteLine("");
            Console.WriteLine("  <<< Mask() call start");
            Console.ReadLine();
            Console.WriteLine("    {0} <== Mask input", Bonus.FormatBinary(input));
            Console.WriteLine("    {0} <== Mask andMask", Bonus.FormatBinary(andMask));
            Console.WriteLine("    {0} <== Mask xorMask", Bonus.FormatBinary(xorMask));
            Console.WriteLine("    {0} <== Mask result", Bonus.FormatBinary((input & andMask) ^ xorMask));
            Console.WriteLine("  >>> Mask() call finish");
            Console.ReadLine();
            Console.WriteLine("");
            return (input & andMask) ^ xorMask;
        }

        private static FacetValue Mask(FacetValue input, FacetMask andMask, FacetMask xorMask)
        {
            return (FacetValue)Mask((UInt32)input, (UInt32)andMask, (UInt32)xorMask);
        }

        private static Boolean TestPopCount(UInt32 input, int expectedBitCount)
        {
            Console.WriteLine("");
            Console.WriteLine("  <<< TestPopCount() call start");
            Console.ReadLine();
            Console.WriteLine("       [Quantity.][Color....][Shading..][Shape....]");
            Console.WriteLine("    {0} <== TestPopCount gives {1} (we wanted {2})", Bonus.FormatBinary(input), BitOperations.PopCount(input), expectedBitCount);
            Console.WriteLine("  >>> TestPopCount() call finish");
            Console.ReadLine();
            Console.WriteLine("");
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
            Console.WriteLine("");
            Console.WriteLine("  <<< PrepareFacetsForComparison() call start");
            Console.ReadLine();
            Console.WriteLine("    {0} <== (facets                )", Bonus.FormatBinary(facets));
            Console.WriteLine("    {0} <== (         (facets >> 4))", Bonus.FormatBinary((facets >> 4)));
            Console.WriteLine("    {0} <== (facets ^ (facets >> 4))", Bonus.FormatBinary(facets ^ (facets >> 4)));
            UInt32 result = Mask(facets ^ (facets >> 4), (UInt32)AllOneMask, MagicXorMask);
            Console.WriteLine("    {1} <== PrepareFacetsForComparison({0}) result", Bonus.FormatBinary(facets), Bonus.FormatBinary(result));
            Console.WriteLine("  >>> PrepareFacetsForComparison() call finish");
            Console.ReadLine();
            Console.WriteLine("");
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
            Console.WriteLine("");
            Console.WriteLine("  <<< FindMatch() call start");
            Console.ReadLine();
            UInt32 oursBeforePrep = (UInt32)FacetValues();
            UInt32 theirsBeforePrep = (UInt32)other.FacetValues();
            UInt32 ours = PrepareFacetsForComparison(oursBeforePrep);
            UInt32 theirs = PrepareFacetsForComparison(theirsBeforePrep);
            Console.WriteLine("  ...back to the task at hand...");
            Console.WriteLine("    {0} <== (First         )                {1:G}", Bonus.FormatBinary(ours), (FacetValue)oursBeforePrep);
            Console.WriteLine("    {0} <== (        Second)                {1:G}", Bonus.FormatBinary(theirs), (FacetValue)theirsBeforePrep);
            Console.WriteLine("    {0} <== (First | Second)               ", Bonus.FormatBinary(ours | theirs));
            Console.WriteLine("");
            Console.WriteLine("    {0} <==                    ~MagicOrMask", Bonus.FormatBinary(~MagicOrMask));
            UInt32 union = (ours | theirs) & ~MagicOrMask;
            Console.WriteLine("    {0} <== (First | Second) & ~MagicOrMask // let's call it \"union\"", Bonus.FormatBinary(union));
            Console.WriteLine("");
            Console.WriteLine("    {0} <== ((        MagicDelta)              )", Bonus.FormatBinary(MagicDelta));
            Console.WriteLine("    {0} <== ((union + MagicDelta)              )", Bonus.FormatBinary(union + MagicDelta));
            Console.WriteLine("    {0} <== (                       MagicOrMask)", Bonus.FormatBinary(MagicOrMask));
            Console.WriteLine("    {0} <== ((union + MagicDelta) & MagicOrMask)", Bonus.FormatBinary((union + MagicDelta) & MagicOrMask));
            UInt32 xorMask = ((union + MagicDelta) & MagicOrMask) * 0b1110;
            Console.WriteLine("    {0} <== ((union + MagicDelta) & MagicOrMask) * 0b1110 // let's call it \"xorMask\"", Bonus.FormatBinary(xorMask));
            Console.WriteLine("");
            Console.WriteLine("    {0} <== (union          )                    // from above", Bonus.FormatBinary(union));
            Console.WriteLine("    {0} <== (        xorMask)                    // from above", Bonus.FormatBinary(xorMask));
            Console.WriteLine("    {0} <== (union ^ xorMask)", Bonus.FormatBinary(union ^ xorMask));
            Console.WriteLine("    {0} <==                     NonInvertedMask", Bonus.FormatBinary(NonInvertedMask));
            UInt32 result = ((union ^ xorMask) & NonInvertedMask);
            Console.WriteLine("       [Quantity.][Color....][Shading..][Shape....]");
            Console.WriteLine("    {0} <== (union ^ xorMask) & NonInvertedMask <== result", Bonus.FormatBinary(result));
            Console.WriteLine("    {0:G}", (FacetValue)result);
            Console.WriteLine("  >>> FindMatch() call finish");
            Console.WriteLine("");
            Console.ReadLine();
            return (FacetValue)result;
        }

        public static Boolean IsMatch(Card[] cards)
        {
            Console.WriteLine("");
            Console.WriteLine("  <<< IsMatch() call start");
            Console.ReadLine();
            Trace.Assert(3 == cards.Length);
            UInt32 matches = cards.Aggregate((UInt32)AllOneMask,
                                             (a, card) => {
                                                 Console.WriteLine("    ...next card...");
                                                 FacetValue facetValuesBeforePrep = card.FacetValues();
                                                 UInt32 facetValues = PrepareFacetsForComparison(facetValuesBeforePrep);
                                                 UInt32 result = a & facetValues;
                                                 Console.WriteLine("    {0} <==  facetValuesBeforePrep", Bonus.FormatBinary((UInt32)facetValuesBeforePrep));
                                                 Console.WriteLine("    {0} <== (facetValues              )", Bonus.FormatBinary(facetValues));
                                                 Console.WriteLine("    {0} <== (              accumulator)", Bonus.FormatBinary(a));
                                                 Console.WriteLine("    {0} <== (facetValues & accumulator) <== new accumulator value", Bonus.FormatBinary(result));
                                                 return result;
                                             },
                                             intersection => {
                                                 Console.ReadLine();
                                                 Console.WriteLine("    {0} <== (intersection             )", Bonus.FormatBinary(intersection));
                                                 Console.WriteLine("    {0} <== (               MagicDelta)", Bonus.FormatBinary(MagicDelta));
                                                 UInt32 difference = intersection - MagicDelta;
                                                 Console.WriteLine("    {0} <== (intersection - MagicDelta)", Bonus.FormatBinary(difference));
                                                 Console.WriteLine("    {0} <== NonInvertedMask for AND", Bonus.FormatBinary(NonInvertedMask));
                                                 Console.WriteLine("    {0} <== MagicOrMask for XOR", Bonus.FormatBinary(MagicOrMask));
                                                 UInt32 result = Mask((intersection - MagicDelta), NonInvertedMask, MagicOrMask);
                                                 return result;
                                             });
            Boolean result = TestPopCount4(matches);
            Console.WriteLine("  >>> IsMatch() call finish");
            Console.WriteLine("");
            Console.ReadLine();
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
                Console.WriteLine("       [Quantity.]                                 ");
                Console.WriteLine("    {0} : One", Bonus.FormatBinary((UInt32)Card.FacetValue.One));
                Console.WriteLine("    {0} : Two", Bonus.FormatBinary((UInt32)Card.FacetValue.Two));
                Console.WriteLine("    {0} : Three", Bonus.FormatBinary((UInt32)Card.FacetValue.Three));
                Console.WriteLine("                  [Color....]                      ");
                Console.WriteLine("    {0} : Red", Bonus.FormatBinary((UInt32)Card.FacetValue.Red));
                Console.WriteLine("    {0} : Purple", Bonus.FormatBinary((UInt32)Card.FacetValue.Purple));
                Console.WriteLine("    {0} : Green", Bonus.FormatBinary((UInt32)Card.FacetValue.Green));
                Console.WriteLine("                             [Shading..]           ");
                Console.WriteLine("    {0} : Solid", Bonus.FormatBinary((UInt32)Card.FacetValue.Solid));
                Console.WriteLine("    {0} : Striped", Bonus.FormatBinary((UInt32)Card.FacetValue.Striped));
                Console.WriteLine("    {0} : Outlined", Bonus.FormatBinary((UInt32)Card.FacetValue.Outlined));
                Console.WriteLine("                                        [Shape....]");
                Console.WriteLine("    {0} : Oval", Bonus.FormatBinary((UInt32)Card.FacetValue.Oval));
                Console.WriteLine("    {0} : Squiggle", Bonus.FormatBinary((UInt32)Card.FacetValue.Squiggle));
                Console.WriteLine("    {0} : Diamond", Bonus.FormatBinary((UInt32)Card.FacetValue.Diamond));
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
