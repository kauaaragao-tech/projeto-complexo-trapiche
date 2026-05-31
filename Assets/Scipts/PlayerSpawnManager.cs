using UnityEngine;
using Unity.XR.CoreUtils;

public class PlayerSpawnManager : MonoBehaviour
{
    public static string spawnDestino;

    [Header("Jogador")]
    public Transform jogador;

    private void Start()
    {
        if (string.IsNullOrEmpty(spawnDestino))
            return;

        GameObject pontoSpawn = GameObject.Find(spawnDestino);

        if (pontoSpawn == null)
        {
            Debug.LogWarning("Spawn nao encontrado: " + spawnDestino);
            return;
        }

        XROrigin xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin != null)
        {
            Camera xrCamera = xrOrigin.Camera;

            if (xrCamera != null)
            {
                float diferencaRotacao = pontoSpawn.transform.eulerAngles.y - xrCamera.transform.eulerAngles.y;
                xrOrigin.transform.Rotate(0f, diferencaRotacao, 0f);
                xrOrigin.MoveCameraToWorldLocation(pontoSpawn.transform.position);
            }
            else
            {
                xrOrigin.transform.SetPositionAndRotation(pontoSpawn.transform.position, pontoSpawn.transform.rotation);
            }
        }
        else
        {
            if (jogador == null && Camera.main != null)
                jogador = Camera.main.transform;

            if (jogador != null)
            {
                jogador.position = pontoSpawn.transform.position;
                jogador.rotation = pontoSpawn.transform.rotation;
            }
            else
            {
                Debug.LogWarning("Jogador nao encontrado para aplicar spawn: " + spawnDestino);
            }
        }

        spawnDestino = null;
    }
}
