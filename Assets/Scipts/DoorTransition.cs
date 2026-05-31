using UnityEngine;

public class DoorTransition : MonoBehaviour
{
    [SerializeField]
    private string sceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            SceneTransitionManager.Instance.LoadScene(sceneToLoad);
        }
    }
}