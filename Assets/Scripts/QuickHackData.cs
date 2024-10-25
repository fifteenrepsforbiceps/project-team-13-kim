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

/*
이 스크립터블 오브젝트는 퀵핵의 기본적인 정보를 담고 있습니다.
Unity Editor에서 이 ScriptableObject를 사용해 다양한 퀵핵 데이터를 생성할 수 있습니다.
각 데이터에는 퀵핵의 이름, 램 코스트, 피해량, 지속 시간, 업로드 시간 및 설명이 포함됩니다.
*/