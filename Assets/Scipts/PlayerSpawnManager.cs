using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static string spawnDestino;

    [Header("Jogador")]
    public Transform jogador;

    private void Start()
    {
        if (jogador == null && Camera.main != null)
            jogador = Camera.main.transform;

        if (string.IsNullOrEmpty(spawnDestino))
            return;

        GameObject pontoSpawn = GameObject.Find(spawnDestino);

        if (pontoSpawn == null)
        {
            Debug.LogWarning("Spawn nao encontrado: " + spawnDestino);
            return;
        }

        jogador.position = pontoSpawn.transform.position;
        jogador.rotation = pontoSpawn.transform.rotation;

        spawnDestino = null;
    }
}