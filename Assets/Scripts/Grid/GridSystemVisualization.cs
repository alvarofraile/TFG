using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static GridSystemVisualization;

public class GridSystemVisualization : MonoBehaviour
{
    public static GridSystemVisualization Instance
    {
        get; private set;
    }

    public enum TileVisualType
    {
        White,
        Blue,
        Red,
        Green,
        LightRed
    }

    [Serializable]
    public struct TileVisualMaterial
    {
        public TileVisualType tileVisualType;
        public Material material;
    }

    [SerializeField] private Transform TileVisualPrefab;
    [SerializeField] private List<TileVisualMaterial> TileVisualMaterials;

    private TileVisual[,] tileVisualArray;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("There's more than one GridSystemVisualization. " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        tileVisualArray = new TileVisual[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetHeight()];

        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                TilePosition tilePosition = new TilePosition(x, z);

                Transform tileVisualTransform = 
                    Instantiate(TileVisualPrefab, LevelGrid.Instance.GetWorldPosition(tilePosition), Quaternion.identity, this.transform);

                tileVisualArray[x, z] = tileVisualTransform.GetComponent<TileVisual>();
            }
        }

        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedTilePosition;

        UpdateGridVisualization();
    }

    private void UpdateGridVisualization()
    {
        HideAllTileVisuals();

        Unit unit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

        TileVisualType tileVisualType;

        switch(selectedAction)
        {
            //TODO: A�adir todas las acciones
            case (MoveAction moveAction):
                tileVisualType = TileVisualType.White;
                break;
            case (ShootAction shootAction):
                tileVisualType = TileVisualType.Red;
                ShowRange(unit.GetTilePosition(), shootAction.GetMaxShootingDistance(), TileVisualType.LightRed);
                break;
            case (HealAction healAction):
                tileVisualType = TileVisualType.Green;
                break;
            case (MeleeAction meleeAction):
                tileVisualType = TileVisualType.Red;
                ShowRange(unit.GetTilePosition(), meleeAction.GetMaxMeleeDistance(), TileVisualType.LightRed);
                break;
            default:
                tileVisualType = TileVisualType.White;
                break;
        }

        ShowList(selectedAction.GetValidTilePositions(), tileVisualType);

    }

    public void HideAllTileVisuals()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                tileVisualArray[x, z].Hide();
            }
        }
    }

    public void ShowRange(TilePosition tilePosition, int range, TileVisualType tileVisualType)
    {
        List<TilePosition> tilePositionList = new List<TilePosition>();

        for(int x = -range; x <= range; x++)
        {
            for(int z = -range; z <= range; z++)
            {
                TilePosition currentTilePosition = tilePosition + new TilePosition(x, z);

                if(!LevelGrid.Instance.IsValidTilePosition(currentTilePosition))
                {
                    continue;
                }

                float distance = new Vector2(Math.Abs(x), Math.Abs(z)).magnitude;
                if (distance > range)
                {
                    continue;
                }

                tilePositionList.Add(currentTilePosition); 
            }
        }

        ShowList(tilePositionList, tileVisualType);
    }

    private void ShowRangeSquare(TilePosition tilePosition, int range, TileVisualType tileVisualType)
    {
        List<TilePosition> tilePositionList = new List<TilePosition>();

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                TilePosition currentTilePosition = tilePosition + new TilePosition(x, z);

                if (!LevelGrid.Instance.IsValidTilePosition(currentTilePosition))
                {
                    continue;
                }

                tilePositionList.Add(currentTilePosition);
            }
        }

        ShowList(tilePositionList, tileVisualType);
    }

    public void ShowList(List<TilePosition> tilePositions, TileVisualType tileVisualType)
    {
        foreach(TilePosition tilePosition in tilePositions)
        {
            tileVisualArray[tilePosition.x, tilePosition.z].Show(GetMaterial(tileVisualType));
        }
    }

    private Material GetMaterial(TileVisualType tileVisualType)
    {
        foreach(TileVisualMaterial tileVisualMaterial in TileVisualMaterials)
        {
            if(tileVisualMaterial.tileVisualType == tileVisualType)
            {
                return tileVisualMaterial.material;
            }
        }

        Debug.LogError("No se ha encontrado el material correspondiente al siguiente TileVisualType: " + tileVisualType);
        return null;
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateGridVisualization();
    }

    private void LevelGrid_OnAnyUnitMovedTilePosition(object sender, EventArgs e)
    {
        UpdateGridVisualization();
    }
}
