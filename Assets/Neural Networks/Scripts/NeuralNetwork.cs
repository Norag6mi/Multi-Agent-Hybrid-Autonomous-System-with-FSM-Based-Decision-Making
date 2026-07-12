using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    public int [] networkShape = {2,4,4,2};
    public Layer [] layers;
    public class Layer
    {
        public float[,] WeightsArray;
        public float[] BiasesArray;
        public float[] NodeArray;
        private int n_nodes;
        private int n_inputs;

        public Layer(int n_inputs,int n_nodes)
        {
            this.n_nodes = n_nodes;
            this.n_inputs = n_inputs;

            WeightsArray = new float [n_nodes,n_inputs];
            BiasesArray = new float [n_nodes];
            NodeArray = new float [n_nodes];
        }

        public void Forward(float [] InputsArray)            //forward pass function
        {
            NodeArray = new float [n_nodes];
            for(int i=0;i<n_nodes;i++)
            {
                for(int j=0;j<n_inputs;j++)
                {
                    NodeArray[i] += WeightsArray[i,j]*InputsArray[j];
                }
                NodeArray[i] += BiasesArray[i];
            }
        }

        public void Activation()                              //activation function
        {
            for(int i=0;i<n_nodes;i++)
            {
                if(NodeArray[i] < 0)
                {
                    NodeArray[i] = 0;
                }
            }
        }
    }

    public void Awake()
    {
        layers = new Layer[networkShape.Length - 1];
        for(int i=0;i<layers.Length;i++)
        {
            layers[i] = new Layer(networkShape[i],networkShape[i+1]);
        }

    }

    public float[] Brain(float [] inputs)
        {
            for(int i=0;i<layers.Length;i++)
            {
                if(i == 0)
                {
                    layers[i].Forward(inputs);
                    layers[i].Activation();
                }
                else if(i == layers.Length - 1)
                {
                    layers[i].Forward(layers[i-1].NodeArray);
                }
                else
                {
                    layers[i].Forward(layers[i-1].NodeArray);
                    layers[i].Activation();
                }
            }
            return(layers[layers.Length - 1].NodeArray);
        }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
