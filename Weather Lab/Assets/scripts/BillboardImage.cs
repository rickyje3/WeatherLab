using System;
using System.Net;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BillboardImage : MonoBehaviour
{
    private const string webImage = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Cat_August_2010-4.jpg/2560px-Cat_August_2010-4.jpg";

    private Texture2D cachedImage; //stores the downloaded image
    public Renderer targetRenderer; //Assign the Renderer to display the image
    public Texture defaultTexture; //Fallback texture if download fails

    private bool isDownloading = false; //prevents multiple downloads

    private void Start()
    {
        // Ensure targetRenderer is assigned
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError("No Renderer component found on this GameObject.");
                return;
            }
        }

        // Get or download the web image
        GetWebImage(texture =>
        {
            if (texture != null)
            {
                targetRenderer.material.mainTexture = texture;
            }
            else
            {
                Debug.LogWarning("Using fallback texture.");
                targetRenderer.material.mainTexture = defaultTexture;
            }
        });
    }

    public IEnumerator DownloadImage(Action<Texture2D> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(webImage);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Failed to download image: {request.error}");
            callback(null);
        }
        else
        {
            Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
            callback(downloadedTexture);
        }
    }

    public void GetWebImage(Action<Texture2D> callback)
    {
        if (cachedImage != null)
        {
            //Use cached image if available
            callback(cachedImage);
        }
        else if (!isDownloading)
        {
            //Only start downloading if not already 
            isDownloading = true;
            StartCoroutine(DownloadImage(texture =>
            {
                cachedImage = texture; //Cache the downloaded image
                isDownloading = false; 
                callback(cachedImage);
            }));
        }
    }

    static BillboardImage()
    {
        //Bypass SSL validation for testing purposes
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
    }
}

