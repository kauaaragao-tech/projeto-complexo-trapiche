using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    [Header("Painel de Créditos")]
    public GameObject painelCreditos;

    public void IniciarTour()
    {
        Debug.Log("Botão INICIAR TOUR foi clicado!");

        SceneManager.LoadScene("Praça");
    }

    public void AbrirCreditos()
    {
        Debug.Log("Botão CRÉDITOS foi clicado!");

        painelCreditos.SetActive(true);
    }

    public void FecharCreditos()
    {
        Debug.Log("Botão VOLTAR foi clicado!");

        painelCreditos.SetActive(false);
    }

    public void Sair()
    {
        Debug.Log("Botão SAIR foi clicado!");

        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #else
        Application.Quit();
        #endif
    }
}