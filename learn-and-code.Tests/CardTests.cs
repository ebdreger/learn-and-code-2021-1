using System;
using Xunit;
using learn_and_code;

namespace learn_and_code.Tests
{
    public class CardTests
    {
        [Fact]
        public void IsMatch_MatchingSetShouldReturnTrue()
        {
            Card card1 = new Card("1111");
            Card card2 = new Card("2222");
            Card card3 = new Card("3333");
            Card[] cards = new Card[] { card1, card2, card3 };

            bool expected = true;
            bool actual = Card.IsMatch(cards);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsMatch_NonMatchingSetShouldReturnFalse()
        {
            Card card1 = new Card("2222");
            Card card2 = new Card("1123");
            Card card3 = new Card("3211");
            Card[] cards = new Card[] { card1, card2, card3 };

            bool expected = false;
            bool actual = Card.IsMatch(cards);
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void StringToFacetValues_InvalidString()
        {
            Card.FacetValue facetValue = Card.StringToFacetValues("4444");

            bool isNumeric = int.TryParse(facetValue.ToString(), out _);

            bool expected = true;
            bool actual = isNumeric;
            Assert.Equal(expected, actual);

        }


        [Fact]
        public void StringToFacetValues_validString()
        {
            //Card.FacetValue facetValue = Card.StringToFacetValues("1111");

            //bool isNumeric = int.TryParse(facetValue.ToString(), out _);
            //Card.FacetValue facetValue = Card.StringToFacetValues("1111");
            Card card = new Card("1111");


            bool isNumeric = card.IsValid();

            bool expected = false;
            bool actual = isNumeric;
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void FindMatch()
        {
            //Placeholder to test this function in the future
            Assert.False(true);
        }

        [Fact]
        public void FacetValues()
        {
            //Placeholder to test this function in the future
            Assert.False(true);
        }

    }
}
