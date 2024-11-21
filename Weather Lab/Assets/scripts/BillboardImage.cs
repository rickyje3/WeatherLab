using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class BillboardImage : MonoBehaviour
{
    private const string webImage = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Cat_August_2010-4.jpg/2560px-Cat_August_2010-4.jpg";

    private Texture2D cachedImage;

    public Renderer targetRenderer; // Assign the Renderer to display the image

    private BillboardImage billboardImage;

    public Texture defaultTexture;

    private void Start()
    {
        billboardImage = gameObject.AddComponent<BillboardImage>();
        targetRenderer = GetComponent<Renderer>();

        billboardImage.GetWebImage(texture =>
        {
            if (texture != null)
            {
                targetRenderer.material.mainTexture = texture;
            }
            else
            {
                Debug.LogWarning("Using fallback texture.");
                targetRenderer.material.mainTexture = defaultTexture; // Assign a preloaded fallback texture
            }
        });

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(webImage);
        request.timeout = 30; // Set timeout to 30 seconds
    }

    public IEnumerator DownloadImage(Action<Texture2D> callback)
    {

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(webImage);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Failed to download image: {request.error} ({request.result})");
            callback(null);
        }
        else
        {
            Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
            callback(downloadedTexture);
        }

        //callback(DownloadHandlerTexture.GetContent(request));
    }

    public void GetWebImage(Action<Texture2D> callback)
    {
        if (cachedImage != null)
        {
            // Use the cached image
            callback(cachedImage);
        }
        else
        {
            // Download the image and cache it
            StartCoroutine(DownloadImage(texture =>
            {
                cachedImage = texture;
                callback(cachedImage);
            }));
        }
    }

    static BillboardImage()
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
    }
}
