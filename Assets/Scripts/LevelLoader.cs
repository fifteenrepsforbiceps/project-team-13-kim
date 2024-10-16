using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // 버튼의 이름을 지정합니다. 유니티 에디터에서 버튼의 이름을 "StartButton"으로 설정하세요.
    private string startButtonName = "StartButton";

    void Start()
    {
        // 씬에서 버튼을 이름으로 찾습니다.
        GameObject buttonObject = GameObject.Find(startButtonName);
        if (buttonObject != null)
        {
            Button startButton = buttonObject.GetComponent<Button>();
            if (startButton != null)
            {
                // LoadGameScene 메서드를 버튼의 onClick 이벤트에 추가합니다.
                startButton.onClick.AddListener(LoadGameScene);
            }
            else
            {
                Debug.LogError($"'{startButtonName}'에 Button 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogError($"씬에 '{startButtonName}'이라는 이름의 게임 오브젝트가 없습니다.");
        }
    }

    // 게임 씬을 로드하는 메서드
    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
