using UnityEngine;

public class PlayerİnteractionController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent<ICollectible>(out var collectible))
        {
            collectible.Collect();
        }
        
    }
}
