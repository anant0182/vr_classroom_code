using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public TMP_Text statusText;

    void Start()
    {
        loginButton.onClick.AddListener(OnLogin);
        registerButton.onClick.AddListener(OnRegister);

        // Mask password
        passwordInput.contentType = TMP_InputField.ContentType.Password;
    }

    public void OnLogin()
    {
        statusText.text = "Logging in...";
        string username = usernameInput.text;
        string password = passwordInput.text;

        string jsonBody = $"{{\"username\": \"{username}\", \"password\": \"{password}\"}}";

        APIManager.Instance.PostRequest("login/", jsonBody, response =>
        {
            if (response != null)
            {
                JObject jsonResponse = JObject.Parse(response);
                string token = jsonResponse["token"].ToString();
                string role = jsonResponse["role"].ToString();
                AuthData.SetAuthData(token, username, role);

                statusText.text = $"Welcome, {username} ({role})!";
                SceneManager.LoadScene("Connect room");
            }
            else
            {
                statusText.text = "Login failed. Invalid credentials.";
            }
        });
    }

    public void OnRegister()
    {
        statusText.text = "Registering...";
        string username = usernameInput.text;
        string password = passwordInput.text;

        string jsonBody = $"{{\"username\": \"{username}\", \"password\": \"{password}\", \"role\": \"student\"}}";

        APIManager.Instance.PostRequest("register/", jsonBody, response =>
        {
            statusText.text = response != null
                ? "Registration successful! Please log in."
                : "Registration failed. Username may be taken.";
        });
    }
}