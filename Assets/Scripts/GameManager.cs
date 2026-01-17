using Cinemachine;   
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject pauseMenuUI;
    [Header("Score System")]
    public TextMeshProUGUI scoreText; 
    public int currentScore = 0;
    public TextMeshProUGUI highScoreText;
    [Header("Scene Names")]
    public string menuSceneName = "beginscene ";

    [Header("Settings")]
    public CinemachineInputProvider cameraInput;
    bool isGameEnded = false;
    bool isPaused = false; 
    PlayerControls controls; 

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // --- CÀI ĐẶT INPUT ---
        controls = new PlayerControls();
        // Đăng ký sự kiện: Bấm ESC (Pause) -> Gọi hàm TogglePause
        controls.Gameplay.Pause.performed += _ => TogglePause();
    }
    void Start()
    {
        UpdateScoreUI();
        LoadHighScore();
    }
    void OnEnable() { controls.Gameplay.Enable(); }
    void OnDisable() { controls.Gameplay.Disable(); }
    // ====================================================
    // CÁC HÀM XỬ LÝ ĐIỂM SỐ 
    // ====================================================

    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"💰 Cộng {amount} điểm! Tổng: {currentScore}");
        UpdateScoreUI();
    }
    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = "Score: " + currentScore.ToString();
    }
    void LoadHighScore()
    {
        // Lấy điểm từ ổ cứng, nếu chưa có thì mặc định là 0
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (highScoreText)
            highScoreText.text = "Best: " + highScore.ToString();
    }
    void CheckAndSaveHighScore()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        // Nếu điểm hiện tại LỚN HƠN kỷ lục cũ
        if (currentScore > currentHighScore)
        {
            // Lưu kỷ lục mới vào ổ cứng
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
            Debug.Log("🏆 KỶ LỤC MỚI: " + currentScore);

            // Cập nhật lại giao diện ngay lập tức
            LoadHighScore();
        }
    }
    // ====================================================
    //  CÁC HÀM XỬ LÝ PAUSE 
    // ====================================================

    public void TogglePause()
    {
        // Nếu game đã kết thúc (thắng/thua) thì không cho Pause nữa
        if (isGameEnded) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // 🛑 Dừng thời gian
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tắt khả năng xoay camera của chuột
        if (cameraInput != null) cameraInput.enabled = false;
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Bật lại xoay camera
        if (cameraInput != null) cameraInput.enabled = true;
    }

    public void Button_Quit()
    {
        Debug.Log("QUIT GAME!");
        Application.Quit();
    }
    public void CheckEnemyCount()
    {
        // Tìm tất cả các object đang mang Tag "Enemy" trong màn chơi
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Nếu không tìm thấy ai (số lượng = 0) -> Đã diệt sạch
        if (enemies.Length <= 0)
        {
            Victory();
        }
        else
        {
            Debug.Log("Vẫn còn " + enemies.Length + " kẻ địch!");
        }
    }
    public void Victory()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        CheckAndSaveHighScore();
        Debug.Log("CHIẾN THẮNG!");

        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;

        // Tắt camera input khi thắng
        if (cameraInput != null) cameraInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GameOver()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        CheckAndSaveHighScore();
        Debug.Log("THUA CUỘC!");

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        // Tắt camera input khi thua
        if (cameraInput != null) cameraInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Button_Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }    
    public void Button_BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}