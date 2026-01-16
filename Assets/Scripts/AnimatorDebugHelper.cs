using UnityEngine;

/// <summary>
/// Script debug helper - Gắn vào Player object
/// Hiển thị realtime các giá trị animator và input
/// </summary>
public class AnimatorDebugHelper : MonoBehaviour
{
    public Animator animator;
    public PlayerController controller;

    [Header("Display Settings")]
    public bool showOnScreen = true;
    public bool showInConsole = false;

    void OnGUI()
    {
        if (!showOnScreen || animator == null) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        int y = 10;
        int lineHeight = 25;

        // Background
        GUI.Box(new Rect(5, 5, 500, 450), "");

        // Title
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 20;
        GUI.Label(new Rect(10, y, 500, 30), "🎮 ANIMATOR DEBUG", style);
        y += lineHeight + 10;

        style.fontStyle = FontStyle.Normal;
        style.fontSize = 16;

        // Current State
        string currentState = GetCurrentStateName();
        GUI.Label(new Rect(10, y, 500, 30), $"Current State: <color=cyan>{currentState}</color>", style);
        y += lineHeight;

        // Parameters
        GUI.Label(new Rect(10, y, 500, 30), "=== PARAMETERS ===", style);
        y += lineHeight;

        DrawParameter("Locked", animator.GetBool("Locked"), ref y, lineHeight, style);
        DrawParameter("isWalking", animator.GetBool("isWalking"), ref y, lineHeight, style);
        DrawParameter("Grounded", animator.GetBool("Grounded"), ref y, lineHeight, style);
        DrawParameter("Horizontal", animator.GetFloat("Horizontal"), ref y, lineHeight, style);
        DrawParameter("Vertical", animator.GetFloat("Vertical"), ref y, lineHeight, style);
        DrawParameter("Turn", animator.GetFloat("Turn"), ref y, lineHeight, style);
        DrawParameter("AttackIndex", animator.GetInteger("AttackIndex"), ref y, lineHeight, style);

        y += 10;
        GUI.Label(new Rect(10, y, 500, 30), "=== CONTROLLER STATE ===", style);
        y += lineHeight;

        if (controller != null)
        {
            DrawParameter("isAttacking", controller.isAttacking, ref y, lineHeight, style);
            DrawParameter("isHit", controller.isHit, ref y, lineHeight, style);
            DrawParameter("isDead", controller.isDead, ref y, lineHeight, style);
            DrawParameter("LockTarget", controller.lockOnTarget != null ? controller.lockOnTarget.name : "None", ref y, lineHeight, style);
        }
    }

    void DrawParameter(string name, object value, ref int y, int lineHeight, GUIStyle style)
    {
        string coloredValue = "";

        if (value is bool boolValue)
        {
            coloredValue = boolValue ? "<color=green>TRUE</color>" : "<color=red>FALSE</color>";
        }
        else if (value is float floatValue)
        {
            string color = Mathf.Abs(floatValue) < 0.01f ? "gray" : "yellow";
            coloredValue = $"<color={color}>{floatValue:F2}</color>";
        }
        else if (value is int intValue)
        {
            coloredValue = $"<color=yellow>{intValue}</color>";
        }
        else
        {
            coloredValue = $"<color=white>{value}</color>";
        }

        GUI.Label(new Rect(20, y, 500, 30), $"{name}: {coloredValue}", style);
        y += lineHeight;
    }

    string GetCurrentStateName()
    {
        if (animator == null) return "No Animator";

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        if (clipInfo.Length > 0)
        {
            return clipInfo[0].clip.name;
        }

        return "Unknown";
    }

    void Update()
    {
        if (showInConsole && Time.frameCount % 60 == 0) // Mỗi giây
        {
            LogDebugInfo();
        }
    }

    void LogDebugInfo()
    {
        Debug.Log($"[ANIMATOR DEBUG]\n" +
                  $"State: {GetCurrentStateName()}\n" +
                  $"Locked: {animator.GetBool("Locked")}\n" +
                  $"Horizontal: {animator.GetFloat("Horizontal"):F2}\n" +
                  $"Vertical: {animator.GetFloat("Vertical"):F2}\n" +
                  $"Turn: {animator.GetFloat("Turn"):F2}\n" +
                  $"isWalking: {animator.GetBool("isWalking")}\n" +
                  $"isAttacking: {(controller != null ? controller.isAttacking : false)}\n" +
                  $"LockTarget: {(controller != null && controller.lockOnTarget != null ? controller.lockOnTarget.name : "None")}");
    }
}