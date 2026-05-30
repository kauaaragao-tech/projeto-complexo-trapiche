using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorSceneLoader : MonoBehaviour
{
    [Header("Jogador")]
    public Transform jogador;
    public float distanciaParaInteragir = 2f;

    [Header("Cena e Spawn")]
    public string nomeDaCenaDestino;
    public string nomeDoSpawnDestino;

    [Header("Input")]
    public Key teclaTeste = Key.E;

    [Header("Fade Opcional")]
    public Image fadeImage;
    public float duracaoFade = 1f;

    private bool estaCarregando;
    private bool botaoAEstavaPressionado;

    private void Start()
    {
        if (jogador == null && Camera.main != null)
            jogador = Camera.main.transform;

        DefinirAlphaFade(0f);
    }

    private void Update()
    {
        if (estaCarregando || jogador == null)
            return;

        float distancia = Vector3.Distance(jogador.position, transform.position);

        if (distancia > distanciaParaInteragir)
            return;

        if (ApertouInteracao())
        {
            StartCoroutine(CarregarCenaComTransicao());
        }
    }

    private bool ApertouInteracao()
    {
        bool apertouTeclado = Keyboard.current != null &&
                              Keyboard.current[teclaTeste].wasPressedThisFrame;

        bool apertouGamepad = Gamepad.current != null &&
                              Gamepad.current.buttonSouth.wasPressedThisFrame;

        bool botaoAAtual = false;

        UnityEngine.XR.InputDevice controleDireito =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);

        if (controleDireito.isValid)
        {
            controleDireito.TryGetFeatureValue(
                UnityEngine.XR.CommonUsages.primaryButton,
                out botaoAAtual
            );
        }

        bool apertouBotaoAQuest = botaoAAtual && !botaoAEstavaPressionado;
        botaoAEstavaPressionado = botaoAAtual;

        return apertouTeclado || apertouGamepad || apertouBotaoAQuest;
    }

    private IEnumerator CarregarCenaComTransicao()
    {
        estaCarregando = true;

        PlayerSpawnManager.spawnDestino = nomeDoSpawnDestino;

        if (fadeImage != null)
        {
            float tempo = 0f;

            while (tempo < duracaoFade)
            {
                tempo += Time.deltaTime;
                float progresso = Mathf.Clamp01(tempo / duracaoFade);

                DefinirAlphaFade(progresso);

                yield return null;
            }
        }

        SceneManager.LoadScene(nomeDaCenaDestino);
    }

    private void DefinirAlphaFade(float alpha)
    {
        if (fadeImage == null)
            return;

        Color cor = fadeImage.color;
        cor.a = alpha;
        fadeImage.color = cor;
    }
}