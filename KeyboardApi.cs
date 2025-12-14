using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro;
using Photon.Pun;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Text; 

/// <summary>
/// This script implements an interactive AI-assisted query system within the
/// virtual classroom environment. It supports both dictionary-based lookups
/// for single-word queries and contextual AI-based responses for conceptual
/// or multi-word queries using the Gemini API.
/// </summary>
public class KeyboardApi : MonoBehaviour
{
    // --- Gemini API Configuration ---
    private const string API_KEY = ""; 
    private const string GEMINI_MODEL = "gemini-2.5-flash-preview-09-2025";
    private string GEMINI_API_URL => $"https://generativelanguage.googleapis.com/v1beta/models/{GEMINI_MODEL}:generateContent?key={API_KEY}";
    
    // --- UI References ---
    // Input field for user queries and output panel for displaying responses
    [Header("UI References")]
    public TMP_InputField queryInput;
    public RawImage resultPanel; 
    public TextMeshProUGUI resultText; 

    // --- XR Input References (Existing) ---
    // References to XR controllers and head-mounted display for interaction
    public InputDevice _rightController;
    public InputDevice _leftController;
    public InputDevice _HMD;
    
    // --- State Management ---
    // Tracks dictionary definition index and query state
    private int currentDefinitionIndex = 0;
    private string currentSearchTerm = null;
    private bool p_BButton = false; 
    private bool isSearching = false; 

    // --- NEW: Chat History State ---
    private string chatHistory = ""; 

    void Start()
    {
        // Initialize output text reference and display default instruction
        if (resultText == null && resultPanel != null)
        {
            resultText = resultPanel.GetComponentInChildren<TextMeshProUGUI>();
        }
        if (resultText != null)
        {
            chatHistory = "Type a concept or question and press 'Ask AI'!";
            resultText.text = chatHistory;
        }
    }

    void Update()
    {
        // Existing XR Input Initialization and B Button Logic
        if (!_rightController.isValid || !_leftController.isValid || !_HMD.isValid)
        {
            InitializeInputDevices();
        }

        // Cycle through dictionary definitions using controller input
        if (_leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool Bbutton))
        {
            if (Bbutton != p_BButton && Bbutton)
            {
                currentDefinitionIndex++;
                if (!string.IsNullOrWhiteSpace(currentSearchTerm))
                {
                    SearchDictionary(currentSearchTerm); 
                }
            }
            p_BButton = Bbutton;
        }
    }
    
    private void InitializeInputDevices()
    {
        if (!_rightController.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, ref _rightController);
        if (!_leftController.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, ref _leftController);
        if (!_HMD.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.HeadMounted, ref _HMD);
    }
    
    private void InitializeInputDevice(InputDeviceCharacteristics inputCharacteristics, ref InputDevice inputDevice)
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(inputCharacteristics, devices);

        if (devices.Count > 0)
        {
            inputDevice = devices[0];
        }
    }

    // --- Public Entry Point for the "Search AI" Button ---
    public void SubmitAIQuery()
    {
        if (isSearching)
        {
            UpdateStatusText("Please wait for the current request to finish.");
            return;
        }

        string query = queryInput.text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            UpdateStatusText("Please enter a word or question.");
            return;
        }

        bool isSingleWord = !query.Contains(" ") && query.Length > 2;

        currentSearchTerm = query; 
        currentDefinitionIndex = 0; 

        if (isSingleWord)
        {
            Debug.Log($"Input '{query}' appears to be a single word. Searching Dictionary...");
            DisplayQueryAndThink(query, isAI: false);
            SearchDictionary(query);
        }
        else
        {
            Debug.Log($"Input '{query}' contains spaces/is a long query. Asking AI...");
            DisplayQueryAndThink(query, isAI: true);
            SearchAI(query);
        }

        // Clear the input field after submitting
        queryInput.text = ""; 
        queryInput.DeactivateInputField();
    }
    
    // --- NEW: Display Query and Thinking Message ---
    private void DisplayQueryAndThink(string query, bool isAI)
    {
        // Add the new query to the history
        string type = isAI ? "<color=yellow>[AI Query]</color>" : "<color=yellow>[Dictionary]</color>";
        chatHistory += $"\n{type} Teacher: {query}\n<color=#C0C0C0>AI is thinking...</color>\n";
        resultText.text = chatHistory; // Update the UI
    }

    // =========================================================================
    //                            1. AI Chatbot Logic
    // =========================================================================

    private void SearchAI(string query)
    {
        isSearching = true;
        StartCoroutine(GetAIResponse(query));
    }

    IEnumerator GetAIResponse(string query)
    {
        // 1. Construct the system prompt (sets the AI's persona)
        string systemPrompt = "You are an AI assistant in a virtual classroom. Provide a concise, educational, and easy-to-understand explanation for the user's question or concept, suitable for a student being tested. Format the response nicely using bold and lists when appropriate.";

        // 2. Build the JSON payload for the Gemini API
        string jsonPayload = JsonUtility.ToJson(new GeminiRequest
        {
            contents = new List<Content> { new Content { parts = new List<Part> { new Part { text = query } } } },
            systemInstruction = new Part { text = systemPrompt }
        });

        using (UnityWebRequest webRequest = new UnityWebRequest(GEMINI_API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();
            
            isSearching = false;

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                DisplayAIResponse(webRequest.downloadHandler.text, query);
            }
            else
            {
                Debug.LogError($"AI Request Error: {webRequest.responseCode} - {webRequest.error}");
                UpdateStatusText($"[ERROR] AI Request Failed: {webRequest.responseCode}. Check API Key and network.");
            }
        }
    }

    private void DisplayAIResponse(string jsonResponse, string originalQuery)
    {
        // 1. Remove the "thinking" placeholder
        string thinkingPlaceholder = $"\n<color=yellow>[AI Query]</color> Teacher: {originalQuery}\n<color=#C0C0C0>AI is thinking...</color>\n";
        if (chatHistory.Contains(thinkingPlaceholder))
        {
            chatHistory = chatHistory.Replace(thinkingPlaceholder, "");
        }
        
        try
        {
            GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(jsonResponse);
            
            string aiText = "Error: No text found.";
            if (response.candidates != null && response.candidates.Count > 0 && 
                response.candidates[0].content != null && 
                response.candidates[0].content.parts != null && 
                response.candidates[0].content.parts.Count > 0)
            {
                aiText = response.candidates[0].content.parts[0].text;
            }

            // 2. Append the final response to the chat history
            string formattedOutput = $"<color=green>-- AI Response to: {originalQuery} --</color>\n{aiText}\n\n";
            chatHistory += formattedOutput;
            resultText.text = chatHistory;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse AI JSON response: " + e.Message);
            chatHistory += $"\n<color=red>[ERROR]</color> Failed to parse AI response: {e.Message}\n\n";
            resultText.text = chatHistory;
        }
    }

    // =========================================================================
    //                            2. Dictionary Logic (Modified)
    // =========================================================================

    private void SearchDictionary(string word)
    {
        isSearching = true;
        string url = "https://api.dictionaryapi.dev/api/v2/entries/en/" + word;
        StartCoroutine(GetDictionaryRequest(url));
    }

    IEnumerator GetDictionaryRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            isSearching = false;
            
            // 1. Remove the "thinking" placeholder
            string thinkingPlaceholder = $"\n<color=yellow>[Dictionary]</color> Teacher: {currentSearchTerm}\n<color=#C0C0C0>AI is thinking...</color>\n";
            if (chatHistory.Contains(thinkingPlaceholder))
            {
                chatHistory = chatHistory.Replace(thinkingPlaceholder, "");
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                chatHistory += $"\n<color=red>[ERROR]</color> Dictionary request failed.\n\n";
                resultText.text = chatHistory;
                yield break;
            }
            
            string jsonResponse = webRequest.downloadHandler.text;
            
            if (jsonResponse.Contains("No Definitions Found"))
            {
                chatHistory += $"<color=#ADD8E6>Dictionary:</color> No definitions found for '{currentSearchTerm}'.\n\n";
                resultText.text = chatHistory;
            }
            else
            {
                DisplayDictionaryResults(jsonResponse);
            }
        }
    }

    private void DisplayDictionaryResults(string jsonResponse)
    {
        try
        {
            DictionaryEntry[] entries = JsonHelper.FromJson<DictionaryEntry>(jsonResponse);

            if (entries.Length > 0)
            {
                DictionaryEntry entry = entries[0];
                int totalDefinitions = 0;
                foreach (Meaning m in entry.meanings)
                {
                    totalDefinitions += m.definitions.Length;
                }

                if (totalDefinitions == 0)
                {
                    chatHistory += $"<color=#ADD8E6>Dictionary:</color> No detailed definitions available for {entry.word}.\n\n";
                    resultText.text = chatHistory;
                    return;
                }

                // Loop currentDefinitionIndex to wrap around
                if (currentDefinitionIndex >= totalDefinitions)
                {
                    currentDefinitionIndex = 0;
                }
                
                // Find the correct Meaning and Definition index
                int definitionCounter = 0;
                Meaning selectedMeaning = null;
                Definition selectedDefinition = null;

                foreach (Meaning m in entry.meanings)
                {
                    if (currentDefinitionIndex < definitionCounter + m.definitions.Length)
                    {
                        selectedMeaning = m;
                        int definitionIndexInMeaning = currentDefinitionIndex - definitionCounter;
                        selectedDefinition = m.definitions[definitionIndexInMeaning];
                        break;
                    }
                    definitionCounter += m.definitions.Length;
                }

                // Format the output
                string definitionsText = $"<color=yellow>-- Definition ({currentDefinitionIndex + 1}/{totalDefinitions}) --</color>\n";
                definitionsText += $"Word: <color=#00FF00>{entry.word}</color>\n";
                if (selectedMeaning != null && selectedDefinition != null)
                {
                    definitionsText += $"Part of Speech: <color=#ADD8E6>{selectedMeaning.partOfSpeech}</color>\n\n";
                    definitionsText += $"Definition: {selectedDefinition.definition}\n";
                    if (!string.IsNullOrWhiteSpace(selectedDefinition.example))
                    {
                        definitionsText += $"Example: <i>\"{selectedDefinition.example}\"</i>";
                    }
                }
                
                chatHistory += definitionsText + "\n";
                resultText.text = chatHistory;
            }
            else
            {
                chatHistory += $"<color=#ADD8E6>Dictionary:</color> No entries found for '{currentSearchTerm}'.\n\n";
                resultText.text = chatHistory;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse Dictionary JSON response: " + e.Message);
            chatHistory += $"\n<color=red>[ERROR]</color> Error parsing dictionary data.\n\n";
            resultText.text = chatHistory;
        }
    }

    // =========================================================================
    //                            3. Utility Methods & Classes
    // =========================================================================

    // Renamed from SetResultText to UpdateStatusText for transient status messages
    private void UpdateStatusText(string text)
    {
        Debug.Log("Status: " + text);
        // Only update the actual UI text if there's no chat history to show transient status
        if (string.IsNullOrWhiteSpace(chatHistory))
        {
            if (resultText != null) resultText.text = text;
        }
        // If there is history, just update the debug log, as the history takes precedence.
    }

    // [Dictionary Classes remain here]
    [System.Serializable]
    private class DictionaryEntry
    {
        public string word;
        public string phonetic;
        public string origin;
        public Meaning[] meanings;
    }

    [System.Serializable]
    private class Meaning
    {
        public string partOfSpeech;
        public Definition[] definitions;
    }

    [System.Serializable]
    private class Definition
    {
        public string definition;
        public string example;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"Items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.Items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }

    // [Gemini Classes remain here]
    [System.Serializable]
    private class GeminiRequest
    {
        public List<Content> contents;
        public Part systemInstruction; 
    }

    [System.Serializable]
    private class Content
    {
        public string role = "user";
        public List<Part> parts;
    }

    [System.Serializable]
    private class Part
    {
        public string text;
    }

    [System.Serializable]
    private class GeminiResponse
    {
        public List<Candidate> candidates;
    }

    [System.Serializable]
    private class Candidate
    {
        public Content content;
    }
}
