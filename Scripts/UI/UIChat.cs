using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChat : MonoBehaviour
{
    public GameObject enterChatRoot;
    public InputField enterChatField;
    public KeyCode enterChatKey = KeyCode.Return;

    private bool enterChatFieldVisible;

    private void Awake()
    {
        HideEnterChatField();
    }

    private void Update()
    {
        if (Input.GetKeyDown(enterChatKey))
        {
            if (!enterChatFieldVisible)
                ShowEnterChatField();
            else
                SendChatMessage();
        }
    }

    public void ShowEnterChatField()
    {
        if (enterChatRoot != null)
            enterChatRoot.SetActive(true);
        if (enterChatField != null)
        {
            enterChatField.Select();
            enterChatField.ActivateInputField();
        }
        enterChatFieldVisible = true;
    }

    public void HideEnterChatField()
    {
        if (enterChatRoot != null)
            enterChatRoot.SetActive(false);
        enterChatFieldVisible = false;
    }

    public void SendChatMessage()
    {
        if (ChatterEntity.Local == null || enterChatField == null)
            return;

        var trimText = enterChatField.text.Trim();
        if (trimText.Length == 0)
            return;

        enterChatField.text = "";
        ChatterEntity.Local.CmdSendChat(trimText);
        HideEnterChatField();
    }

    public void SendEmoticon(int id)
    {
        if (ChatterEntity.Local == null)
            return;
        ChatterEntity.Local.CmdSendEmoticon(id);
    }
}
