using System;
using System.Collections.Generic;

public static class AlgoritmoGenetico
{
    // Selecciona los mejores drones
    public static List<int> SelectElites(List<float> fitnessList, int eliteCount)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < fitnessList.Count; i++)
        {
            indices.Add(i);
        }


        
        // Ordena de mayor a menor fitness
        for (int i = 0; i < indices.Count - 1; i++)
        {
            for (int j = 0; j < indices.Count - i - 1; j++)
            {
                int idxA = indices[j];
                int idxB = indices[j + 1];
                if (fitnessList[idxA] < fitnessList[idxB])
                {
                    int temp = indices[j];
                    indices[j] = indices[j + 1];
                    indices[j + 1] = temp;
                }
            }
        }


        List<int> elites = new List<int>();
        for (int i = 0; i < eliteCount; i++)
        {
            elites.Add(indices[i]);
        }
        return elites;
    }


    // Cruza simple elemento a elemento usando RNG compartido
    public static RedNeuronal Breed(RedNeuronal parentA, RedNeuronal parentB, System.Random rng)
    {
        RedNeuronal child = RedNeuronal.Crossover(parentA, parentB, rng);
        return child;
    }


    // Mutación de un individuo
    public static void Mutate(RedNeuronal individual, float rate, float strength, System.Random rng)
    {
        
    }
}