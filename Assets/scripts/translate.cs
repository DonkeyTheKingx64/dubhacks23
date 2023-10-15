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

    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    private const string apiKey = "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";

    private void Start()
    {
        if (textToTranslate.text != lastChecked)
        {
            lastChecked = textToTranslate.text;

            if (!string.IsNullOrEmpty(lastChecked))
            {
                string prompt = $"Translate the following English text to French: '{lastChecked}', only outputting the translated word and nothing else";
                StartCoroutine(GetTranslationFromGPT3(prompt));
            }
        }
    }

    private IEnumerator GetTranslationFromGPT3(string prompt)
    {
        string jsonBody = $"{{\"prompt\":\"{prompt}\",\"max_tokens\":150}}";

        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                translatedText.text = "Error: " + www.error;
            }
            else
            {
                string response = www.downloadHandler.text;
                string chatResponse = JsonUtility.FromJson<ChatResponse>(response).choices[0].text.Trim();
                translatedText.text = chatResponse;
            }
        }
    }

    [System.Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public string text;
    }
}
