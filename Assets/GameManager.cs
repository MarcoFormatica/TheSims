using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

[Serializable]
public class SaveFile
{
    public List<FurnitureDescriptor> furnitures;
}

[Serializable]
public class FurnitureDescriptor
{
    public string assetLabelString;
    public Vector3 position;
}

[Serializable]
public class FurnitureData
{
    public string assetLabelString;
    public string thumbnailUrl;
}

[Serializable]
public class FurnitureDatabase
{
    public List<FurnitureData> database;
}

public class GameManager : MonoBehaviour
{
    public List<FurnitureData> furnitureDatabase;
    public Camera myCamera;
    public Furniture selectedFurniture;
    public Transform horizontalBar;
    public FurnitureIcon furnitureIconPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //   PlayerPrefs.SetString("Poesia", "Nel mezzo del cammin di nostra vita");
        //  PlayerPrefs.Save();

        // Debug.Log(PlayerPrefs.GetString("Poesia"));

        /*   */
#if UNITY_EDITOR
        FurnitureDatabase fd = new FurnitureDatabase();
        fd.database = furnitureDatabase;
        Debug.Log(JsonUtility.ToJson(fd));
        UnityEditor.EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(fd);
#endif
     
        myCamera = Camera.main;
        Addressables.InitializeAsync().Completed += AddressablesInitializationCompleted;
    }

    private void AddressablesInitializationCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> obj)
    {
        // InitializeHorizontalBar();
        StartCoroutine(GetJsonDatabase());
    }

    public void InitializeHorizontalBar()
    {
        foreach(FurnitureData furnitureData in furnitureDatabase)
        {
            FurnitureIcon spawnedFurnitureIcon = Instantiate(furnitureIconPrefab, horizontalBar);
            spawnedFurnitureIcon.InitializeFurnitureIcon(furnitureData, this);
        }
    }

    IEnumerator GetJsonDatabase()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://ilblogdimarco.altervista.org/furnitureDatabase.html"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log(webRequest.downloadHandler.text);
                    furnitureDatabase = JsonUtility.FromJson<FurnitureDatabase>(webRequest.downloadHandler.text).database;
                    InitializeHorizontalBar();
                    break;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        bool hitSomething = Physics.Raycast(ray, out raycastHit);

        if(hitSomething && selectedFurniture != null)
        {
            selectedFurniture.transform.position = raycastHit.point;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (hitSomething)
            {
                if (selectedFurniture != null) 
                {
                    Deselect();
                }
                else
                {
                    Furniture furniture = raycastHit.collider.gameObject.GetComponent<Furniture>();
                    if (furniture != null)
                    {

                        Select(furniture);

                    }
                    else
                    {
                        Deselect();
                    }
                }

            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            SaveWorld();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            LoadWorld();
        }


    }

    private void LoadWorld()
    {
        if (PlayerPrefs.HasKey("Salvataggio"))
        {
            DestroyAllFurnitures();

            string salvataggioJson = PlayerPrefs.GetString("Salvataggio");
            SaveFile saveFile = JsonUtility.FromJson<SaveFile>(salvataggioJson);
            foreach(FurnitureDescriptor furnitureDescriptor in saveFile.furnitures)
            {
                SpawnFurniture(furnitureDescriptor.assetLabelString, (Furniture spawnedFurniture) => { spawnedFurniture.transform.position = furnitureDescriptor.position; });
            }
        }
    }

    private void DestroyAllFurnitures()
    {
        foreach(Furniture furniture in FindObjectsOfType<Furniture>())
        {
            Destroy(furniture.gameObject);
        }
    }

    private void SaveWorld()
    {
        List<Furniture> furnitureList = FindObjectsOfType<Furniture>().ToList();
        SaveFile saveFile = new SaveFile();
        saveFile.furnitures = furnitureList.Select(f => f.GetFurnitureDescriptor()).ToList();
        PlayerPrefs.SetString("Salvataggio", JsonUtility.ToJson(saveFile));
        Debug.Log(JsonUtility.ToJson(saveFile));
        PlayerPrefs.Save();

    }

    public void Select(Furniture furniture)
    {
        if (selectedFurniture != null) { Deselect(); }

        selectedFurniture = furniture;
        furniture.SetSelection(true);
    }
    public void Deselect()
    {
        if (selectedFurniture != null) { selectedFurniture.SetSelection(false); }
        selectedFurniture = null;
    }

    public void SpawnFurniture(string furnitureAssetLabelString, UnityAction<Furniture> OnFurnitureSpawned)
    {
       Addressables.InstantiateAsync(furnitureAssetLabelString).Completed += (asyncOperationResult) => { asyncOperationResult.Result.GetComponent<Furniture>().assetLabelString = furnitureAssetLabelString; OnFurnitureSpawned.Invoke(asyncOperationResult.Result.GetComponent<Furniture>());  };
    }



    public void SpawnFurnitureAndSelect(string furnitureAssetLabelString)
    {
        SpawnFurniture(furnitureAssetLabelString, Select);
    }
}
