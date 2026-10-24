using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Graph
{

    public class ConditionListGenerator : ListGeneratorBase<ConditionViewBase>
    {
        [SerializeField] private ConditionPrefabDataBase _prefabData;

        private Action<ConditionViewBase> _callback;
        protected override IReadOnlyList<FolderData<ConditionViewBase>> GetContents(string folder)
        {
           return  _prefabData.ConditionPrefab.GetContents(folder);
        }

        protected override void OnCreate(ConditionViewBase data)
        {
            _callback?.Invoke(data);
        }

        public void SetCallback(Action<ConditionViewBase> callback)
        {
            _callback = callback;
        }
    }
}
