using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using TMPro;

public class translate : MonoBehaviour
{
    public TMP_Text textToTranslate;
    public TMP_Text translatedText;
    public TMP_Dropdown ddwon;
    string to = "en"; // name change to

    private string lastChecked = "hello";

    private const string apiUrl = "https://api.us-south.language-translator.watson.cloud.ibm.com/instances/23381c5d-acb8-4cd3-a014-15afe2cc3cc0/v3/translate?version=2018-05-01";
    private const string apiKey = "27tYdhgUAlIdHT8AqC6Zn2T8oI1q_Q5LXtqg6dBsoF5j";

    public void onValChanged(int val)
    {
        switch (val) {
            case 0:
                to = "en";
                break;
            case 1:
                to = "fr";
                break;
            case 2:
                to = "cs";
                break;
            case 3:
                to = "es";
                break;
            case 4:
                to = "de";
                break;
            case 5:
                to = "pl";
                break;
            case 6:
                to = "it";
                break;
            default:
                to = "en";
                break;
        }
    }
    private void Update() {
        if (textToTranslate.text != lastChecked) {
            lastChecked = textToTranslate.text;

            if (!string.IsNullOrEmpty(lastChecked)) {
                WatsonRequest request = new WatsonRequest();
                request.text = new List<string>() { lastChecked };
                request.source = "en";
                request.target = to;
                string jsonBody = JsonUtility.ToJson(request);
                StartCoroutine(GetTranslationFromWatson(jsonBody));
            }
        }
    }

    private IEnumerator GetTranslationFromWatson(string jsonBody) {
        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            if (to != "en")
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(Encoding.ASCII.GetBytes("apikey:" + apiKey)));

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    translatedText.text = "Error: " + www.error + "Reason: " + www.downloadHandler.text + "Body: " + jsonBody;
                }
                else
                {
                    string response = www.downloadHandler.text;
                    string watsonResponse = JsonUtility.FromJson<WatsonResponse>(response).translations[0].translation;
                    translatedText.text = watsonResponse;
                }
            }
            else {
                translatedText.text = textToTranslate.text;
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
