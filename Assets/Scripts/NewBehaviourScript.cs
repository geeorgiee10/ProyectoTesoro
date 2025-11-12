using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

[System.Serializable]
public class PlayerInfo
{
    public string link;
    public string name;
    public string text;

    public static PlayerInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<PlayerInfo>(jsonString);
    }

}
public class NewBehaviourScript : MonoBehaviour
{
    public PlayerInfo jugador;
    public string json = "";
    // Start is called before the first frame update
    void Start()
    { 
        TextAsset loadedJsonFile = Resources.Load<TextAsset>("caja");
        Debug.Log(loadedJsonFile.text);
        jugador = PlayerInfo.CreateFromJSON(loadedJsonFile.text);
      //  StartCoroutine(FetchGameObjectFromServer(jugador.link,"jose",0, new Hash128() ));
        StartCoroutine(GetAssetBundle());
    }
       IEnumerator GetAssetBundle() {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(jugador.link);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            string[] allAssetNames = bundle.GetAllAssetNames();
            string gameObjectName = Path.GetFileNameWithoutExtension(allAssetNames[0]).ToString();
            GameObject objectFound = bundle.LoadAsset(gameObjectName) as GameObject;
            Instantiate(objectFound,transform.position, transform.rotation);
            
        }
    }
/* IEnumerator FetchGameObjectFromServer(string url,string manifestFileName,uint crcR,Hash128 hashR)
        {
         
            //Get from generated manifest file of assetbundle.
            uint crcNumber = crcR;
            //Get from generated manifest file of assetbundle.
            Hash128 hashCode = hashR;
             UnityWebRequest webrequest =
                UnityWebRequestAssetBundle.GetAssetBundle(url, new CachedAssetBundle(manifestFileName,hashCode), crcNumber);
    
           
            webrequest.SendWebRequest();
    
            while (!webrequest.isDone)
            {
              Debug.Log(webrequest.downloadProgress);  
              
            }
        
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(webrequest);
           yield return assetBundle;
           if (assetBundle == null)
                yield break;
       
      
            //Gets name of all the assets in that assetBundle.
            string[] allAssetNames = assetBundle.GetAllAssetNames();
             Debug.Log(allAssetNames.Length +"objects inside prefab bundle");
            foreach (string gameObjectsName in allAssetNames)
            {
                string gameObjectName = Path.GetFileNameWithoutExtension(gameObjectsName).ToString();
                GameObject objectFound = assetBundle.LoadAsset(gameObjectName) as GameObject;
                Instantiate(objectFound);
            }
            assetBundle.Unload(false);
            yield return null;
        } */
    // Update is called once per frame
    void Update()
    {
        
    }

      void OnGUI() {
      // Display current 'scanning' status
      GUI.Box (new Rect(100,100,200,50), "Not scanning");
      // Display metadata of latest detected cloud-target
      GUI.Box (new Rect(100,200,1000,50), "Nombre : "+jugador.name + "Link : "+jugador.link);
      
  }
}

