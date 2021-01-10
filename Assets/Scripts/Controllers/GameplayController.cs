using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controllers
{
    public class GameplayController : MonoBehaviour
    {
        public GameObject panelSideMenu;
        private bool isMenuOpened = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isMenuOpened)
                    Resume();
                else
                    Pause();
            }
        }

        public void Pause()
        {
            isMenuOpened = true;
            Time.timeScale = 0f;
            panelSideMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            isMenuOpened = false;
            Time.timeScale = 1f;
            panelSideMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SaveGame()
        {

        }

        public void GoToHomeScreen()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene("HomeScreenScene");
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
