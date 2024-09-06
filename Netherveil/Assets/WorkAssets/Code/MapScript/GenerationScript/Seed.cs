using System.Collections.Generic;
using Unity.Mathematics;

namespace Map.Generation
{
    static public class Seed
    {
        static private readonly int seedSize = 8;
        static public string seed = "XXXXXXXX";
        static public int Iteration { private set; get; } = 0;

        static private System.Random random = new System.Random(0);

        static public void RandomizeSeed()
        {
            System.Random tempRand = new System.Random();
            seed = string.Empty;

            for (int i = 0; i < seedSize; i++)
            {
                switch (tempRand.Next(0, 2))
                {
                    case 0:
                        seed += (char)tempRand.Next(97, 123); // lower case letters
                        break;
                    case 1:
                        seed += (char)tempRand.Next(48, 58); // numbers
                        break;
                }
            }

            random = new System.Random(SeedToInt());
            Iteration = 0;
        }

        static public void Reset()
        {
            random = new System.Random(SeedToInt());
            Iteration = 0;
        }

        static public void Set(string seed)
        {
            Seed.seed = seed;
            random = new System.Random(SeedToInt());
            Iteration = 0;
        }

        static public int Range(int minInclusive, int maxExclusive)
        {
            Iteration++;
            return random.Next(minInclusive, maxExclusive);
        }

        static public void Iterate(int numIt)
        {
            for (int i = 0; i < numIt; i++)
            {
                Iteration++;
                random.Next();
            }
        }

        // TODO : make it function lmao
        static public float Range(float minInclusive, float maxExclusive)
        {
            Iteration++;
            return (float)random.NextDouble() % (maxExclusive - minInclusive) + minInclusive;
        }

        static public float Range()
        {
            Iteration++;
            return (float)random.NextDouble();
        }

        static public List<T> RandList<T>(List<T> list)
        {
            List<T> result = new List<T>();

            int iNoise = Range(0, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                int index = (i + iNoise) % list.Count;
                result.Add(list[index]);
            }

            return result;
        }

        static public T[] RandList<T>(T[] array)
        {
            List<T> result = new List<T>();

            int iNoise = Range(0, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                int index = (i + iNoise) % array.Length;
                result.Add(array[index]);
            }

            return result.ToArray();
        }

        static private int SeedToInt()
        {
            int seedToInt = 0;
            for (int i = 0; i < seed.Length; i++)
            {
                seedToInt += seed[i] * (int)math.pow(10, i);
            }

            return seedToInt;
        }
    }
}