using System.Collections.Generic;
using CoreKit.UI;
using CoreKit.UI.Selector;
using InGame.UI.SquadCommand;
using UnityEngine;

public class AllySelector : ScrollViewBase
{
    [SerializeField] private AllyInfoView _prefab;
    [SerializeField] private Transform[] _allyls;

    private AllyInfoView[] _allyInfoViews;
    private MultiSelectionController<AllyInfoView> _selectionController;

    private void Start()
    {
        DeleteChildren();

        var viewList = new List<AllyInfoView>();

        foreach (var ally in _allyls)
        {
            var view = InstantiateContent(_prefab);
            view.Initialize();
            viewList.Add(view);
        }

        _selectionController = new(viewList.ToArray(), new int[0]);
    }

    public IReadOnlyList<Transform> GetCurrentSelectAlly()
    {
        var selectAlly = new List<Transform>();

        foreach (var index in _selectionController.GetSelectIndex())
        {
            var ally = _allyls[index];
            selectAlly.Add(ally);
        }

        return selectAlly;
    }
}
