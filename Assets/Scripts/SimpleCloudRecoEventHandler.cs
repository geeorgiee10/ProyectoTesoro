using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using UnityEngine.Networking;
using System.IO;


public class MetaDatos
{
    public string nombre;
    public string url;
    public string adivinanza;
    public string respuesta;
    public bool esPrimera;

    public static MetaDatos CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<MetaDatos>(jsonString);
    }

}


public class SimpleCloudRecoEventHandler : MonoBehaviour
{

    CloudRecoBehaviour mCloudRecoBehaviour;
    bool mIsScanning = false;

    string siguienteRespuesta = "";
    bool juegoIniciado = false;



    int vidas = 3;

    [SerializeField] private TextMeshProUGUI textoCanva;
    [SerializeField] private TextMeshProUGUI vidasTexto;

    public ImageTargetBehaviour ImageTargetTemplate;

    GameObject modeloActual;


    void Start()
    {
        textoCanva.text = "Busca la primera pelicula para empezar la busqueda";
        vidasTexto.text = "Vidas: " + vidas;
    }

    // Register cloud reco callbacks
    void Awake()
    {
        mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        mCloudRecoBehaviour.RegisterOnInitializedEventHandler(OnInitialized);
        mCloudRecoBehaviour.RegisterOnInitErrorEventHandler(OnInitError);
        mCloudRecoBehaviour.RegisterOnUpdateErrorEventHandler(OnUpdateError);
        mCloudRecoBehaviour.RegisterOnStateChangedEventHandler(OnStateChanged);
        mCloudRecoBehaviour.RegisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }
    //Unregister cloud reco callbacks when the handler is destroyed
    void OnDestroy()
    {
        mCloudRecoBehaviour.UnregisterOnInitializedEventHandler(OnInitialized);
        mCloudRecoBehaviour.UnregisterOnInitErrorEventHandler(OnInitError);
        mCloudRecoBehaviour.UnregisterOnUpdateErrorEventHandler(OnUpdateError);
        mCloudRecoBehaviour.UnregisterOnStateChangedEventHandler(OnStateChanged);
        mCloudRecoBehaviour.UnregisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }

    public void OnInitialized(CloudRecoBehaviour cloudRecoBehaviour)
    {
        Debug.Log("Cloud Reco initialized");
    }

    public void OnInitError(CloudRecoBehaviour.InitError initError)
    {
        Debug.Log("Cloud Reco init error " + initError.ToString());
    }

    public void OnUpdateError(CloudRecoBehaviour.QueryError updateError)
    {
        Debug.Log("Cloud Reco update error " + updateError.ToString());

    }

    public void OnStateChanged(bool scanning)
    {
        mIsScanning = scanning;

        if (scanning)
        {
            // Clear all known targets
        }
    }

    IEnumerator GetAssetBundle(string url)
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            string[] allAssetNames = bundle.GetAllAssetNames();
            string gameObjectName = Path.GetFileNameWithoutExtension(allAssetNames[0]).ToString();
            GameObject objectFound = bundle.LoadAsset(gameObjectName) as GameObject;
            

            modeloActual = Instantiate(objectFound, new Vector3(0f, 0f, 0f), transform.rotation);

            modeloActual.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f); 

        }
    }

    // Here we handle a cloud target recognition event
    public void OnNewSearchResult(CloudRecoBehaviour.CloudRecoSearchResult cloudRecoSearchResult)
    {
        MetaDatos datos;

        datos = MetaDatos.CreateFromJSON(cloudRecoSearchResult.MetaData);

        //mCloudRecoBehaviour.enabled = false;

        if (ImageTargetTemplate != null)
        {
            /* Enable the new result with the same ImageTargetBehaviour: */
            mCloudRecoBehaviour.EnableObservers(cloudRecoSearchResult, ImageTargetTemplate.gameObject);
        }

        if(!juegoIniciado && datos.esPrimera)
        {
            juegoIniciado = true;
            StartCoroutine(Acierto(datos));
            siguienteRespuesta = datos.respuesta;
            return;
        }
        
        if(!juegoIniciado && !datos.esPrimera)
        {
            StartCoroutine(FallarPrimera());
            return;
        }

        if(datos.nombre == siguienteRespuesta)
        {
            siguienteRespuesta = datos.respuesta;
            StartCoroutine(Acierto(datos));
        }
        else
        {
            StartCoroutine(Fallo());
        }

        //StartCoroutine(GetAssetBundle(datos.url));
        
    }

    IEnumerator Acierto(MetaDatos datos)
    {
        textoCanva.text = "¡Correcto! ¡Has Acertado!";

        if (modeloActual != null) 
            Destroy(modeloActual);

        yield return StartCoroutine(GetAssetBundle(datos.url));

        yield return new WaitForSeconds(4f);

        textoCanva.text = "Adivinanza:\n\n" + datos.adivinanza;
        siguienteRespuesta = datos.respuesta;

        mCloudRecoBehaviour.ClearObservers();
        mCloudRecoBehaviour.enabled = false;
        yield return new WaitForSeconds(1f);
        mCloudRecoBehaviour.enabled = true;
    }

    IEnumerator FallarPrimera()
    {
        textoCanva.text = "¡Lamentablemente esta no es la primera imagen, prueba con otra pelicula!";

        yield return new WaitForSeconds(2f);

        mCloudRecoBehaviour.enabled = true;
    }

    IEnumerator Fallo()
    {
        vidas--;
        vidasTexto.text = "Vidas: " + vidas;

        textoCanva.text = "¡Has Fallado!";
        yield return new WaitForSeconds(1.5f);

        if(vidas <= 0)
        {
            textoCanva.text = "Game Over";
            yield break;
        }

        textoCanva.text = "Intentalo de nuevo";
        yield return new WaitForSeconds(1f);

        mCloudRecoBehaviour.enabled = true;
    }


    

    
}
