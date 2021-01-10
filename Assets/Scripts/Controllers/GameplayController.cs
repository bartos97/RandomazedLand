//using System.Text.Json;
//using System.Text.Json.Serialization;
using TerrainGeneration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace Controllers
{
    public class GameplayController : MonoBehaviour
    {
        public GameObject panelSideMenu;
        public GameObject panelAlert;
        public Text panelAlertText;
        public Text panelPosText;

        private bool isMenuOpened = false;
        private MapGenerator mapGeneratorRef;

        private void Start()
        {
            mapGeneratorRef = FindObjectOfType<MapGenerator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isMenuOpened)
                    Resume();
                else
                    Pause();
            }

            var ppos = mapGeneratorRef.playerObject.position;
            int dist = Convert.ToInt32(Vector3.Distance(Vector3.zero, ppos));
            panelPosText.text = $"Player world position: x={Convert.ToInt32(ppos.x)} y={Convert.ToInt32(ppos.y)} z={Convert.ToInt32(ppos.z)}\nDistance from origin is {dist}";
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
            try
            {
                string json = JsonUtility.ToJson(mapGeneratorRef.GetSaveData());
                PlayerPrefs.SetString(Config.PrefKeyGameSave, json);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
                StartCoroutine(ShowAlert("Wystąpił błąd, gra nie została zapisana"));
                return;
            }

            StartCoroutine(ShowAlert("Gra zapisana"));
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

        private IEnumerator ShowAlert(string text)
        {
            panelAlert.SetActive(true);
            panelAlertText.text = text;
            yield return new WaitForSecondsRealtime(3);
            panelAlert.SetActive(false);
            panelAlertText.text = "";
        }
    }
}
