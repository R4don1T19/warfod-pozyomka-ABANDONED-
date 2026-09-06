using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
public class SmallDialogueWindowManager : MonoBehaviour
{
    [SerializeField] private DialogueUI dUI;
    [SerializeField] private SmallDialogueWindowPlayerTriggerZone pTZ;
    [SerializeField] private GameObject DIalogueBox;
    [SerializeField] private GameObject QuestUI;
    [SerializeField] private GameObject InventoryUI;
    [SerializeField] private TMP_Text TextLine;
    [SerializeField] private Image SpeakerSprite;
    private SmallDialogueWindowLine currentLine;
    private bool DialogueIsActive = false;
    private bool ReadyToEnd = false;
    private int LineCount = 0;
    private int CurrentNode = 0;
    private string previousSpeakerIcon;
    private Coroutine CurrentCoroutine; // определять переменную-корутину необязательно, но для её контроля лучше это делать. 
    private void Start()
    {
        pTZ = PlayerMovement.Instance.gameObject.GetComponentInChildren<SmallDialogueWindowPlayerTriggerZone>();
    }
    private void Update()
    {
        if (pTZ.StartInspect)
        {
            // проверка, чтобы не прерывать работу корутины.
            if (CurrentCoroutine != null)
                return;
            else if (Input.GetKeyDown(KeyCode.E) && ReadyToEnd)
                SmallTalkEnd();
            else if (Input.GetKeyDown(KeyCode.E) && !DialogueIsActive)
                StartSmallTalk(pTZ.Graph);
        }
        else
            SmallTalkEnd();
    }

    private void StartSmallTalk(SmallDialogueWindowGraph graph)
    { 
        InventoryUI.SetActive(false);
        QuestUI.SetActive(false);

        // Все нужны для UI-элементов значения берутся напрямую именно с самого последнего списка(реплика, спрайт и имя говорящего).
        currentLine = graph.NodesList[CurrentNode].lines[LineCount];

        DIalogueBox.SetActive(true);
        SpeakerSprite.sprite = currentLine.icon;
        
        // Корутина печатает текст
        if (CurrentCoroutine != null)
            StopCoroutine(CurrentCoroutine);
        CurrentCoroutine = StartCoroutine(Typetext(currentLine.line));
        
        LineCount++;
        previousSpeakerIcon = SpeakerSprite.name; // нужно для корутины
        // если локальное количество реплик превысило допустимое
        if (LineCount >= graph.NodesList[CurrentNode].lines.Count)
        {
            LineCount = 0;  // обнуляем локальную переменную
            CurrentNode++;  // переходим на следюущий узел
            if (CurrentNode < graph.NodesList.Count) // если нод существует
                return;   // прожолжает работу
            else
            {
                // если нет, то диалог заканчивается.
                CurrentNode = 0;
                LineCount = 0;
                ReadyToEnd = true;
                return;
            }
        }
    }
    private IEnumerator Typetext(string line)
    {
        int count = line.Length;
        // Условие для продолжения текста или его сброса(если продолжает реплику один герой)
        if (previousSpeakerIcon != SpeakerSprite.name)
            TextLine.text = "";
        else
            TextLine.text += " ";

        while (count > 0)
        {
            foreach (char c in line)
            {
                TextLine.text += c;
                yield return new WaitForSeconds(0.03f);
                count--;
            }
        }
        CurrentCoroutine = null;
    }
    private void SmallTalkEnd()
    {
        // Удаление информации, если внезапно закончился смол-ток
        LineCount = 0;
        CurrentNode = 0;

        // Все остальное
        SpeakerSprite.sprite = null;
        TextLine.text = null;
        DIalogueBox.SetActive(false);

        ReadyToEnd = false;

        QuestUI.SetActive(true);
        InventoryUI.SetActive(true);
    }
}
