using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform gunTransform;

    void LateUpdate()
    {
        if (gunTransform != null)
        {
            // This forces the muzzle to match the gun's exact rotation
            transform.rotation = gunTransform.rotation;
        }
    }
}