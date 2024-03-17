using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{

    public static UnitActionSystem Instance
    {
        get; private set;
    }

    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;

    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;

    private BaseAction selectedAction;
    private bool isBusy;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("There is already an UnitActionSystem: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        SetSelectedUnit(selectedUnit);
    }

    void Update()
    {
        if(isBusy)
        {
            return;
        }

        //TODO: Comprobar si es el turno del jugador o del enemigo

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (TryUnitSelection())
        {
            return;
        }

        HandleSelectedAction();
    }

    private void SetBusy()
    {
        isBusy = true;

        OnBusyChanged?.Invoke(this, isBusy);
    }

    private void ClearBusy()
    {
        isBusy = false;

        OnBusyChanged?.Invoke(this, isBusy);
    }

    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;

        SetSelectedAction(unit.GetBaseActions()[0]);

        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelectedAction(BaseAction action)
    {
        selectedAction = action;

        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }

    private bool TryUnitSelection()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                { 
                    if (unit == selectedUnit)
                    {
                        return false;
                    }

                    if (unit.IsEnemy())
                    {
                        return false;
                    }

                    SetSelectedUnit(unit);
                    return true;
                }
            }
        }

        return false;
    }

    private void HandleSelectedAction()
    {
        if (Input.GetMouseButton(0))
        {

            TilePosition mouseTilePosition = LevelGrid.Instance.GetTilePosition(MouseWorldPosition.GetPosition());

            if (!selectedAction.IsValidTilePosition(mouseTilePosition))
            {
                return;
            }

            if (!selectedUnit.TryUseActionPointsToTakeAction(selectedAction))
            {
                return;
            }

            SetBusy();
            selectedAction.TakeAction(mouseTilePosition, ClearBusy);

            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}
