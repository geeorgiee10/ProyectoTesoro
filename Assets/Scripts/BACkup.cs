using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using UnityEngine.Networking;
using System.IO;


public class MetaDatos2
{
    public string nombre;
    public string url;
    public string adivinanza;

    public static MetaDatos CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<MetaDatos>(jsonString);
    }

}


public class SimpleCloudRecoEventHandler2 : MonoBehaviour
{

    CloudRecoBehaviour mCloudRecoBehaviour;
    bool mIsScanning = false;

    bool primeraRonda = true;
    string RespuestaPrimeraRonda = "Vengadores";

    string adivinanzaSiguiente = "";  



    string mTargetMetadata = "";
    string mTargetMetadataText = "";
    string mTargetMetadataURL = "";
    string mTargetMetadataError = "";


    int vidas = 3;

    [SerializeField] private TextMeshProUGUI textoCanva;
    [SerializeField] private TextMeshProUGUI vidasTexto;

    public ImageTargetBehaviour ImageTargetTemplate;

    GameObject modeloActual;


    void Start()
    {
        textoCanva.text = "Adivinanza:\n\nUn dios, un genio de metal y un gigante de color. Cuando el mundo esta en peligro, llegan con valor";
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


        mTargetMetadata = datos.nombre;
        mTargetMetadataText = datos.adivinanza;
        mTargetMetadataURL = datos.url;

        adivinanzaSiguiente = datos.adivinanza;

        //mCloudRecoBehaviour.enabled = false;

        if (ImageTargetTemplate != null)
        {
            /* Enable the new result with the same ImageTargetBehaviour: */
            mCloudRecoBehaviour.EnableObservers(cloudRecoSearchResult, ImageTargetTemplate.gameObject);
        }

        if(primeraRonda)
        {
            if(datos.nombre.Trim().ToLower() == RespuestaPrimeraRonda.Trim().ToLower())
            {
                StartCoroutine(Acierto(datos));
            }
            else{
                StartCoroutine(Fallo());
            }
            primeraRonda = false;
        }
        else
        {
            if (cloudRecoSearchResult.TargetName.Trim().ToLower() == datos.nombre.Trim().ToLower())
                StartCoroutine(Acierto(datos));
            else
                StartCoroutine(Fallo());
        }

        //StartCoroutine(GetAssetBundle(datos.url));
        
    }

    IEnumerator Acierto(MetaDatos datos)
    {
        textoCanva.text = "¡Correcto! ¡Has Acertado!";

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(GetAssetBundle(datos.url));

        yield return new WaitForSeconds(4f);

        if (modeloActual != null) 
            Destroy(modeloActual);

        textoCanva.text = "Adivinanza:\n\n" + adivinanzaSiguiente;

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


    void OnGUI()
    {
        // Display current 'scanning' status
        GUI.Box(new Rect(100, 100, 200, 50), mIsScanning ? "Scanning" : "Not scanning");
        // Display metadata of latest detected cloud-target
        GUI.Box(new Rect(100, 200, 200, 50), "Nombre: " + mTargetMetadata);
        GUI.Box(new Rect(100, 300, 700, 100), "Adivinanza: " + mTargetMetadataText);
        GUI.Box(new Rect(100, 400, 700, 50), "URL: " + mTargetMetadataURL);
        GUI.Box(new Rect(100, 500, 700, 50), "Error: " + mTargetMetadataError);
        // If not scanning, show button
        // so that user can restart cloud scanning
        if (!mIsScanning)
        {
            if (GUI.Button(new Rect(100, 600, 200, 50), "Restart Scanning"))
            {
                // Reset Behaviour
                mCloudRecoBehaviour.enabled = true;
                mTargetMetadata = "";
            }
        }
    }

    
}
