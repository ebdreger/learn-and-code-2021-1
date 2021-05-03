using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace learn_and_code
{
    public class Card
    {
        [Flags]
        public enum BitField : UInt32
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

        // special
        // XXX: BUG - s/public/private/g
        public static readonly UInt32 MagicOrMask     = 0b__0001_0000__0001_0000__0001_0000__0001_0000;
        public static readonly UInt32 MagicDelta      = 0b__0000_0010__0000_0010__0000_0010__0000_0010;
        public static readonly UInt32 NonInvertedMask = 0b__1111_0000__1111_0000__1111_0000__1111_0000;
        public static readonly UInt32 InvertedMask    = 0b__0000_1111__0000_1111__0000_1111__0000_1111;

        private BitField bitField;

        public static BitField StringToBitField(string input)
        {
            byte position = 36;
            UInt32 result = 0;
            foreach (char c in input)
            {
                result |= (UInt32)1 << ((position -= 8) + c - '0');
            }
            result |= ((~result >> 4) & InvertedMask) | MagicOrMask;
            // result |= (~result >> 4) | MagicOrMask;
            return (BitField)result;
        }

        public Card (string input)
        {
            bitField = StringToBitField(input);
        }

        public static BitField FindMatch(BitField[] bitFields)
        {
            // Trace.Assert(2 == bitFields.Length);
            UInt32
                union = (UInt32)(bitFields[0] | bitFields[1]) & ~MagicOrMask,
                xorMask = ((union + MagicDelta) & MagicOrMask) * 0b1110;
            return (BitField)((union ^ xorMask) & NonInvertedMask);
        }

        public static Boolean IsMatch(Card[] cards)
        {
            Trace.Assert(3 == cards.Length);
            UInt32
                intersection = (UInt32)(cards[0].bitField & cards[1].bitField & cards[2].bitField),
                allDifferentCheck = intersection - MagicDelta,
                matches = (allDifferentCheck & NonInvertedMask) ^ MagicOrMask;
            Console.WriteLine("{0:x} & {1:x} & {2:x} => i {3:x} => adc {4:x} => match {5:x}",
                              cards[0].bitField, cards[1].bitField, cards[2].bitField,
                              intersection,
                              allDifferentCheck,
                              matches);
            Console.WriteLine("{0:x} {0:G}", (BitField)matches);
            return (4 == BitOperations.PopCount(matches));
        }

        // public static void PrintBitFields(BitField[] bitFields)
        // {
        //     Trace.Assert(3 == bitFields.Length);
        //     foreach (BitField bitField in bitFields)
        //     {
        //         Console.WriteLine("{0:x} {0:G}", bitField);
        //     }
        //     Console.WriteLine("");
        // }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Card[] cards = {
                new Card("1111"),
                new Card("2211"),
                new Card("3311")
            };

            // Console.WriteLine("{0:G} / {1:G} / {2:G}", bitFields[0], bitFields[1], Card.FindMatch(bitFields));
            Console.WriteLine("Match status: {0}", Card.IsMatch(cards));
        }
    }
}

// XXX: TO DO -
//
// - Apply MagicOrMask JIT (not stored in this.bitField)
