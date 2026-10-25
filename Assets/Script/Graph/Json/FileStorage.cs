using System.IO;
using UnityEngine;

namespace Common.Storage
{
    /// <summary>
    /// ファイルの保存・取得
    /// </summary>
    public static class FileStorage
    {
        private const bool _isEncrypt = true;

        /// <summary>
        /// ファイルパスを取得する。
        /// </summary>
        public static string GetPath(string folder, string name)
        {
            return Path.Combine(Application.persistentDataPath, folder, name);
        }

        public static string GetDirectory(params string[] folder)
        {
            string directoryPath = Application.persistentDataPath;

            foreach (string folderName in folder)
            {
                directoryPath = Path.Combine(directoryPath, folderName);
            }
            return directoryPath;
        }

        /// <summary>
        /// ファイルを作成、または既存のファイルを上書き保存する。
        /// </summary>
        public static void SaveOrCreateFile(string folder, string name, string text)
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, folder);

            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, name);

            if (_isEncrypt)
                text = AESCipher.Encrypt(text);

            File.WriteAllText(filePath, text);
        }

        /// <summary>
        /// 画像を保存する
        /// </summary>
        public static void SaveOrCreateBytes(string folder, string fileName, byte[] data)
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, folder);

            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, fileName);

            if (_isEncrypt)
                data = AESCipher.Encrypt(data);

            File.WriteAllBytes(filePath, data);
        }

        /// <summary>
        /// ファイルを読み込む。
        /// </summary>
        /// <returns>ファイルが存在しない場合はnull。</returns>
        public static string LoadFile(string folder, string name)
        {
            string filePath = GetPath(folder, name);

            if (!File.Exists(filePath))
            {
                return null;
            }

            string text = File.ReadAllText(filePath);
            if (_isEncrypt)
                text = AESCipher.Decrypt(text);

            return text;
        }

        /// <summary>
        /// 画像を読み込む。
        /// </summary>
        /// <returns> 画像が存在しない場合はnull。</returns>
        public static byte[] LoadBytes(string folder, string name)
        {
            string filePath = GetPath(folder, name);

            if (!File.Exists(filePath))
            {
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);

            if (_isEncrypt)
                bytes = AESCipher.Decrypt(bytes);

            return bytes;
        }
    }
}