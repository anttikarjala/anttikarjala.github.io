using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

// JSON dataset for OpenAI API
public class DataSet
{
    public string prompt;
    public string size;
    public int n;
}

public class EditorMenu : MonoBehaviour
{
    // Attributes to class methods
    public List<GameObject> gameObjects = new();
    public TMP_InputField TMPApiField;
    public TMP_InputField TMPInputField;
    private List<string> goNames = new();
    private List<string> textureNames = new();
    private List<string> materialNames = new();
    private GameObject go;
    private GameObject imageField;
    private GameObject loadingField;
    private GameObject inputErrorField;
    private GameObject applyErrorField;
    private GameObject apiErrorField;
    private GameObject generateImageButton;
    private GameObject applyButton;
    private Button applyButtonInteract;
    private Button generateImageButtonInteract;
    private string playerPrompt;
    private string jsonString;
    private string texturePath;
    private Material material;
    private Sprite sprite;
    private Texture2D texture;
    private Texture2D savedTexture;

    public void DeletePreviousImage()
    {
        imageField = GameObject.Find("ImageField");

        if (imageField.GetComponent<Image>())
        {
            Image exImage = imageField.GetComponent<Image>();
            Destroy(exImage);
        }  
    }

    public void GenerateImage()
    {
        apiErrorField = GameObject.Find("ApiErrorField");
        applyButton = GameObject.Find("ApplyButton");
        applyButtonInteract = applyButton.GetComponent<Button>();
        generateImageButton = GameObject.Find("GenerateImageButton");
        generateImageButtonInteract = generateImageButton.GetComponent<Button>();
        inputErrorField = GameObject.Find("InputErrorField");
        loadingField = GameObject.Find("LoadingField");
        TMPApiField.GetComponent<TMP_InputField>();
        TMPInputField.GetComponent<TMP_InputField>();
        playerPrompt = TMPInputField.text;

        // Clear error fields
        apiErrorField.GetComponent<Text>().text = "";
        inputErrorField.GetComponent<Text>().text = "";

        // Check API key length
        if (TMPApiField.text.Length < 50)
        {
            string apiError = "Minimum API key length 50 characters.";
            apiErrorField.GetComponent<Text>().text = apiError;
            return;
        }
        
        // Check player input length
        if (playerPrompt.Length < 8)
        {
            string inputError = "Prompt minimum length 8 characters.";
            inputErrorField.GetComponent<Text>().text = inputError;
            return;
            
        }
        // Set buttons non-interactable
        applyButtonInteract.interactable = false;
        generateImageButtonInteract.interactable = false;
        
        // Start image generation
        loadingField.GetComponent<Text>().text = "Generating image, please wait as it might take a while..";
        StartCoroutine(SendImageRequest());
    }

    public IEnumerator SendImageRequest()
    {
        // Create instance of class DataSet with values
        DataSet data = new()
        {
            prompt = playerPrompt,
            n = 1,
            size = "512x512"
        };

        // Convert dataset to json
        string JsonData = JsonUtility.ToJson(data);

        // OpenAI url
        string uri = "https://api.openai.com/v1/images/generations";

        // Read the player API key
        string apikey = TMPApiField.text;

        // Create instance of Unity Web Request
        byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonData);
        UnityWebRequest uwrRequest = UnityWebRequest.Post(uri, JsonData);

        // Set the upload/download headers
        uwrRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        uwrRequest.downloadHandler = new DownloadHandlerBuffer();

        // Set the json message headers
        uwrRequest.SetRequestHeader("Authorization", $"Bearer {apikey}");
        uwrRequest.SetRequestHeader("Content-Type", "application/json");

        // yield return uwr.SendWebRequest();
        yield return (uwrRequest.SendWebRequest());

        // UWR response error
        if (uwrRequest.result != UnityWebRequest.Result.Success)
        {
            string inputError = "The prompt used contained an error.";
            
            // Clear error fields and buttons
            inputErrorField.GetComponent<Text>().text = inputError;
            loadingField.GetComponent<Text>().text = "";
            applyButtonInteract.interactable = true;
            generateImageButtonInteract.interactable = true;

            yield break;
        }

        else
        {
            // Save the response to a string
            jsonString = uwrRequest.downloadHandler.text;
            StartCoroutine(DownloadImage());
        }
    }
    public IEnumerator DownloadImage()
    {
        // Parse through the response for image url
        string pattern = @"(https?://[^\s]+)";
        Regex regex = new(pattern);
        Match match = regex.Match(jsonString);

        if (match.Success)
        {
            string url = match.Groups[1].Value;
            url = url.TrimEnd('\"');

            // Create a UWR to download the image as texture
            UnityWebRequest uwrTexture = UnityWebRequestTexture.GetTexture(url);

            yield return (uwrTexture.SendWebRequest());

            if (uwrTexture.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }

            else
            {
                // Get texture and create a sprite from the DownloadHandler response
                texture = DownloadHandlerTexture.GetContent(uwrTexture);
                sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);

                // Read texture to byte array and save to file
                byte[] bytes = texture.EncodeToPNG();
                texturePath = $"{Application.persistentDataPath}/PlayerTexture{Random.Range(1, 100)}.png";
                while(File.Exists(texturePath))
                {
                    texturePath = $"{Application.persistentDataPath}/PlayerTexture{Random.Range(1, 100)}.png";
                }
                File.WriteAllBytes(texturePath, bytes);

                // Save texture filename path to list
                textureNames.Add(texturePath);

                // Portray downloaded image to the player
                Image image = imageField.AddComponent<Image>();
                image.sprite = sprite;

                // Clear loading prompt and return button functionality
                loadingField.GetComponent<Text>().text = "";
                applyButtonInteract.interactable = true;
                generateImageButtonInteract.interactable = true;
            }
        }
    }

    public void ApplyImage()
    {
        applyErrorField = GameObject.Find("ApplyErrorField");
        
        if (!File.Exists(texturePath))
        {
            string error = "Please generate the image first!";
            applyErrorField.GetComponent<Text>().text = error;
            return;
        }
        
        // Select a pre-created material and add name to list
        material = Resources.Load<Material>($"PlayerMaterial{Random.Range(1, 100)}");
        while (material.mainTexture)
        {
            material = Resources.Load<Material>($"PlayerMaterial{Random.Range(1, 100)}");
        }
        materialNames.Add(material.name);

        // Load the saved texture and set it on the material
        byte[] textureData = File.ReadAllBytes(texturePath);
        savedTexture = new(512, 512);
        savedTexture.LoadImage(textureData);
        material.SetTexture("_MainTex", savedTexture);

        // Create a list of chosen gameobjects
        foreach (GameObject go in gameObjects)
        {
            if (go.GetComponent<Toggle>().isOn)
            {
                goNames.Add(go.tag);
            }
        }

        // Apply the material to all child components
        foreach (string tag in goNames)
        {
            go = GameObject.Find(tag);
            foreach (Transform child in go.transform)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                renderer.material = material;
            }
        }
        goNames.Clear();
    }
    
    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
        ClearUsedMaterials();
        ClearTextureFiles();
    }

    public void ClearUsedMaterials()
    {
        foreach (string name in materialNames)
        {
            material = Resources.Load<Material>(name);
            material.SetTexture("_MainTex", null);
        }
        materialNames.Clear();
    }

    public void ClearTextureFiles()
    {
        foreach (string fileName in textureNames)
        {
            File.Delete(fileName);
        }
        textureNames.Clear();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}