namespace ItemConduits.ConduitLib.APIs
{
    public interface IFilter
    {
        bool IsWhitelist { get; set; }
        int FiltersCount { get; }
        object this[int index] { get; set; }
        bool Condition(int index, object obj);
    }
}
