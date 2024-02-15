using System.Collections.Generic;
using PrimerTools;

namespace RockPaperScissors;

public static class InPlaceShuffle
{
    public static void Shuffle<T>(this IList<T> list, Rng rng)  
    {
        // Same as the other shuffle, but in place
        int n = list.Count;  
        while (n > 1) {  
            n--;
            int k = rng.RangeInt(0, n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}