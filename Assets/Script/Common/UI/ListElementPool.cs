
//AI
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// UIリスト要素を再利用するプールを管理する
/// </summary>
public class ListElementPool<T> where T : MonoBehaviour
{
    public IReadOnlyList<T> ActiveElement => _clonedPrefab.Where(x =>x != null && x.gameObject.activeSelf).ToList();

    private readonly RectTransform _content;
    private readonly List<T> _clonedPrefab = new();
    private readonly Func<T, Transform, T> _instance;
    private int _activeIndex;

    public ListElementPool(RectTransform content, Func<T, Transform, T> instance)
    {
        _content = content;
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));

        if (_content == null)
            throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// 要素を取得する。プールに存在しない場合は新規生成する
    /// </summary>
    public T AddElement(T prefab)
    {
        if (prefab == null)
            throw new ArgumentNullException(nameof(prefab));

        T element;

        if (_activeIndex < _clonedPrefab.Count)
        {
            element = _clonedPrefab[_activeIndex];

            if (element == null)
            {
                element = InstanceElement(prefab);
                _clonedPrefab[_activeIndex] = element;
            }
            else
            {
                element.gameObject.SetActive(true);
            }
        }
        else
        {
            element = InstanceElement(prefab);
            _clonedPrefab.Add(element);
        }

        _activeIndex++;
        return element;
    }

    private T InstanceElement(T prefab)
    {
        T element = _instance.Invoke(prefab, _content);

        if (element == null)
            throw new InvalidOperationException("インスタンスの生成に失敗しました。");
        return element;
    }

    /// <summary>
    /// すべての要素を非表示にして再利用可能にする
    /// </summary>
    public void RemoveAll()
    {
        for (int i = 0; i < _activeIndex; i++)
        {
            if (_clonedPrefab[i] != null)
            {
                _clonedPrefab[i].gameObject.SetActive(false);
            }
        }

        _activeIndex = 0;
    }
}
