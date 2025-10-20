using UnityEngine;

public class ExitCheckpoint : MonoBehaviour
{
    
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            GameMode.Instance.Victory();
            
            gameObject.SetActive(false);
        }
    }
}
