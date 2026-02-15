using TMPro;
using UnityEngine;

public class CardState
{
    public CardModel model;
}

public class CardInstance : DraggableWContainer<CardInstance, ICardContainer>
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
        UpdateClassText();
        UpdateTypeText();
    }

    private void UpdateClassText()
    {
        cardClassText.text = state.model.ClassType.ToString()[0].ToString();
    }

    private void UpdateTypeText()
    {
        cardTypeText.text = state.model.CardType.ToString()[0].ToString();
    }
}