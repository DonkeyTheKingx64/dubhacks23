using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;
using TMPro;

public class camtotext : MonoBehaviour
{

    public string url = "https://vision.googleapis.com/v1/images:annotate?key=";
    public string apiKey = ""; //Put your google cloud vision api key here
    public float captureIntervalSeconds = 1.0f;
    public int requestedWidth = 600;
    public int requestedHeight = 800;   
    public FeatureType featureType = FeatureType.TEXT_DETECTION;
    public int maxResults = 1;
    public GameObject resPanel;
    public TMP_Text responseText, responseArray;
    public Quaternion baseRotation;

    WebCamTexture webcamTexture;
    Texture2D texture2D;
    Dictionary<string, string> headers;

    [System.Serializable]
    public class AnnotateImageRequests
    {
        public List<AnnotateImageRequest> requests;
    }

    [System.Serializable]
    public class AnnotateImageRequest
    {
        public Image image;
        public List<Feature> features;
    }

    [System.Serializable]
    public class Image
    {
        public string content;
    }

    [System.Serializable]
    public class Feature
    {
        public string type;
        public int maxResults;
    }

    public enum FeatureType
    {
        TYPE_UNSPECIFIED,
        FACE_DETECTION,
        LANDMARK_DETECTION,
        LOGO_DETECTION,
        LABEL_DETECTION,
        TEXT_DETECTION,
        SAFE_SEARCH_DETECTION,
        IMAGE_PROPERTIES
    }

    // Use this for initialization
    void Start()
    {
        responseText.text = "The word will appear here!";
        baseRotation = transform.rotation;
        headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json; charset=UTF-8");

        if (apiKey == null || apiKey == "")
            Debug.LogError("No API key. Please set your API key into the \"Web Cam Texture To Cloud Vision(Script)\" component.");

        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
        {
            Debug.Log(devices[i].name);
        }
        if (devices.Length > 0)
        {
            webcamTexture = new WebCamTexture(devices[0].name, requestedWidth, requestedHeight);
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                Material m = r.material;
                if (m != null)
                {
                    m.mainTexture = webcamTexture;
                }
            }
            webcamTexture.Play();
            StartCoroutine("Capture");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = baseRotation * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.down);
    }

    [System.Obsolete]
    private IEnumerator Capture()
    {
        while (true)
        {
            if (this.apiKey == null)
                yield return null;

            yield return new WaitForSeconds(captureIntervalSeconds);

            Color[] pixels = webcamTexture.GetPixels();
            if (pixels.Length == 0)
                yield return null;
            if (texture2D == null || webcamTexture.width != texture2D.width || webcamTexture.height != texture2D.height)
            {
                texture2D = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32, false);
            }

            texture2D.SetPixels(pixels);
            byte[] jpg = texture2D.EncodeToJPG();
            string base64 = System.Convert.ToBase64String(jpg);

            AnnotateImageRequests requests = new AnnotateImageRequests();
            requests.requests = new List<AnnotateImageRequest>();

            AnnotateImageRequest request = new AnnotateImageRequest();
            request.image = new Image();
            request.image.content = base64;
            request.features = new List<Feature>();
            Feature feature = new Feature();
            feature.type = this.featureType.ToString();
            feature.maxResults = this.maxResults;
            request.features.Add(feature);
            requests.requests.Add(request);

            string jsonData = JsonUtility.ToJson(requests, false);
            if (jsonData != string.Empty)
            {
                string url = this.url + this.apiKey;
                byte[] postData = System.Text.Encoding.Default.GetBytes(jsonData);
                using (WWW www = new WWW(url, postData, headers))
                {
                    yield return www;
                    if (string.IsNullOrEmpty(www.error))
                    {
                        string responses = www.text.Replace("\n", "").Replace(" ", "");
                        // Debug.Log(responses);
                        JSONNode res = JSON.Parse(responses);
                        string fullText = res["responses"][0]["labelAnnotations"][0]["description"].ToString().Trim('"');
                        if (fullText != "")
                        {
                            Debug.Log("OCR Response: " + fullText);
                            resPanel.SetActive(true);
                            responseText.text = fullText.Replace("\\n", " ");
                            fullText = fullText.Replace("\\n", ";");
                            string[] texts = fullText.Split(';');
                            responseArray.text = "";
                            for (int i = 0; i < texts.Length; i++)
                            {
                                responseArray.text += texts[i];
                                if (i != texts.Length - 1)
                                    responseArray.text += ", ";
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Error: " + www.error);
                    }
                }
            }
        }
    }

}