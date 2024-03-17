using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UnitUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private Unit unit;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private UnitHealth unitHealth;

    void Start()
    {
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged;
        unitHealth.OnDamaged += UnitHealth_OnDamaged;
        UpdateActionPointsText();
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBarImage.fillAmount = unitHealth.GetHealthNormalized();
    }

    private void UpdateActionPointsText()
    {
        actionPointsText.text = unit.GetActionPoints().ToString();
    }

    private void UnitHealth_OnDamaged(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }

    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        UpdateActionPointsText();
    }
}
