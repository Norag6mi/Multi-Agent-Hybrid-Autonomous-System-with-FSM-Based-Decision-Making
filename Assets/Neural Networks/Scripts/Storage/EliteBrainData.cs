using System;

[System.Serializable]
public class EliteBrainData
{
    public float mmr;

    public float[] weights;
    public float[] biases;

    public int[] networkShape;
}