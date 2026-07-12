using UnityEngine;

public class BombInteractionManager : MonoBehaviour
{
    public static BombInteractionManager Instance { get; private set; }

    private BombDefuse currentBomb;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool RequestInteraction(BombDefuse bomb)
    {
        if (currentBomb == null)
        {
            currentBomb = bomb;
            return true;
        }

        if (currentBomb == bomb)
            return true;

        return false;
    }

    public void ReleaseInteraction(BombDefuse bomb)
    {
        if (currentBomb == bomb)
            currentBomb = null;
    }

    public bool IsCurrentBomb(BombDefuse bomb)
    {
        return currentBomb == bomb;
    }
}