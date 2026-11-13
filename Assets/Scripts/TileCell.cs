using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCell : MonoBehaviour
{
    [SerializeField] List<TileCell> borderOnTiles = new List<TileCell>();
    [SerializeField] int needStep = 1;

    public void SetStep(int count)
    {
        if (count <= 0)
        {
            return;
        }

        foreach (var tile in borderOnTiles)
        {
            tile.SetStep(count - tile.needStep);
        }
    }
    
    public void SetRelation(TileCell[] cells)
    {
		borderOnTiles.Clear();
		foreach (var cell in cells)
		{
			if (cell == this)
			{
				continue;
			}

			float distance = Vector3.Distance(cell.transform.position, this.transform.position);
			if (distance < 1.1f)
			{
				borderOnTiles.Add(cell);
			}
		}
	}
}
