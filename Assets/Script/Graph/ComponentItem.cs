using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Graph
{

    public class ComponentItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Button _button;

        public void SetData(string name,Action action)
        {
            _text.text = name;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>action?.Invoke());
        }
    }
}