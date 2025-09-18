using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class RedNeuronal 
{
    public int[] layers;
    public float[][] neurons;
    public float[][] biases;
    public float[][][] weights;

    private System.Random rng;  //random pesos conexiones, sesgos + diversidad

    public RedNeuronal (int[] layersDefinition, int? seed = null)
    {


        // Copiamos la definición para evitar modificaciones externas
        layers = new int[layersDefinition.Length];
        for (int i = 0; i < layersDefinition.Length; i++)
        {
            layers[i] = layersDefinition[i];
        }


        // Inicializamos RNG
        if (seed.HasValue)
        {
            rng = new System.Random(seed.Value);
        }
        else
        {
            rng = new System.Random(Environment.TickCount);
        }


        // Creamos las estructuras de datos
        CreateNeurons();
        CreateBiases();
        CreateWeights();
    }

    private void CreateNeurons()
    {
        neurons = new float[layers.Length][];
        for (int i = 0; i < layers.Length; i++)
        {
            int neuronCount = layers[i];
            neurons[i] = new float[neuronCount];
            for (int n = 0; n < neuronCount; n++)
            {
                neurons[i][n] = 0f;
            }
        }
    }

    private void CreateBiases() //más flexibilidad + puntos de partida diferentes + rendimiento
    {
        biases = new float[layers.Length][]; //inicializar valor
        for (int i = 0; i < layers.Length; i++) //random
        {
            int neuronCount = layers[i];
            biases[i] = new float[neuronCount];
            for (int n = 0; n < neuronCount; n++)
            {
                
                if (i == 0)
                {
                    biases[i][n] = 0f;
                }
                else
                {
                    // Bias inicial pequeño en rango [-0.5, 0.5]
                    float b = (float)(rng.NextDouble() - 0.5);
                    biases[i][n] = b;
                }
            }
        }
    }


    private void CreateWeights()
    {
        weights = new float[layers.Length][][];
        for (int i = 0; i < layers.Length; i++)
        {
            if (i == 0)
            {
                
                weights[i] = new float[0][];
            }
            else
            {
                int neuronCount = layers[i];
                int prevCount = layers[i - 1];


                weights[i] = new float[neuronCount][];
                for (int n = 0; n < neuronCount; n++)
                {
                    weights[i][n] = new float[prevCount];
                    for (int p = 0; p < prevCount; p++)
                    {
                        // Pesos iniciales 
                        float w = (float)(rng.NextDouble() * 2.0 - 1.0);
                        weights[i][n][p] = w;
                    }
                }
            }
        }
    }


    public float[] FeedForward(float[] inputs)
    {
      


        //inputs en la capa 0
        for (int i = 0; i < layers[0]; i++)
        {
            neurons[0][i] = inputs[i];
        }


        // Propagamos hacia adelante
        for (int layer = 1; layer < layers.Length; layer++)
        {
            int neuronCount = layers[layer];
            int prevCount = layers[layer - 1];


            for (int n = 0; n < neuronCount; n++)
            {
                float sum = 0f;
                for (int p = 0; p < prevCount; p++)
                {
                    float prevValue = neurons[layer - 1][p];
                    float w = weights[layer][n][p];
                    sum += w * prevValue;
                }
                sum += biases[layer][n];


                //rangos simetrcios equilibrio en valores se ajusta a mayor precision 
                float activated = Tanh(sum);
                neurons[layer][n] = activated;
            }
        }


        // Devolvemos copia de la capa de salida
        int last = layers.Length - 1;
        int outCount = layers[last];
        float[] outputs = new float[outCount];
        for (int i = 0; i < outCount; i++)
        {
            outputs[i] = neurons[last][i];
        }
        return outputs;
    }


    private float Tanh(float x)
    {
        float ePos = Mathf.Exp(x);
        float eNeg = Mathf.Exp(-x);
        return (ePos - eNeg) / (ePos + eNeg);
    }


    public RedNeuronal CloneDeep() //copias red con idenpendencia 
    {
        RedNeuronal clone = new RedNeuronal(layers);


        for (int i = 0; i < layers.Length; i++)
        {
            for (int n = 0; n < layers[i]; n++)
            {
                clone.neurons[i][n] = neurons[i][n];
                clone.biases[i][n] = biases[i][n];
            }
        }


        for (int i = 1; i < layers.Length; i++)
        {
            for (int n = 0; n < layers[i]; n++)
            {
                int prevCount = layers[i - 1];
                for (int p = 0; p < prevCount; p++)
                {
                    clone.weights[i][n][p] = weights[i][n][p];
                }
            }
        }


        return clone;
    }

    public static RedNeuronal Crossover(RedNeuronal a, RedNeuronal b, System.Random rng) //cruza entre a y b para más decendencia
    {
       

       RedNeuronal child = new RedNeuronal(a.layers); //la cria 


        // Para cada bias escoge de A o B
        for (int i = 1; i < a.layers.Length; i++)
        {
            int neuronCount = a.layers[i];
            for (int n = 0; n < neuronCount; n++)
            {
                bool takeA = rng.NextDouble() < 0.5;
                if (takeA)
                {
                    child.biases[i][n] = a.biases[i][n];
                }
                else
                {
                    child.biases[i][n] = b.biases[i][n];
                }
            }
        }


        // Para cada peso escoge de A o B
        for (int i = 1; i < a.layers.Length; i++)
        {
            int neuronCount = a.layers[i];
            int prevCount = a.layers[i - 1];
            for (int n = 0; n < neuronCount; n++)
            {
                for (int p = 0; p < prevCount; p++)
                {
                    bool takeB = rng.NextDouble() < 0.5;
                    if (takeB)
                    {
                        child.weights[i][n][p] = a.weights[i][n][p];
                    }
                    else
                    {
                        child.weights[i][n][p] = b.weights[i][n][p];
                    }
                }
            }
        }


        return child;
    }

    public void Mutate(float rate, float strength, System.Random providedRng = null) //basicamente un q probabilidad hay + variabilidad.
    {
        System.Random localRng = providedRng != null ? providedRng : rng;


        if (rate < 0f) rate = 0f;
        if (rate > 1f) rate = 1f;
        if (strength < 0f) strength = 0f;


        for (int i = 1; i < layers.Length; i++)
        {
            int neuronCount = layers[i];
            int prevCount = layers[i - 1];


            for (int n = 0; n < neuronCount; n++)
            {
                // Mutamos bias con probabilidad "rate"
                double rb = localRng.NextDouble();
                if (rb < rate)
                {
                    // Variación en rango [-strength, strength]
                    float deltaB = (float)(localRng.NextDouble() * 2.0 - 1.0) * strength;
                    biases[i][n] += deltaB;
                }


                // Mutamos cada peso
                for (int p = 0; p < prevCount; p++)
                {
                    double rw = localRng.NextDouble();
                    if (rw < rate)
                    {
                        float deltaW = (float)(localRng.NextDouble() * 2.0 - 1.0) * strength;
                        weights[i][n][p] += deltaW;
                    }
                }
            }
        }
    }
}
