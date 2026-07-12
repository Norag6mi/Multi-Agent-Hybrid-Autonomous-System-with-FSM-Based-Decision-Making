using UnityEngine;

public class SimulationSpeedController : MonoBehaviour
{
    [Header("Simulation Speed")]
    [Range(1f, 100f)]
    public float simulationSpeed = 10f;

    void Update()
    {
        Time.timeScale = simulationSpeed;

        // Keeps physics stable
        Time.fixedDeltaTime = 0.02f / simulationSpeed;
    }

    private void OnDestroy()
    {
        // Reset when exiting play mode
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}