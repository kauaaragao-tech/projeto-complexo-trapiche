using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.TryAdoptFadeImage(fadeImage);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        KeepFadeCanvasWithManager();
        DontDestroyOnLoad(gameObject);
        SetFadeAlpha(0f);
    }

    private void Start()
    {
        if (fadeImage != null)
            StartCoroutine(Fade(1f, 0f));
    }

    public void LoadScene(string sceneName)
    {
        LoadScene(sceneName, null);
    }

    public void LoadScene(string sceneName, string spawnDestino)
    {
        if (!isTransitioning)
        {
            StartCoroutine(Transition(sceneName, spawnDestino));
        }
    }

    private IEnumerator Transition(string sceneName, string spawnDestino)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("SceneTransitionManager recebeu uma cena vazia.");
            yield break;
        }

        isTransitioning = true;

        if (!string.IsNullOrEmpty(spawnDestino))
            PlayerSpawnManager.spawnDestino = spawnDestino;

        yield return StartCoroutine(Fade(0f, 1f));

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogWarning("Nao foi possivel carregar a cena: " + sceneName);
            yield return StartCoroutine(Fade(1f, 0f));
            isTransitioning = false;
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    private IEnumerator Fade(float alphaInicial, float alphaFinal)
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(alphaInicial, alphaFinal, timer / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = alphaFinal;
        fadeImage.color = color;
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void KeepFadeCanvasWithManager()
    {
        if (fadeImage == null)
            return;

        Canvas canvas = fadeImage.GetComponentInParent<Canvas>();

        if (canvas != null && canvas.transform.parent != transform)
            canvas.transform.SetParent(transform, true);
    }

    private void TryAdoptFadeImage(Image candidate)
    {
        if (fadeImage != null || candidate == null)
            return;

        fadeImage = candidate;
        KeepFadeCanvasWithManager();
        SetFadeAlpha(0f);
    }
}
