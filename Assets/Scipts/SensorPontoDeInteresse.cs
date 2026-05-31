using UnityEngine;

public class SensorPontoDeInteresse : MonoBehaviour
{
    // Arrastaremos o Canvas para cá no Inspector
    [SerializeField] private GameObject painelInformativo;

    // Quando algo entra no sensor
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem entrou foi o jogador (geralmente a Main Camera ou o Rig VR)
        if (other.CompareTag("MainCamera") || other.gameObject.layer == LayerMask.NameToLayer("XR Rig"))
        {
            if (painelInformativo != null)
            {
                painelInformativo.SetActive(true); // Mostra o painel
            }
        }
    }

    // Quando o jogador se afasta e sai do sensor
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.gameObject.layer == LayerMask.NameToLayer("XR Rig"))
        {
            if (painelInformativo != null)
            {
                painelInformativo.SetActive(false); // Esconde o painel novamente
            }
        }
    }
}