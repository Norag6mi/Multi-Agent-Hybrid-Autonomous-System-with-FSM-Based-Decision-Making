using UnityEngine;

public class CopyNeuralNetwork : MonoBehaviour
{
    [Header("Neural Network Shape")]
    public int[] networkShape = { 11, 4, 4, 3 };

    public Layer[] layers;

    [System.Serializable]
    public class Layer
    {
        public float[,] WeightsArray;
        public float[] BiasesArray;
        public float[] NodeArray;

        private int n_nodes;
        private int n_inputs;

        public Layer(int n_inputs, int n_nodes)
        {
            this.n_nodes = n_nodes;
            this.n_inputs = n_inputs;

            WeightsArray = new float[n_nodes, n_inputs];
            BiasesArray = new float[n_nodes];
            NodeArray = new float[n_nodes];

            // Random initialization
            for (int i = 0; i < n_nodes; i++)
            {
                BiasesArray[i] = Random.Range(-1f, 1f);

                for (int j = 0; j < n_inputs; j++)
                {
                    WeightsArray[i, j] = Random.Range(-1f, 1f);
                }
            }
        }

        // Forward Pass
        public void Forward(float[] InputsArray)
        {
            NodeArray = new float[n_nodes];

            for (int i = 0; i < n_nodes; i++)
            {
                for (int j = 0; j < n_inputs; j++)
                {
                    NodeArray[i] += WeightsArray[i, j] * InputsArray[j];
                }

                NodeArray[i] += BiasesArray[i];
            }
        }

        // ReLU Activation
        public void Activation()
        {
            for (int i = 0; i < n_nodes; i++)
            {
                if (NodeArray[i] < 0)
                {
                    NodeArray[i] = 0;
                }
            }
        }
    }

    void Awake()
    {
        // Validation
        if (networkShape.Length < 2)
        {
            Debug.LogError("Network must contain at least Input and Output layer.");
            return;
        }

        for (int i = 0; i < networkShape.Length; i++)
        {
            if (networkShape[i] <= 0)
            {
                Debug.LogError("Layer size cannot be 0 or negative.");
                return;
            }
        }

        // Create Layers Dynamically
        layers = new Layer[networkShape.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i + 1]);
        }

        Debug.Log("Neural Network Created Successfully!");
    }

    public float[] Brain(float[] inputs)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (i == 0)
            {
                layers[i].Forward(inputs);
                layers[i].Activation();
            }
            else if (i == layers.Length - 1)
            {
                layers[i].Forward(layers[i - 1].NodeArray);
            }
            else
            {
                layers[i].Forward(layers[i - 1].NodeArray);
                layers[i].Activation();
            }
        }

        return layers[layers.Length - 1].NodeArray;
    }

//To load from save files of previous training sessions
    public void LoadBrain(float[] flatWeights, float[] flatBiases)
    {
        // 1. Calculate how many elements are expected for the CURRENT network architecture
        int expectedWeightsCount = 0;
        int expectedBiasesCount = 0;
        
        for (int l = 0; l < layers.Length; l++)
        {
            expectedWeightsCount += layers[l].WeightsArray.GetLength(0) * layers[l].WeightsArray.GetLength(1);
            expectedBiasesCount += layers[l].BiasesArray.GetLength(0);
        }

        // 2. Validate incoming arrays against expected counts
        if (flatWeights.Length != expectedWeightsCount || flatBiases.Length != expectedBiasesCount)
        {
            Debug.LogWarning($"[Brain Load Failed] Mismatch on {gameObject.name}. " +
                            $"Network expects {expectedWeightsCount} weights & {expectedBiasesCount} biases. " +
                            $"Save file provided {flatWeights.Length} weights & {flatBiases.Length} biases. " +
                            $"Falling back to random brain initialization for a fresh training session.");
            return; // Abort load safely so random initialization from Awake remains active
        }

        int weightIndex = 0;
        int biasIndex = 0;

        for (int l = 0; l < layers.Length; l++)
        {
            int rows = layers[l].WeightsArray.GetLength(0);
            int cols = layers[l].WeightsArray.GetLength(1);

            // Restore weights
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    layers[l].WeightsArray[i, j] = flatWeights[weightIndex];
                    weightIndex++;
                }
            }

            // Restore biases
            for (int i = 0; i < rows; i++)
            {
                layers[l].BiasesArray[i] = flatBiases[biasIndex];
                biasIndex++;
            }
        }

        Debug.Log($"{gameObject.name} brain loaded successfully!");
    }
}