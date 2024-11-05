using UnityEngine;

[CreateAssetMenu(fileName = "New QuickHack", menuName = "QuickHack/New QuickHack Data")]
public class QuickHackData : ScriptableObject
{
    [Header("QuickHack Information")]
    public string hackName;           // 퀵핵의 이름
    public int ramCost;               // 퀵핵 사용에 필요한 램 비용
    public int damage;                // 퀵핵의 피해량
    public float duration;            // 퀵핵의 지속 시간
    public float uploadTime;          // 퀵핵 업로드 시간
    [TextArea]
    public string description;        // 퀵핵의 설명

    [Header("QuickHack UI Prefab")]
    public GameObject hackUIPrefab;   // 퀵핵의 UI 프리팹

    private void OnValidate()
    {
        UpdateDescription();
    }

    public void UpdateDescription()
    {
        description = $"Hack Name: {hackName}\n" +
                    $"RAM Cost: {ramCost}\n" +
                    $"Damage: {damage}\n" +
                    $"Duration: {duration} seconds\n" +
                    $"Upload Time: {uploadTime} seconds";
    }
}
