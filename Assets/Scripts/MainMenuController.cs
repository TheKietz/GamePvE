using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện quản lý chuyển cảnh

public class MainMenuController : MonoBehaviour
{
    // Gán vào nút PLAY
    public void PlayGame()
    {
        // Reset thời gian về 1 (Phòng trường hợp bị dính pause từ màn chơi trước)
        Time.timeScale = 1f;

        // Load màn chơi chính (Nhớ điền đúng tên Scene của bạn)
        SceneManager.LoadScene("Stage 1");
    }

    // Gán vào nút QUIT
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
        Application.Quit();
    }

    // Gán vào nút OPTION (Nếu có tính năng, tạm thời để trống)
    public void OpenOptions()
    {
        Debug.Log("Mở Option...");
    }
}