using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class EvolutionSaveManager : MonoBehaviour
{
    public static EvolutionSaveManager Instance;

    private string savePath;

    public BrainSaveData cachedBrains;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        savePath =
            Application.persistentDataPath +
            "/EliteBrains.json";
    }

    private void Start()
    {
        LoadBrainsIntoScene();
    }

    // =====================================================
    // SAVE ON PLAY STOP
    // =====================================================

    private void OnApplicationQuit()
    {
        SaveTopBrains();
    }

    // =====================================================
    // SAVE TOP 3 NPCs
    // =====================================================

    public void SaveTopBrains()
    {
        AgentRewardSystem[] allAgents =
            FindObjectsOfType<AgentRewardSystem>();

        // Alive agents only
        var aliveAgents =
            allAgents.Where(agent =>
            {
                HealthComponent health =
                    agent.GetComponent<HealthComponent>();

                return health != null &&
                       !health.Model.IsDead;
            });

        // Sort by highest MMR
        var topAgents =
            aliveAgents
            .OrderByDescending(a => a.MMR)
            .Take(3)
            .ToList();

        BrainSaveData saveData =
            new BrainSaveData();

        foreach (var agent in topAgents)
        {
            CopyNeuralNetwork nn =
                agent.GetComponent<CopyNeuralNetwork>();

            if (nn == null)
                continue;

            EliteBrainData brain =
                ExtractBrain(nn);

            brain.mmr = agent.MMR;

            saveData.eliteBrains.Add(brain);

            Debug.Log(
                $"Saved Brain: {agent.name} | MMR: {agent.MMR}"
            );
        }

        string json =
            JsonUtility.ToJson(saveData, true);

        File.WriteAllText(savePath, json);

        Debug.Log("Elite brains saved!");
    }

    //To load brians from previous sessions
    public BrainSaveData LoadBrains()
    {
        if (cachedBrains != null)
            return cachedBrains;

        if (!File.Exists(savePath))
        {
            Debug.Log("No saved brains found.");
            return null;
        }

        string json =
            File.ReadAllText(savePath);

        cachedBrains =
            JsonUtility.FromJson<BrainSaveData>(json);

        Debug.Log(
            $"Loaded {cachedBrains.eliteBrains.Count} elite brains."
        );

        return cachedBrains;
    }

    //Load Brains into the scene
    public void LoadBrainsIntoScene()
    {
        BrainSaveData data = LoadBrains();

        if (data == null)
            return;

        CopyNeuralNetwork[] agents =
            FindObjectsOfType<CopyNeuralNetwork>()
            .OrderBy(a => a.gameObject.name)
            .ToArray();

        if (data.eliteBrains.Count == 0)
            return;

        for (int i = 0; i < agents.Length; i++)
        {
            // Cycle through elite brains
            EliteBrainData brain =
                data.eliteBrains[
                    i % data.eliteBrains.Count
                ];

            agents[i].LoadBrain(
                brain.weights,
                brain.biases
            );

            // Restore inherited MMR
            AgentRewardSystem reward =
                agents[i].GetComponent<AgentRewardSystem>();

            if (reward != null)
            {
                reward.MMR = brain.mmr;
            }

            // MUTATE CHILD
            NeuralMutation mutation =
                agents[i].GetComponent<NeuralMutation>();

            if (mutation != null)
            {
                mutation.MutateCreature();
            }

            Debug.Log(
                $"Loaded evolved brain into: " +
                agents[i].gameObject.name
            );
        }
    }

    // =====================================================
    // EXTRACT NN DATA
    // =====================================================

    private EliteBrainData ExtractBrain(
        CopyNeuralNetwork nn)
    {
        EliteBrainData data =
            new EliteBrainData();

        data.networkShape = nn.networkShape;

        List<float> weights =
            new List<float>();

        List<float> biases =
            new List<float>();

        foreach (var layer in nn.layers)
        {
            int rows =
                layer.WeightsArray.GetLength(0);

            int cols =
                layer.WeightsArray.GetLength(1);

            // Flatten weights
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    weights.Add(
                        layer.WeightsArray[i, j]);
                }
            }

            // Biases
            foreach (float b in layer.BiasesArray)
            {
                biases.Add(b);
            }
        }

        data.weights = weights.ToArray();
        data.biases = biases.ToArray();

        return data;
    }
}