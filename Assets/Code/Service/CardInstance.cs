using TMPro;
using UnityEngine;

public class CardInstance : Draggable
{
    public CardState state;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescText;
    [SerializeField] private TMP_Text cardClassText;
    [SerializeField] private TMP_Text cardTypeText;


    public void SetState(CardState newState)
    {
        this.state = newState;
        UpdateVisuals();
    }

    public void SetModel(CardModel model)
    {
        if (model == null) return;

        var newState = new CardState()
        {
            model = model
        };

        SetState(newState);
    }

    private void UpdateVisuals()
    {
        cardClassText.text = state.model.ClassType.ToString();
        cardTypeText.text = state.model.CardType.ToString();
    }
}