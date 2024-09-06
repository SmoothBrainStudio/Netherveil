using System.Collections.Generic;
using System.Linq;

static public class DictionaryExtensions
{
    /// <summary>
    /// Count the number of values in every list of the dictionnary
    /// </summary>
    /// <returns>Number of values in dictionnary</returns>
    static public int CountValues<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionnary)
    {
        int count = 0;
        foreach (var keyValuePair in dictionnary)
        {
            count += keyValuePair.Value.Count;
        }

        return count;
    }

    /// <summary>
    /// Count the number of values in every countainer of the dictionnary
    /// </summary>
    /// <returns>Number of values in dictionnary</returns>
    static public int CountValues<TKey, TValue>(this Dictionary<TKey, IEnumerable<TValue>> dictionnary)
    {
        int count = 0;
        foreach (var keyValuePair in dictionnary)
        {
            count += keyValuePair.Value.Count();
        }

        return count;
    }
}