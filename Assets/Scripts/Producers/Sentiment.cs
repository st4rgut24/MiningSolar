using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sentiment
{
    const int UPPER_BOUND = 100;
    const int LOWER_BOUND = 0;

    public float bullishness { get; private set; }

    public Sentiment(int range) {
        int sentiment = Mathf.Max(LOWER_BOUND, range);
        sentiment = Mathf.Min(sentiment, UPPER_BOUND);
        bullishness = sentiment / UPPER_BOUND;
    }
}
