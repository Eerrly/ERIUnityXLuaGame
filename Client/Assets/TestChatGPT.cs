using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable] public class ChatGPTPostData
{
    public string model;
    public string prompt;
    public float temperature;
}

[System.Serializable] public class ChatGPTCallBackData
{
    public string id;
    public string created;
    public string model;
    public List<TextSample> choices;

    [System.Serializable]
    public class TextSample
    {
        public string text;
        public string index;
        public string finish_reason;
    }
}


public class TestChatGPT : MonoBehaviour
{
    private const string chatGPTUri = "https://api.openai.com/v1/completions";
    private const string chatGPTKey = "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";
    private ChatGPTPostData chatGPTPostData;

    [Header("ChatGPT Model")]public string model;
    [Header("ChatGPT Temperature")]public float temperature;

    public Button button;
    public InputField inputField;
    public Text text;

    public void Start()
    {
        button.onClick.AddListener(OnSendBtnClicked);
    }

    public void OnSendBtnClicked()
    {
        var msg = inputField.text;
        SendToChatGPT(msg);
    }

    public void SendToChatGPT(string msg)
    {
        if (string.IsNullOrEmpty(msg))
        {
            text.color = Color.red;
            text.text = "ChatGPT msg cannot be empty!!";
            return;
        }
        if(string.IsNullOrEmpty(model) || temperature == default(float))
        {
            text.color = Color.red;
            text.text = "ChatGPT model and temperature cannot be empty!!";
            return;
        }
        chatGPTPostData = new ChatGPTPostData()
        {
            model = model,
            temperature = temperature,
            prompt = msg,
        };
        StartCoroutine(PostData(ChatGPTCallBack));
    }

    public void ChatGPTCallBack(string result)
    {
        text.color = Color.white;
        text.text = result;
    }

    private IEnumerator PostData(System.Action<string> callback)
    {
        var request = new UnityWebRequest(chatGPTUri, "POST");
        var json = JsonUtility.ToJson(chatGPTPostData);
        var bytes = System.Text.ASCIIEncoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", string.Format("Bearer {0}", chatGPTKey));

        text.color = Color.white;
        text.text = "waiting...";
        yield return request.SendWebRequest();
        if(request.responseCode == 200)
        {
            var result = request.downloadHandler.text;
            var data = JsonUtility.FromJson<ChatGPTCallBackData>(result);
            if(data != null && data.choices.Count > 0)
            {
                callback(data.choices[0].text.Replace("\n", ""));
            }
        }
        else
        {
            text.color = Color.red;
            text.text = request.error;
        }
    }
}
