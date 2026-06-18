#nullable enable
using System;
using System.Security.Cryptography;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Shared primitives for the PDF Standard Security Handler (ISO 32000-1 §7.6):
    /// password padding, file/object key derivation, RC4, and AES-128-CBC. Used by both the
    /// writer (<see cref="PdfEncryptor"/>) and the reader (<c>PdfDecryptor</c>). RC4 is
    /// symmetric, so <see cref="RC4Transform"/> both encrypts and decrypts.
    /// </summary>
    internal static class StandardSecurityHandler
    {
        // Table 3.19 — padding string used in password computation.
        public static readonly byte[] Padding =
        {
            0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41,
            0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
            0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80,
            0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A
        };

        public static byte[] PadPassword(string password)
        {
            byte[] result = new byte[32];
            int length = Math.Min(password.Length, 32);
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)password[i];
            }
            for (int i = length; i < 32; i++)
            {
                result[i] = Padding[i - length];
            }
            return result;
        }

        // Algorithm 2 (ISO 32000-1 §7.6.3.3): computing the file encryption key.
        public static byte[] ComputeEncryptionKey(byte[] paddedPassword, byte[] oValue, int pValue,
            byte[] fileId, int revision, int keyLength)
        {
            using var md5 = MD5.Create();

            byte[] input = new byte[32 + 32 + 4 + fileId.Length];
            Array.Copy(paddedPassword, 0, input, 0, 32);
            Array.Copy(oValue, 0, input, 32, Math.Min(32, oValue.Length));
            input[64] = (byte)(pValue & 0xFF);
            input[65] = (byte)((pValue >> 8) & 0xFF);
            input[66] = (byte)((pValue >> 16) & 0xFF);
            input[67] = (byte)((pValue >> 24) & 0xFF);
            Array.Copy(fileId, 0, input, 68, fileId.Length);

            byte[] hash = md5.ComputeHash(input);

            int keyBytes = keyLength / 8;
            if (revision >= 3)
            {
                for (int i = 0; i < 50; i++)
                {
                    hash = md5.ComputeHash(hash, 0, keyBytes);
                }
            }

            byte[] key = new byte[keyBytes];
            Array.Copy(hash, key, keyBytes);
            return key;
        }

        // Algorithm 1 (ISO 32000-1 §7.6.2): per-object key derivation.
        public static byte[] DeriveObjectKey(byte[] fileKey, bool useAes, int objectNumber, int generation)
        {
            using var md5 = MD5.Create();

            int extraLength = useAes ? 9 : 5;
            byte[] input = new byte[fileKey.Length + extraLength];
            Array.Copy(fileKey, input, fileKey.Length);
            int offset = fileKey.Length;

            input[offset++] = (byte)(objectNumber & 0xFF);
            input[offset++] = (byte)((objectNumber >> 8) & 0xFF);
            input[offset++] = (byte)((objectNumber >> 16) & 0xFF);
            input[offset++] = (byte)(generation & 0xFF);
            input[offset++] = (byte)((generation >> 8) & 0xFF);

            if (useAes)
            {
                input[offset++] = 0x73; // 's'
                input[offset++] = 0x41; // 'A'
                input[offset++] = 0x6C; // 'l'
                input[offset++] = 0x54; // 'T'
            }

            byte[] hash = md5.ComputeHash(input);
            int objectKeyLength = Math.Min(fileKey.Length + 5, 16);
            byte[] objectKey = new byte[objectKeyLength];
            Array.Copy(hash, objectKey, objectKeyLength);
            return objectKey;
        }

        // RC4 stream cipher (symmetric — encrypts and decrypts).
        public static byte[] RC4Transform(byte[] key, byte[] data)
        {
            byte[] state = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                state[i] = (byte)i;
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + state[i] + key[i % key.Length]) & 255;
                byte tmp = state[i]; state[i] = state[j]; state[j] = tmp;
            }

            byte[] output = new byte[data.Length];
            int x = 0, y = 0;
            for (int i = 0; i < data.Length; i++)
            {
                x = (x + 1) & 255;
                y = (y + state[x]) & 255;
                byte tmp = state[x]; state[x] = state[y]; state[y] = tmp;
                output[i] = (byte)(data[i] ^ state[(state[x] + state[y]) & 255]);
            }
            return output;
        }

        // AES-128-CBC with PKCS7 padding; the 16-byte IV is prepended to the output (AESV2).
        public static byte[] AesEncryptCbc(byte[] key, byte[] data)
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            aes.IV = iv;

            byte[] encrypted;
            using (var encryptor = aes.CreateEncryptor())
            {
                encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
            }

            byte[] result = new byte[16 + encrypted.Length];
            Array.Copy(iv, result, 16);
            Array.Copy(encrypted, 0, result, 16, encrypted.Length);
            return result;
        }

        // AES-CBC decrypt; the 16-byte IV is the leading bytes of the data (AESV2).
        public static byte[] AesDecryptCbc(byte[] key, byte[] data)
        {
            if (data.Length <= 16)
            {
                return Array.Empty<byte>();
            }

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;

            byte[] iv = new byte[16];
            Array.Copy(data, iv, 16);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 16, data.Length - 16);
        }

        // Algorithm 4/5 (ISO 32000-1 §7.6.3.4): computing the U value from the file key. The
        // reader compares this against the stored /U to validate the (empty) user password.
        public static byte[] ComputeUValue(byte[] fileKey, byte[] fileId, int revision)
        {
            if (revision == 2)
            {
                return RC4Transform(fileKey, Padding);
            }

            using var md5 = MD5.Create();
            byte[] input = new byte[32 + fileId.Length];
            Array.Copy(Padding, 0, input, 0, 32);
            Array.Copy(fileId, 0, input, 32, fileId.Length);
            byte[] hash = md5.ComputeHash(input);

            byte[] result = RC4Transform(fileKey, hash);
            for (int n = 1; n <= 19; n++)
            {
                byte[] modKey = new byte[fileKey.Length];
                for (int j = 0; j < fileKey.Length; j++)
                {
                    modKey[j] = (byte)(fileKey[j] ^ n);
                }
                result = RC4Transform(modKey, result);
            }

            byte[] uValue = new byte[32];
            Array.Copy(result, uValue, Math.Min(result.Length, 16));
            return uValue;
        }
    }
}
