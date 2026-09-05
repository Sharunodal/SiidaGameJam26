using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    
    [SerializeField]
    string[] requests;

    [SerializeField]
    string[] completions; //does nothing

    [SerializeField]
    TextMeshProUGUI textBox;


    int dialogue;
    private void Awake()
    {
        dialogue = DialogueSaveManager.Instance.m_DialogueIndex;
        textBox.text = requests[dialogue];
        IterateText();
    }

    private void Update()
    {
        
    }

    void IterateText()
    {
        DialogueSaveManager.Instance.m_DialogueIndex = Mathf.Clamp(DialogueSaveManager.Instance.m_DialogueIndex + 1, 0, requests.Length - 1); ;
    }

    public void DoSomething()
    {
        //does nothing
    }
}
