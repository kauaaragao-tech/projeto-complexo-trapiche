using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TimeShiftController : MonoBehaviour
{
    [Header("Salas")]
    public GameObject salaPresente;
    public GameObject salaPassado;

    [Header("Efeito Visual")]
    public Volume volumeTransicao;
    public Image fadeImage;
    public float duracaoTransicao = 2.6f;
    public float tempoTelaOculta = 0.45f;

    [Header("Efeitos Extras Opcionais")]
    public Light luzTemporal;
    public ParticleSystem particulasTemporais;
    public AudioSource somTransicao;

    private bool estaNoPassado;
    private bool estaTransicionando;

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
        PrepararFadeImage();

        AplicarEstadoDasSalas();
        AplicarVisualFinal();

        if (luzTemporal != null)
            luzTemporal.intensity = 0f;
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame &&
            !estaTransicionando)
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

        yield return InicioDaRuptura();

        DefinirAlphaFade(1f);

        yield return new WaitForSeconds(0.08f);

        estaNoPassado = !estaNoPassado;
        AplicarEstadoDasSalas();
        AplicarVisualFinal();

        yield return new WaitForSeconds(tempoTelaOculta);

        yield return FimDaRuptura();

        AplicarVisualFinal();
        DefinirAlphaFade(0f);

        if (luzTemporal != null)
            luzTemporal.intensity = 0f;

        estaTransicionando = false;
    }

    private IEnumerator InicioDaRuptura()
    {
        float tempo = 0f;
        float duracao = duracaoTransicao * 0.45f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);
            float pulso = Mathf.Sin(progresso * Mathf.PI);
            float tremor = Mathf.Abs(Mathf.Sin(progresso * Mathf.PI * 12f));

            DefinirAlphaFade(Mathf.Lerp(0f, 1f, progresso));
            AplicarEfeitoRuptura(progresso, pulso, tremor);

            yield return null;
        }
    }

    private IEnumerator FimDaRuptura()
    {
        float tempo = 0f;
        float duracao = duracaoTransicao * 0.55f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);
            float inverso = 1f - progresso;
            float pulso = Mathf.Sin(progresso * Mathf.PI);
            float tremor = Mathf.Abs(Mathf.Sin(progresso * Mathf.PI * 10f));

            DefinirAlphaFade(Mathf.Lerp(1f, 0f, progresso));
            AplicarEfeitoRuptura(inverso, pulso, tremor);

            yield return null;
        }
    }

    private void AplicarEfeitoRuptura(float intensidade, float pulso, float tremor)
    {
        float energia = Mathf.Clamp01(intensidade + tremor * 0.2f);

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0.15f, 0.95f, energia);

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, energia);

        if (lensDistortion != null)
            lensDistortion.intensity.value = Mathf.Lerp(0f, -0.55f, energia);

        if (bloom != null)
            bloom.intensity.value = Mathf.Lerp(0.4f, 12f, energia);

        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = Mathf.Lerp(0f, 45f, energia);
            colorAdjustments.saturation.value = Mathf.Lerp(0f, -80f, energia);
            colorAdjustments.colorFilter.value = Color.Lerp(
                Color.white,
                new Color(0.55f, 0.8f, 1f),
                energia
            );
        }

        if (luzTemporal != null)
            luzTemporal.intensity = Mathf.Lerp(0f, 10f, energia);
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
            colorAdjustments.saturation.value = estaNoPassado ? -100f : 0f;
            colorAdjustments.contrast.value = estaNoPassado ? 15f : 0f;
            colorAdjustments.colorFilter.value = Color.white;
        }
    }

    private void PrepararFadeImage()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("Fade Image nao foi colocada no Inspector.");
            return;
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeImage.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void DefinirAlphaFade(float alpha)
    {
        if (fadeImage == null)
            return;

        fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }
}