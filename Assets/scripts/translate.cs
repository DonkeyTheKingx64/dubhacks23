using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;

public class translate : MonoBehaviour
{
    public Text textToTranslate;
    public Text translatedText;

    private string lastChecked = "";

    private const string apiUrl = "https://api.us-south.language-translator.watson.cloud.ibm.com/instances/23381c5d-acb8-4cd3-a014-15afe2cc3cc0/v3/translate?version=2018-05-01";
    private const string apiKey = "27tYdhgUAlIdHT8AqC6Zn2T8oI1q_Q5LXtqg6dBsoF5j";

    private void Start() {
        if (textToTranslate.text != lastChecked) {
            lastChecked = textToTranslate.text;

            if (!string.IsNullOrEmpty(lastChecked)) {
                WatsonRequest request = new WatsonRequest();
                request.text = new List<string>() { lastChecked };
                request.source = "en";
                request.target = "fr";
                string jsonBody = JsonUtility.ToJson(request);
                StartCoroutine(GetTranslationFromWatson(jsonBody));
            }
        }
    }

    private IEnumerator GetTranslationFromWatson(string jsonBody) {
        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(Encoding.ASCII.GetBytes("apikey:" + apiKey)));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
                translatedText.text = "Error: " + www.error + "Reason: " + www.downloadHandler.text + "Body: " + jsonBody;
            } else {
                string response = www.downloadHandler.text;
                string watsonResponse = JsonUtility.FromJson<WatsonResponse>(response).translations[0].translation;
                translatedText.text = watsonResponse;
            }
        }
    }

    [System.Serializable]
    public class WatsonRequest {
        public List<string> text;
        public string source;
        public string target;
    }

    [System.Serializable]
    public class WatsonResponse {
        public List<Translation> translations;
    }

    [System.Serializable]
    public class Translation {
        public string translation;
    }
}
