namespace learn_and_code
{
    public interface ICard
    {
        Card.FacetValue FacetValues();
        Card.FacetValue FindMatch(Card other);
        bool IsValid();
    }
}