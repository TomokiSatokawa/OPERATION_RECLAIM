using CoreKit.UI.Selector;
using InGame.UI.SquadCommand;
using UnityEngine;

public class AllySelector : MonoBehaviour
{
    [SerializeField] private AllyInfoView[] _allyInfoViews;

    private MultiSelectionController<AllyInfoView> _selectionController;

    private void Start()
    {
        _selectionController = new(_allyInfoViews, 0);
    }
}
