using System;
using System.Security.Cryptography;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// PDF encryption engine implementing the Standard Security Handler (ISO 32000-1 §7.6).
    /// Supports RC4-128 (V=2, R=3) and AES-128 (V=4, R=4).
    /// </summary>
    internal sealed class PdfEncryptor
    {
        // Table 3.19 — Padding string used in password computation
        private static readonly byte[] Padding =
        {
            0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41,
            0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
            0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80,
            0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A
        };

        private readonly byte[] _encryptionKey;
        private readonly bool _useAes;

        public byte[] OValue { get; }
        public byte[] UValue { get; }
        public int PValue { get; }
        public byte[] FileId { get; }

        /// <summary>Object number of the /Encrypt dictionary — never encrypted.</summary>
        public int EncryptDictObjectNumber { get; set; }

        public PdfEncryptor(string userPassword, string ownerPassword,
                             PdfPermissions permissions, bool useAes)
        {
            _useAes = useAes;
            int keyLength = 128;
            int revision = useAes ? 4 : 3;

            // Generate file ID (16 random bytes)
            FileId = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(FileId);

            PValue = ComputePValue(permissions);

            byte[] userPwd = PadPassword(userPassword ?? "");
            byte[] ownerPwd = PadPassword(ownerPassword ?? userPassword ?? "");

            OValue = ComputeOValue(userPwd, ownerPwd, revision, keyLength);
            _encryptionKey = ComputeEncryptionKey(userPwd, OValue, PValue, FileId, revision, keyLength);
            UValue = ComputeUValue(_encryptionKey, FileId, revision);
        }

        /// <summary>
        /// Encrypt data for a specific indirect object.
        /// Returns data unchanged for object 0 (trailer) and the encrypt dict itself.
        /// </summary>
        public byte[] EncryptData(byte[] data, int objectNumber, int generation)
        {
            if (objectNumber == 0 || objectNumber == EncryptDictObjectNumber)
                return data;
            if (data.Length == 0)
                return data;

            byte[] objectKey = DeriveObjectKey(objectNumber, generation);

            if (_useAes)
                return AesEncrypt(objectKey, data);
            else
                return RC4Transform(objectKey, data);
        }

        private static int ComputePValue(PdfPermissions permissions)
        {
            // Set required reserved bits per ISO 32000-1 Table 22:
            // Bits 7-8 (0-indexed 6-7) must be 1, bits 13-32 (0-indexed 12-31) must be 1
            int p = (int)permissions;
            p |= unchecked((int)0xFFFFF0C0);
            // Bits 1-2 (0-indexed 0-1) must be 0
            p &= unchecked((int)0xFFFFFFFC);
            return p;
        }

        private static byte[] PadPassword(string password)
        {
            byte[] result = new byte[32];
            int len = Math.Min(password.Length, 32);
            for (int i = 0; i < len; i++)
                result[i] = (byte)password[i];
            for (int i = len; i < 32; i++)
                result[i] = Padding[i - len];
            return result;
        }

        // Algorithm 3 (ISO 32000-1 §7.6.3.4): Computing the O value
        private static byte[] ComputeOValue(byte[] userPwd, byte[] ownerPwd, int revision, int keyLength)
        {
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(ownerPwd);
                if (revision >= 3)
                {
                    for (int i = 0; i < 50; i++)
                        hash = md5.ComputeHash(hash);
                }
            }

            int keyBytes = keyLength / 8;
            byte[] key = new byte[keyBytes];
            Array.Copy(hash, key, keyBytes);

            byte[] result = new byte[32];
            Array.Copy(userPwd, result, 32);
            result = RC4Transform(key, result);

            if (revision >= 3)
            {
                for (int n = 1; n <= 19; n++)
                {
                    byte[] modKey = new byte[keyBytes];
                    for (int j = 0; j < keyBytes; j++)
                        modKey[j] = (byte)(key[j] ^ n);
                    result = RC4Transform(modKey, result);
                }
            }

            return result;
        }

        // Algorithm 2 (ISO 32000-1 §7.6.3.3): Computing the encryption key
        private static byte[] ComputeEncryptionKey(byte[] userPwd, byte[] oValue, int pValue,
                                                     byte[] fileId, int revision, int keyLength)
        {
            using var md5 = MD5.Create();

            // Input: padded password + O + P (4 bytes LE) + file ID
            byte[] input = new byte[32 + 32 + 4 + fileId.Length];
            Array.Copy(userPwd, 0, input, 0, 32);
            Array.Copy(oValue, 0, input, 32, 32);
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
                    hash = md5.ComputeHash(hash, 0, keyBytes);
            }

            byte[] key = new byte[keyBytes];
            Array.Copy(hash, key, keyBytes);
            return key;
        }

        // Algorithm 4/5 (ISO 32000-1 §7.6.3.4): Computing the U value
        private static byte[] ComputeUValue(byte[] encryptionKey, byte[] fileId, int revision)
        {
            if (revision == 2)
            {
                // Algorithm 4: RC4-encrypt padding string
                return RC4Transform(encryptionKey, Padding);
            }

            // Algorithm 5: MD5(padding + fileId), then iterated RC4
            using var md5 = MD5.Create();
            byte[] input = new byte[32 + fileId.Length];
            Array.Copy(Padding, 0, input, 0, 32);
            Array.Copy(fileId, 0, input, 32, fileId.Length);
            byte[] hash = md5.ComputeHash(input);

            byte[] result = RC4Transform(encryptionKey, hash);
            for (int n = 1; n <= 19; n++)
            {
                byte[] modKey = new byte[encryptionKey.Length];
                for (int j = 0; j < encryptionKey.Length; j++)
                    modKey[j] = (byte)(encryptionKey[j] ^ n);
                result = RC4Transform(modKey, result);
            }

            // Pad to 32 bytes (remaining bytes are arbitrary)
            byte[] uValue = new byte[32];
            Array.Copy(result, uValue, Math.Min(result.Length, 16));
            return uValue;
        }

        // Algorithm 1 (ISO 32000-1 §7.6.2): Per-object key derivation
        private byte[] DeriveObjectKey(int objectNumber, int generation)
        {
            using var md5 = MD5.Create();

            int extraLen = _useAes ? 9 : 5;
            byte[] input = new byte[_encryptionKey.Length + extraLen];
            Array.Copy(_encryptionKey, input, _encryptionKey.Length);
            int offset = _encryptionKey.Length;

            // Object number (3 bytes LE)
            input[offset++] = (byte)(objectNumber & 0xFF);
            input[offset++] = (byte)((objectNumber >> 8) & 0xFF);
            input[offset++] = (byte)((objectNumber >> 16) & 0xFF);
            // Generation (2 bytes LE)
            input[offset++] = (byte)(generation & 0xFF);
            input[offset++] = (byte)((generation >> 8) & 0xFF);

            if (_useAes)
            {
                // AES salt: "sAlT"
                input[offset++] = 0x73;
                input[offset++] = 0x41;
                input[offset++] = 0x6C;
                input[offset++] = 0x54;
            }

            byte[] hash = md5.ComputeHash(input);
            int keyLen = Math.Min(_encryptionKey.Length + 5, 16);
            byte[] objectKey = new byte[keyLen];
            Array.Copy(hash, objectKey, keyLen);
            return objectKey;
        }

        // RC4 stream cipher (symmetric — same function encrypts and decrypts)
        private static byte[] RC4Transform(byte[] key, byte[] data)
        {
            // KSA
            byte[] s = new byte[256];
            for (int i = 0; i < 256; i++) s[i] = (byte)i;

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % key.Length]) & 255;
                byte tmp = s[i]; s[i] = s[j]; s[j] = tmp;
            }

            // PRGA
            byte[] output = new byte[data.Length];
            int x = 0, y = 0;
            for (int i = 0; i < data.Length; i++)
            {
                x = (x + 1) & 255;
                y = (y + s[x]) & 255;
                byte tmp = s[x]; s[x] = s[y]; s[y] = tmp;
                output[i] = (byte)(data[i] ^ s[(s[x] + s[y]) & 255]);
            }
            return output;
        }

        // AES-128-CBC with PKCS7 padding, IV prepended to output
        private static byte[] AesEncrypt(byte[] key, byte[] data)
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);
            aes.IV = iv;

            byte[] encrypted;
            using (var encryptor = aes.CreateEncryptor())
                encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

            // Prepend IV to encrypted data (per PDF spec for AESV2)
            byte[] result = new byte[16 + encrypted.Length];
            Array.Copy(iv, result, 16);
            Array.Copy(encrypted, 0, result, 16, encrypted.Length);
            return result;
        }
    }
}
