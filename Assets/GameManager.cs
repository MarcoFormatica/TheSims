using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

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
        
        myCamera = Camera.main;
        Addressables.InitializeAsync().Completed += AddressablesInitializationCompleted;
    }

    private void AddressablesInitializationCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> obj)
    {
        InitializeHorizontalBar();
    }

    public void InitializeHorizontalBar()
    {
        foreach(FurnitureData furnitureData in furnitureDatabase)
        {
            FurnitureIcon spawnedFurnitureIcon = Instantiate(furnitureIconPrefab, horizontalBar);
            spawnedFurnitureIcon.InitializeFurnitureIcon(furnitureData, this);
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
       Addressables.InstantiateAsync(furnitureAssetLabelString).Completed += (asyncOperationResult) => { OnFurnitureSpawned.Invoke(asyncOperationResult.Result.GetComponent<Furniture>());  };
    }



    public void SpawnFurnitureAndSelect(string furnitureAssetLabelString)
    {
        SpawnFurniture(furnitureAssetLabelString, Select);
    }
}
