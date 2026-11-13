using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSetting : MonoBehaviour
{
    [ContextMenu("子のBOXにTileCellを追加")]
    public void Setting()
    {
        var boxlist = GetComponentsInChildren<BoxCollider>();
        foreach (var box in boxlist)
        {
            box.gameObject.AddComponent<TileCell>();
        }
    }

    [ContextMenu("TileCellにSetRelation")]
    public void SetRelationAll()
    {
        var tileCellList = GetComponentsInChildren<TileCell>();
        foreach (var cell in tileCellList)
        {
            cell.SetRelation(tileCellList);
        }
    }
}
