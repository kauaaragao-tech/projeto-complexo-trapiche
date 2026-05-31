using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimeShiftController : MonoBehaviour
{
    [Header("Salas")]
    public GameObject salaPresente;
    public GameObject salaPassado;

    [Header("Efeito Visual")]
    public Volume volumeTransicao;
    public float duracaoEntrada = 1.2f;
    public float tempoTelaPreta = 1.5f;
    public float duracaoSaida = 1.2f;

    [Header("Efeitos Extras Opcionais")]
    public Light luzTemporal;
    public ParticleSystem particulasTemporais;
    public AudioSource somTransicao;

    private bool estaNoPassado;
    private bool estaTransicionando;
    private bool botaoAEstavaPressionado;

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Bloom bloom;

    private void Awake()
    {
        if (volumeTransicao != null && volumeTransicao.profile != null)
        {
            volumeTransicao.profile.TryGet(out colorAdjustments);
            volumeTransicao.profile.TryGet(out vignette);
            volumeTransicao.profile.TryGet(out chromaticAberration);
            volumeTransicao.profile.TryGet(out lensDistortion);
            volumeTransicao.profile.TryGet(out bloom);
        }
    }

    private void Start()
    {
        AplicarEstadoDasSalas();
        AplicarVisualFinal();

        if (luzTemporal != null)
            luzTemporal.intensity = 0f;
    }

    private void Update()
    {
        bool apertouTeclado = Keyboard.current != null &&
                              Keyboard.current.tKey.wasPressedThisFrame;

        bool apertouBotaoA = Gamepad.current != null &&
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

        if ((apertouTeclado || apertouBotaoA || apertouBotaoAQuest) && !estaTransicionando)
        {
            StartCoroutine(RupturaTemporal());
        }
    }

    private IEnumerator RupturaTemporal()
    {
        estaTransicionando = true;

        if (somTransicao != null)
            somTransicao.Play();

        if (particulasTemporais != null)
            particulasTemporais.Play();

        yield return EfeitoEntrada();

        AplicarTelaPreta();

        yield return new WaitForSeconds(0.15f);

        estaNoPassado = !estaNoPassado;
        AplicarEstadoDasSalas();

        AplicarVisualFinal();
        AplicarTelaPreta();

        yield return new WaitForSeconds(tempoTelaPreta);

        yield return EfeitoSaida();

        AplicarVisualFinal();

        if (luzTemporal != null)
            luzTemporal.intensity = 0f;

        estaTransicionando = false;
    }

    private IEnumerator EfeitoEntrada()
    {
        float tempo = 0f;

        while (tempo < duracaoEntrada)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracaoEntrada);
            float pulsoPrincipal = Mathf.Sin(progresso * Mathf.PI);
            float pulsoRapido = Mathf.Abs(Mathf.Sin(progresso * Mathf.PI * 8f));
            float energia = Mathf.Clamp01(pulsoPrincipal + pulsoRapido * 0.25f);

            AplicarEfeitoRuptura(energia, pulsoPrincipal);

            if (colorAdjustments != null)
                colorAdjustments.postExposure.value = Mathf.Lerp(0f, -8f, progresso);

            yield return null;
        }
    }

    private IEnumerator EfeitoSaida()
    {
        float tempo = 0f;

        while (tempo < duracaoSaida)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracaoSaida);
            float inverso = 1f - progresso;
            float pulsoRapido = Mathf.Abs(Mathf.Sin(progresso * Mathf.PI * 8f));
            float energia = Mathf.Clamp01(inverso + pulsoRapido * 0.25f);

            AplicarEfeitoRuptura(energia, inverso);

            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(-8f, 0f, progresso);
                colorAdjustments.saturation.value = estaNoPassado ? -100f : 0f;
            }

            yield return null;
        }
    }

    private void AplicarEfeitoRuptura(float energia, float pulsoPrincipal)
    {
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0.15f, 0.95f, energia);

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, energia);

        if (lensDistortion != null)
            lensDistortion.intensity.value = Mathf.Lerp(0f, -0.45f, pulsoPrincipal);

        if (bloom != null)
            bloom.intensity.value = Mathf.Lerp(0.4f, 10f, energia);

        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = Mathf.Lerp(0f, 45f, energia);

            float saturacaoFinal = estaNoPassado ? -100f : 0f;
            colorAdjustments.saturation.value = Mathf.Lerp(saturacaoFinal, -80f, energia);

            colorAdjustments.colorFilter.value = Color.Lerp(
                Color.white,
                new Color(0.55f, 0.8f, 1f),
                energia
            );
        }

        if (luzTemporal != null)
            luzTemporal.intensity = Mathf.Lerp(0f, 8f, energia);
    }

    private void AplicarTelaPreta()
    {
        if (vignette != null)
            vignette.intensity.value = 1f;

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 1f;

        if (lensDistortion != null)
            lensDistortion.intensity.value = -0.45f;

        if (bloom != null)
            bloom.intensity.value = 0f;

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = -10f;
            colorAdjustments.contrast.value = 100f;
            colorAdjustments.saturation.value = -100f;
            colorAdjustments.colorFilter.value = Color.black;
        }

        if (luzTemporal != null)
            luzTemporal.intensity = 0f;
    }

    private void AplicarEstadoDasSalas()
    {
        if (salaPresente != null)
            salaPresente.SetActive(!estaNoPassado);

        if (salaPassado != null)
            salaPassado.SetActive(estaNoPassado);
    }

    private void AplicarVisualFinal()
    {
        if (vignette != null)
            vignette.intensity.value = estaNoPassado ? 0.35f : 0.15f;

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;

        if (lensDistortion != null)
            lensDistortion.intensity.value = 0f;

        if (bloom != null)
            bloom.intensity.value = estaNoPassado ? 1.2f : 0.4f;

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = 0f;
            colorAdjustments.saturation.value = estaNoPassado ? -100f : 0f;
            colorAdjustments.contrast.value = estaNoPassado ? 15f : 0f;
            colorAdjustments.colorFilter.value = Color.white;
        }
    }
}
