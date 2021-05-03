using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace learn_and_code
{
    [Flags]
    public enum Card : UInt32
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

    public class Fubar
    {
        public static Card FindMatch(Card[] cards)
        {
            // Trace.Assert(2 == cards.Length);
            Card
                union = (cards[0] | cards[1]) & ~Card.MagicOrMask,
                xorMask = (Card)((((UInt32)union + (UInt32)Card.MagicDelta) & (UInt32)Card.MagicOrMask) * 0b1110);
            return (union ^ xorMask) & Card.NonInvertedMask;
        }

        public static Boolean IsMatch(Card[] cards)
        {
            Trace.Assert(3 == cards.Length);
            Card
                intersection = cards[0] & cards[1] & cards[2],
                allDifferentCheck = (Card)(intersection - Card.MagicDelta),
                matches = (allDifferentCheck & Card.NonInvertedMask) ^ Card.MagicOrMask;
            Console.WriteLine("{0:x} & {1:x} & {2:x} => i {3:x} => adc {4:x} => match {5:x}",
                              cards[0], cards[1], cards[2],
                              intersection,
                              allDifferentCheck,
                              matches);
            Console.WriteLine("{0:x} {0:G}", (Card)matches);
            return (4 == BitOperations.PopCount((UInt32)matches));
        }

        public static void PrintCards(Card[] cards)
        {
            Trace.Assert(3 == cards.Length);
            foreach (Card card in cards)
            {
                Console.WriteLine("{0:x} {0:G}", card);
            }
            Console.WriteLine("");
        }

        public static Card StringToCard(string input)
        {
            byte position = 36;
            UInt32 result = (UInt32)Card.MagicOrMask;
            foreach (char c in input)
            {
                // Console.WriteLine("{0}", ((position -= 8) + c - 48));
                result |= (UInt32)1 << ((position -= 8) + c - 48);
            }
            result |= (~(UInt32)result >> 4) & (UInt32)Card.InvertedMask;
            return (Card)result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Card[] cards = {
                Card.One | Card.Red | Card.Solid | Card.Oval | Card.MagicOrMask,
                Card.Two | Card.Purple | Card.Solid | Card.Oval | Card.MagicOrMask,
                Card.Three | Card.Green | Card.Solid | Card.Oval | Card.MagicOrMask
            };

            Fubar.PrintCards(cards);

            cards[0] |= (Card)(~(UInt32)cards[0] >> 4) & Card.InvertedMask;
            cards[1] |= (Card)(~(UInt32)cards[1] >> 4) & Card.InvertedMask;
            cards[2] |= (Card)(~(UInt32)cards[2] >> 4) & Card.InvertedMask;

            Fubar.PrintCards(cards);

            Console.WriteLine("{0} => {1:G}", "1123", Fubar.StringToCard("1123") & Card.NonInvertedMask);

            Console.WriteLine("{0:G} / {1:G} / {2:G}", cards[0], cards[1], Fubar.FindMatch(cards));

            Console.WriteLine("Match status: {0}", Fubar.IsMatch(cards));
            // Console.WriteLine("Hello World! {0:G}", (Card)16);
        }
    }
}
