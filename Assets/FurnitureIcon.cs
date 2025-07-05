using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;


using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FurnitureIcon : MonoBehaviour
{
    public GameObject debugFurniture;
    public FurnitureData furnitureData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


#if UNITY_EDITOR
    [Button]
    public void GenerateThumbnail()
    {
        var thumbnailTexture = UnityEditor.AssetPreview.GetAssetPreview(debugFurniture);
        //  GetComponent<Image>().sprite = 
        SaveTexture(thumbnailTexture);
    }
#endif

    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/RenderOutput";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.IO.File.WriteAllBytes(dirPath + "/" + debugFurniture.name+ ".png", bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    internal void InitializeFurnitureIcon(FurnitureData furnitureData, GameManager gameManager)
    {
        //GetComponent<Image>().sprite = furnitureData.thumbnail;
        GetComponent<Button>().onClick.AddListener(() => { gameManager.SpawnFurnitureAndSelect(furnitureData.assetLabelString); });
        this.furnitureData = furnitureData;
        StartCoroutine(GetThumbnail(furnitureData.thumbnailUrl));

    }
    IEnumerator GetThumbnail(string thumbnailUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(thumbnailUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture thumbnailTexture = DownloadHandlerTexture.GetContent(www);
            GetComponent<Image>().sprite = Sprite.Create((Texture2D)thumbnailTexture, new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), Vector2.zero);
        }
    }
}
