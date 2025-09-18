using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Prefab del dron (debe tener componente Drone y un Rigidbody)
    public Drone dronePrefab;

    // Población y topología de la red
    public int populationSize = 20;
    // SIN waypoints: usamos 6 inputs (5 sensores + velocidad), 6 ocultas, 3 outputs
    public int[] networkTopology = new int[] { 6, 6, 3 };

    // Parámetros de GA
    public float mutationRate = 0.05f;
    public float mutationStrength = 0.3f;
    [Range(0.05f, 0.5f)] public float eliteFraction = 0.2f; // 20% élite

    // Épocas
    public float epochDuration = 40f; // segundos por época
    private float epochTimer = 0f;

    // Spawn
    public Transform spawnPoint;

    // Estado de simulación
    private List<Drone> drones = new List<Drone>();
    private List<RedNeuronal> genomes = new List<RedNeuronal>();
    private List<float> fitnesses = new List<float>();
    private int generation = 1;
    private System.Random rng;

    // UI opcional
    public ControlUI ui;

    private void Awake()  
    {
    }

    private void Start()
    {
        SpawnGeneration();
    }





    private void SpawnGeneration()
    {
        // Limpieza previa
        ClearDrones();
        fitnesses.Clear();

        // Instanciar drones y asignar cerebros
        for (int i = 0; i < genomes.Count; i++)
        {
            Drone agent = Instantiate(dronePrefab);

            if (spawnPoint != null)
            {
                agent.transform.position = spawnPoint.position;
                agent.transform.rotation = spawnPoint.rotation;
            }
            else
            {
                agent.transform.position = Vector3.zero;
                agent.transform.rotation = Quaternion.identity;
            }

            agent.gameObject.SetActive(true);


            drones.Add(agent);
            fitnesses.Add(0f);
        }

        // Reiniciar temporizador de época
        epochTimer = 0f;

        // UI
        if (ui != null)
        {
            ui.SetGeneration(generation);
            ui.SetAliveCount(drones.Count);
            ui.SetBestFitness(0f);
            ui.SetTimer(epochDuration - epochTimer);
            ui.SetTimeScale(Time.timeScale);
        }

        HighlightBestDrone();
    }


    private void HighlightBestDrone()
    {

        if (drones.Count == 0) return;

        // Resetear todos primero
        foreach (Drone d in drones)
            d.ResetHighlight();

        // Buscar el que tenga mejor fitness
        Drone best = drones[0];
        foreach (Drone d in drones)
        {
            if (d != null && d.fitness > best.fitness)
                best = d;
        }

        // Resaltar al campeon
        if (best != null)
            best.HighlightChampion();

    }

    private void ClearDrones()
    {
        for (int i = 0; i < drones.Count; i++)
        {
            if (drones[i] != null)
            {
                Destroy(drones[i].gameObject);
            }
        }
        drones.Clear();
    }

    
    private void Update()
    {

        HighlightBestDrone();

        
        epochTimer += Time.deltaTime;
        if (ui != null)
        {
            ui.SetTimer(Mathf.Max(0f, epochDuration - epochTimer));
        }

        // Actualizar fitness y contadores
        float best = 0f;
        int alive = 0;
        for (int i = 0; i < drones.Count; i++)
        {
            Drone agent = drones[i];
            if (agent != null)
            {
                fitnesses[i] = agent.fitness;
                if (agent.isAlive)
                {
                    alive++;
                }
                if (agent.fitness > best)
                {
                    best = agent.fitness;
                }
            }
        }
        if (ui != null)
        {
            ui.SetBestFitness(best);
            ui.SetAliveCount(alive);
        }

        // Fin de época si se acaba el tiempo o todos murieron
        bool timeUp = epochTimer >= epochDuration;
        bool allDead = alive == 0;
        if (timeUp || allDead)
        {
            NextGeneration();
        }
    }

   
    public void NotifyDroneDeath(Drone agent)
    {
        if (ui != null)
        {
            int alive = 0;
            for (int i = 0; i < drones.Count; i++)
            {
                if (drones[i] != null && drones[i].isAlive)
                {
                    alive++;
                }
            }
            ui.SetAliveCount(alive);
        }
    }

    
   
    public void Button_ToggleSpeed()
    {
        if (Mathf.Approximately(Time.timeScale, 1f))
        {
            Time.timeScale = 3f; //5
        }
        else
        {
            Time.timeScale = 1f;//3 
        }
        if (ui != null)
        {
            ui.SetTimeScale(Time.timeScale);
        }
    }

   
    private void NextGeneration()
    {
        // Guardar fitness final por si hay rezagos
        for (int i = 0; i < drones.Count; i++)
        {
            if (drones[i] != null)
            {
                fitnesses[i] = drones[i].fitness;
            }
        }

        // Selección de élites
        int eliteCount = Mathf.Clamp(Mathf.RoundToInt(populationSize * eliteFraction), 1, populationSize);
        List<int> eliteIndices = AlgoritmoGenetico.SelectElites(fitnesses, eliteCount);

        // Nueva lista de genomas
        List<RedNeuronal> newGenomes = new List<RedNeuronal>();

        // Copiar élites (elitismo)
        for (int i = 0; i < eliteIndices.Count; i++)
        {
            RedNeuronal eliteClone = genomes[eliteIndices[i]].CloneDeep();
            newGenomes.Add(eliteClone);
        }

        // Rellenar con cruza + mutación
        while (newGenomes.Count < populationSize)
        {
            int aIdx = eliteIndices[rng.Next(0, eliteIndices.Count)];
            int bIdx = eliteIndices[rng.Next(0, eliteIndices.Count)];

            RedNeuronal parentA = genomes[aIdx];
            RedNeuronal parentB = genomes[bIdx];

            RedNeuronal child = AlgoritmoGenetico.Breed(parentA, parentB, rng);
            AlgoritmoGenetico.Mutate(child, mutationRate, mutationStrength, rng);

            newGenomes.Add(child);
        }

        // Reemplazar población y avanzar generación
        genomes = newGenomes;
        generation++;

        // Spawnear siguiente generación
        SpawnGeneration();
    }
}
