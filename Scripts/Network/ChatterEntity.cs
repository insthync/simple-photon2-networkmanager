using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ChatterEntity : MonoBehaviourPunCallbacks
{
    public static ChatterEntity Local { get; private set; }
    [Header("Chat Bubble")]
    public float chatBubbleVisibleDuration = 2f;
    public GameObject chatBubbleRoot;
    public Text chatBubbleText;
    [Header("Emoticons")]
    public float emoticonVisibleDuration = 2f;
    public GameObject[] emoticons;

    private float lastShowChatBubbleTime;
    private float lastShowEmoticonTime;
    private GameObject lastShowEmoticon;

    private void Start()
    {
        if (photonView.IsMine)
            Local = this;
    }

    private void Awake()
    {
        if (chatBubbleRoot != null)
            chatBubbleRoot.SetActive(false);

        if (lastShowEmoticon != null)
            lastShowEmoticon.SetActive(false);
    }

    private void Update()
    {
        // Hide chat bubble
        if (Time.realtimeSinceStartup - lastShowChatBubbleTime >= chatBubbleVisibleDuration)
        {
            if (chatBubbleRoot != null)
                chatBubbleRoot.SetActive(false);
        }
        // Hide emoticon
        if (Time.realtimeSinceStartup - lastShowEmoticonTime >= emoticonVisibleDuration)
        {
            if (lastShowEmoticon != null)
                lastShowEmoticon.SetActive(false);
        }
    }
    
    public void CmdSendChat(string message)
    {
        photonView.AllRPC(RpcShowChat, message);
    }

    [PunRPC]
    public void RpcShowChat(string message)
    {
        // Set chat text and show chat bubble
        if (chatBubbleText != null)
            chatBubbleText.text = message;

        if (chatBubbleRoot != null)
            chatBubbleRoot.SetActive(true);

        lastShowChatBubbleTime = Time.realtimeSinceStartup;

        // TODO: Add chat message to chat history (maybe in any network manager)
    }
    
    public void CmdSendEmoticon(int id)
    {
        photonView.AllRPC(RpcShowEmoticon, id);
    }

    [PunRPC]
    public void RpcShowEmoticon(int id)
    {
        if (id < 0 || id >= emoticons.Length)
            return;

        // Show emoticon by index
        foreach (var emoticon in emoticons)
        {
            if (emoticon != null)
                emoticon.SetActive(false);
        }

        lastShowEmoticon = emoticons[id];
        if (lastShowEmoticon != null)
            lastShowEmoticon.SetActive(true);

        lastShowEmoticonTime = Time.realtimeSinceStartup;
    }
}
