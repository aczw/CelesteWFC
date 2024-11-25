using UnityEngine;
using UnityEngine.EventSystems;

public class MouseDetection : MonoBehaviour
{
    public bool IsHoveringOver { get; set; }

    private void Update() {
        IsHoveringOver = EventSystem.current.IsPointerOverGameObject();
    }
}