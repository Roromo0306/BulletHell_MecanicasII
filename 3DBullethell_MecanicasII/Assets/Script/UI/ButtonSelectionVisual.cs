using UnityEngine;
using UnityEngine.UI;

public class ButtonSelectionVisual : MonoBehaviour
{
    public GameObject selectionBorder;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (selectionBorder != null)
            selectionBorder.SetActive(false);
    }

    private void Update()
    {
        if (button == null || selectionBorder == null)
            return;

        bool isSelected = button.gameObject == UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        selectionBorder.SetActive(isSelected);
    }
}