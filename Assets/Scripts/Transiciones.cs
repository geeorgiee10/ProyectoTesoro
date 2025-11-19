using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Transiciones : MonoBehaviour
{
    public Image imageTransicion;
    public float velocidadTransicion = 1f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = imageTransicion.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        canvasGroup.blocksRaycasts = false; // permite tocar botones mientras FadeIn
        StartCoroutine(TransicionEntrar());
    }

    public void CambiarEscena(string nombreEscena)
    {
        StartCoroutine(TransicionSalir(nombreEscena));
    }

    IEnumerator TransicionEntrar()
    {
        float a = 1f;
        while (a > 0)
        {
            a -= Time.deltaTime * velocidadTransicion;
            imageTransicion.color = new Color(0, 0, 0, a);
            yield return null;
        }
        canvasGroup.blocksRaycasts = false; // Asegurar que los botones funcionen
    }

    IEnumerator TransicionSalir(string nombreEscena)
    {
        canvasGroup.blocksRaycasts = true; // Bloquea la pantalla mientras se hace la transicion
        float a = 0f;
        while (a < 1f)
        {
            a += Time.deltaTime * velocidadTransicion;
            imageTransicion.color = new Color(0, 0, 0, a);
            yield return null;
        }
        SceneManager.LoadScene(nombreEscena);
    }
}
