using System;

public class SysRand {
    Random rand;

    public SysRand(int seed) {
        rand = new Random(seed);
    }

    public float Range(float min, float max)
    {
        return (float)rand.NextDouble() * (max - min) + min;
    }

    public float NextFloat() {
        return (float)rand.NextDouble();
    }

    public int Range(int min, int max) {
        return rand.Next(min, max);
    }
}
