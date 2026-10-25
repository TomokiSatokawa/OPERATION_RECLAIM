
//AI
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// AES-256-CBCとHMAC-SHA256による暗号化・復号を行う
/// </summary>
public static class AESCipher
{
    private const int KeySize = 32;
    private const int IvSize = 16;
    private const int HashSize = 32;

    // 既存の鍵文字列を使用
    private const string AES_Key_256 =
        @"5TGB&YHN7UJM(IK<5TGB&YHN7UJM(IK<";

    /// <summary>
    /// 暗号化用の鍵を生成する
    /// </summary>
    private static byte[] GetEncryptionKey()
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(
                Encoding.UTF8.GetBytes("AES:" + AES_Key_256));
        }
    }

    /// <summary>
    /// 改ざん検証用の鍵を生成する
    /// </summary>
    private static byte[] GetAuthenticationKey()
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(
                Encoding.UTF8.GetBytes("HMAC:" + AES_Key_256));
        }
    }

    /// <summary>
    /// 文字列を暗号化してBase64形式で返す
    /// </summary>
    public static byte[] Encrypt(byte[] plaintext)
    {
        byte[] iv = new byte[IvSize];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        byte[] ciphertext;

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = GetEncryptionKey();
            aes.IV = iv;

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                ciphertext = encryptor.TransformFinalBlock(
                    plaintext, 0, plaintext.Length);
            }
        }

        // IV + 暗号文を結合する
        byte[] data = new byte[IvSize + ciphertext.Length];

        Buffer.BlockCopy(iv, 0, data, 0, IvSize);
        Buffer.BlockCopy(
            ciphertext, 0, data, IvSize, ciphertext.Length);

        // HMAC-SHA256で改ざん検証用のタグを生成
        byte[] hash;

        using (HMACSHA256 hmac =
               new HMACSHA256(GetAuthenticationKey()))
        {
            hash = hmac.ComputeHash(data);
        }

        // IV + 暗号文 + HMAC
        byte[] result = new byte[data.Length + HashSize];

        Buffer.BlockCopy(data, 0, result, 0, data.Length);
        Buffer.BlockCopy(hash, 0, result, data.Length, HashSize);

        return result;
    }

    /// <summary>
    /// Base64形式の暗号文を復号する
    /// 改ざんが検出された場合は例外を発生させる
    /// </summary>
    public static byte[] Decrypt(byte[] data)
    {
        // IV + 1ブロック以上の暗号文 + HMAC
        if (data.Length < IvSize + 16 + HashSize)
        {
            throw new CryptographicException("暗号文のサイズが不正です。");
        }

        int authenticatedLength = data.Length - HashSize;

        // HMACを検証する
        byte[] expectedHash;

        using (HMACSHA256 hmac =
               new HMACSHA256(GetAuthenticationKey()))
        {
            expectedHash = hmac.ComputeHash(
                data, 0, authenticatedLength);
        }

        int difference = 0;

        for (int i = 0; i < HashSize; i++)
        {
            difference |=
                expectedHash[i] ^ data[authenticatedLength + i];
        }

        if (difference != 0)
        {
            throw new CryptographicException(
                "データが改ざんされているか、鍵が一致しません。");
        }

        // IVと暗号文を取り出す
        byte[] iv = new byte[IvSize];
        int ciphertextLength = authenticatedLength - IvSize;
        byte[] ciphertext = new byte[ciphertextLength];

        Buffer.BlockCopy(data, 0, iv, 0, IvSize);
        Buffer.BlockCopy(
            data, IvSize, ciphertext, 0, ciphertextLength);

        byte[] plaintext;

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = GetEncryptionKey();
            aes.IV = iv;

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                plaintext = decryptor.TransformFinalBlock(
                    ciphertext, 0, ciphertext.Length);
            }
        }

        return plaintext;
    }
    /// <summary>
    /// 文字列を暗号化する
    /// </summary>
    public static string Encrypt(string text)
    {
        return Convert.ToBase64String(
            Encrypt(Encoding.UTF8.GetBytes(text)));
    }

    /// <summary>
    /// 暗号化された文字列を復号する
    /// </summary>
    public static string Decrypt(string text)
    {
        return Encoding.UTF8.GetString(
            Decrypt(Convert.FromBase64String(text)));
    }
}
