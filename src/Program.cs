using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace learn_and_code
{

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
