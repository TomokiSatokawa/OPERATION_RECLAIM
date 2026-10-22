using System.Collections.Generic;
using UnityEngine;

namespace Common
{

    [System.Serializable]
    public class FolderList<T> where T : class
    {
        [SerializeField] private List<FolderItem<T>> _datas;

        public IReadOnlyList<FolderData<T>> GetContents(string folder = "")
        {
            var folderList = new List<FolderData<T>>();
            var prefabList = new List<FolderData<T>>();

            foreach (var data in _datas)
            {
                if (!data.Path.StartsWith(folder)) continue;

                string substring = data.Path.Substring(folder.Length);

                if (substring != "" && substring[0] == '/')
                    substring = substring.Substring(1);

                string[] split = substring.Split('/');
                var folderName = split[0];
                Debug.Log(folderName);

                if (string.IsNullOrWhiteSpace(folderName))
                {
                    if (prefabList.FindIndex(x => x.Path == data.Path && x.Data == data.Data) == -1)
                        prefabList.Add(new(data.Path, data.Data));
                }
                else
                {
                    if (folderList.FindIndex(x => x.Folder == folderName) == -1)
                        folderList.Add(new(data.Path, folderName));
                }
            }
            folderList.AddRange(prefabList);
            return folderList;
        }
    }

    [System.Serializable]
    public struct FolderData<T>
    {
        public string Path;
        public string Folder;
        public T Data;

        public FolderData(string path, T data)
        {
            Path = path;
            Data = data;
            Folder = "";
        }
        public FolderData(string path, string folder)
        {
            Path = path;
            Folder = folder;
            Data = default(T);
        }
    }

    [System.Serializable]
    public class FolderItem<T>
    {
        public string Path;
        public T Data;
    }

}
