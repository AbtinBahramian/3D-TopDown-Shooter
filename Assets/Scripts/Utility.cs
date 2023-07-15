using System.Collections;
using System.Collections.Generic;

public static class Utility
{
    // Fisher-Rated shuffle algorithem 
    public static T[] FisherRatesShuffle<T>(T[] arrayToShuffle, int seed){
        System.Random prgn = new System.Random(seed);

        for(int i = 0; i <= arrayToShuffle.Length - 1; i++ ){
            int randomIndex = prgn.Next(i, arrayToShuffle.Length);

            T tempItem = arrayToShuffle[randomIndex];
            arrayToShuffle[randomIndex] = arrayToShuffle[i];
            arrayToShuffle[i] = tempItem;

        }
        return arrayToShuffle;
    }
}
