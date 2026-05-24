using UnityEngine;

/// <summary>
/// 队伍管理器 - 管理红蓝队伍分配
/// 设置标签、颜色和层级
/// </summary>
public class TeamManager : MonoBehaviour
{
    [Header("玩家队伍 (从TeamSelect获取)")]
    public string playerTeam;
    public string enemyTeam;

    [Header("队伍成员")]
    public GameObject[] redTeam = new GameObject[4];
    public GameObject[] blueTeam = new GameObject[4];

    [Header("队伍颜色")]
    public Color redColor = new Color(0.9f, 0.3f, 0.3f, 1f);
    public Color blueColor = new Color(0.3f, 0.3f, 0.9f, 1f);

    void Start()
    {
        // 从TeamSelect读取玩家选择的队伍
        playerTeam = TeamSelect.selectedTeam;
        enemyTeam = (playerTeam == "red") ? "blue" : "red";

        SetupTeams();
    }

    /// <summary>
    /// 初始化所有队伍成员: 标签、颜色、层级
    /// </summary>
    public void SetupTeams()
    {
        // 设置红队
        foreach (GameObject obj in redTeam)
        {
            if (obj != null)
            {
                AssignTeam(obj, "red");
            }
        }

        // 设置蓝队
        foreach (GameObject obj in blueTeam)
        {
            if (obj != null)
            {
                AssignTeam(obj, "blue");
            }
        }
    }

    /// <summary>
    /// 判断两个对象是否属于不同队伍 (敌方关系)
    /// </summary>
    public bool IsEnemy(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;
        string teamA = a.tag;
        string teamB = b.tag;
        return !string.IsNullOrEmpty(teamA) && !string.IsNullOrEmpty(teamB) && teamA != teamB;
    }

    /// <summary>
    /// 判断两个对象是否属于同一队伍 (友方关系)
    /// </summary>
    public bool IsFriendly(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;
        string teamA = a.tag;
        string teamB = b.tag;
        return !string.IsNullOrEmpty(teamA) && !string.IsNullOrEmpty(teamB) && teamA == teamB;
    }

    /// <summary>
    /// 分配队伍: 设置标签、PlayerVisual队伍颜色
    /// </summary>
    public void AssignTeam(GameObject obj, string team)
    {
        if (obj == null) return;

        // 设置Unity标签用于队伍识别
        if (team == "red")
        {
            obj.tag = "RedTeam";
        }
        else
        {
            obj.tag = "BlueTeam";
        }

        // 设置PlayerVisual的队伍颜色
        PlayerVisual visual = obj.GetComponent<PlayerVisual>();
        if (visual != null)
        {
            Color teamColor = (team == "red") ? redColor : blueColor;
            visual.SetTeamColor(teamColor);
        }

        // 设置层级用于物理碰撞过滤
        int layer = (team == "red") ? LayerMask.NameToLayer("RedTeam") : LayerMask.NameToLayer("BlueTeam");
        if (layer >= 0)
        {
            SetLayerRecursively(obj, layer);
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
