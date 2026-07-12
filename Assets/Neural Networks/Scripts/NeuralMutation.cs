using UnityEngine;

public class NeuralMutation : MonoBehaviour
{
    [Header("References")]
    public CopyNeuralNetwork nn;

    [Header("Mutation Settings")]
    [Range(0f, 1f)]
    public float mutationChance = 0.03f;

    public float mutationAmount = 0.15f;

    [Header("Evolving Mutation")]
    public bool mutateMutations = true;

    private void Awake()
    {
        if (nn == null)
        {
            nn = GetComponent<CopyNeuralNetwork>();
        }
    }

    // =====================================================
    // MAIN MUTATION FUNCTION
    // =====================================================

    public void MutateCreature()
    {
        if (nn == null)
            return;

        // ============================================
        // EVOLVE THE MUTATION VALUES THEMSELVES
        // ============================================

        if (mutateMutations)
        {
            mutationAmount +=
                Random.Range(-1f, 1f) / 100f;

            mutationChance +=
                Random.Range(-1f, 1f) / 100f;
        }

        // ============================================
        // KEEP VALUES POSITIVE
        // ============================================

        mutationAmount =
            Mathf.Max(mutationAmount, 0.001f);

        mutationChance =
            Mathf.Clamp01(mutationChance);

        // ============================================
        // MUTATE ALL LAYERS
        // ============================================

        MutateNetwork();
    }

    // =====================================================
    // NETWORK MUTATION
    // =====================================================

    private void MutateNetwork()
    {
        for (int i = 0; i < nn.layers.Length; i++)
        {
            MutateLayer(nn.layers[i]);
        }
    }

    // =====================================================
    // LAYER MUTATION
    // =====================================================

    private void MutateLayer(CopyNeuralNetwork.Layer layer)
    {
        int neurons =
            layer.BiasesArray.Length;

        int inputs =
            layer.WeightsArray.GetLength(1);

        // ============================================
        // MUTATE WEIGHTS
        // ============================================

        for (int i = 0; i < neurons; i++)
        {
            for (int j = 0; j < inputs; j++)
            {
                if (Random.value < mutationChance)
                {
                    layer.WeightsArray[i, j] +=
                        Random.Range(-1f, 1f)
                        * mutationAmount;
                }
            }

            // ========================================
            // MUTATE BIAS
            // ========================================

            if (Random.value < mutationChance)
            {
                layer.BiasesArray[i] +=
                    Random.Range(-1f, 1f)
                    * mutationAmount;
            }
        }
    }
}