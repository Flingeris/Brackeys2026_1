using System;
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int startHp;
    [SerializeField] private TMP_Text hpText;
    public int CurrHp { get; private set; }

    private void Awake()
    {
        G.Player = this;
    }

    private void Start()
    {
        CurrHp = startHp;
    }

    public void TakeDamage(int dmgAmount)
    {
        if (dmgAmount <= 0) return;
        CurrHp = Mathf.Max(0, CurrHp - dmgAmount);
        CheckIsDead();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        UpdateHpVisuals();
    }

    private void UpdateHpVisuals()
    {
        hpText.SetText(CurrHp.ToString() + " / " + startHp.ToString());
    }


    private void CheckIsDead()
    {
        if (CurrHp <= 0)
        {
            G.UI.SetLoseActive(true);
        }
    }
}