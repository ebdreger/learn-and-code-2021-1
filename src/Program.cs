﻿using System;
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
            // special
            MagicOrMask         = 0b__0001_0000__0001_0000__0001_0000__0001_0000,
            MagicDelta          = 0b__0000_0010__0000_0010__0000_0010__0000_0010,
            NonInvertedMask     = 0b__1111_0000__1111_0000__1111_0000__1111_0000,
            InvertedMask        = 0b__0000_1111__0000_1111__0000_1111__0000_1111,
            AllBits             = ~(UInt32)0
        }

        public static BitField FindMatch(BitField[] bitFields)
        {
            // Trace.Assert(2 == bitFields.Length);
            UInt32
                union = (UInt32)((bitFields[0] | bitFields[1]) & ~BitField.MagicOrMask),
                xorMask = (((union + (UInt32)BitField.MagicDelta) & (UInt32)BitField.MagicOrMask) * 0b1110);
            return (BitField)(union ^ xorMask) & BitField.NonInvertedMask;
        }

        public static Boolean IsMatch(BitField[] bitFields)
        {
            Trace.Assert(3 == bitFields.Length);
            BitField
                intersection = bitFields[0] & bitFields[1] & bitFields[2],
                allDifferentCheck = (BitField)(intersection - BitField.MagicDelta),
                matches = (allDifferentCheck & BitField.NonInvertedMask) ^ BitField.MagicOrMask;
            Console.WriteLine("{0:x} & {1:x} & {2:x} => i {3:x} => adc {4:x} => match {5:x}",
                              bitFields[0], bitFields[1], bitFields[2],
                              intersection,
                              allDifferentCheck,
                              matches);
            Console.WriteLine("{0:x} {0:G}", (BitField)matches);
            return (4 == BitOperations.PopCount((UInt32)matches));
        }

        public static void PrintBitFields(BitField[] bitFields)
        {
            Trace.Assert(3 == bitFields.Length);
            foreach (BitField bitField in bitFields)
            {
                Console.WriteLine("{0:x} {0:G}", bitField);
            }
            Console.WriteLine("");
        }

        public static BitField StringToBitField(string input)
        {
            byte position = 36;
            UInt32 result = (UInt32)BitField.MagicOrMask;
            foreach (char c in input)
            {
                // Console.WriteLine("{0}", ((position -= 8) + c - '0'));
                result |= (UInt32)1 << ((position -= 8) + c - '0');
            }
            result |= (~(UInt32)result >> 4) & (UInt32)BitField.InvertedMask;
            return (BitField)result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Card.BitField[] bitFields = {
                Card.BitField.One | Card.BitField.Red | Card.BitField.Solid | Card.BitField.Oval | Card.BitField.MagicOrMask,
                Card.BitField.Two | Card.BitField.Purple | Card.BitField.Solid | Card.BitField.Oval | Card.BitField.MagicOrMask,
                Card.BitField.Three | Card.BitField.Green | Card.BitField.Solid | Card.BitField.Oval | Card.BitField.MagicOrMask
            };

            Card.PrintBitFields(bitFields);

            bitFields[0] |= (Card.BitField)(~(UInt32)bitFields[0] >> 4) & Card.BitField.InvertedMask;
            bitFields[1] |= (Card.BitField)(~(UInt32)bitFields[1] >> 4) & Card.BitField.InvertedMask;
            bitFields[2] |= (Card.BitField)(~(UInt32)bitFields[2] >> 4) & Card.BitField.InvertedMask;

            Card.PrintBitFields(bitFields);

            Console.WriteLine("{0} => {1:G}", "1123", Card.StringToBitField("1123") & Card.BitField.NonInvertedMask);

            Console.WriteLine("{0:G} / {1:G} / {2:G}", bitFields[0], bitFields[1], Card.FindMatch(bitFields));

            Console.WriteLine("Match status: {0}", Card.IsMatch(bitFields));
            // Console.WriteLine("Hello World! {0:G}", (Card.BitField)16);
        }
    }
}