#nullable enable
using System;
using System.Security.Cryptography;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// PDF encryption engine implementing the Standard Security Handler (ISO 32000-1 §7.6).
    /// Supports RC4-128 (V=2, R=3) and AES-128 (V=4, R=4). Shares its crypto primitives with
    /// the reader-side decryptor via <see cref="StandardSecurityHandler"/>.
    /// </summary>
    internal sealed class PdfEncryptor
    {
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

            FileId = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(FileId);
            }

            PValue = ComputePValue(permissions);

            byte[] userPwd = StandardSecurityHandler.PadPassword(userPassword ?? "");
            byte[] ownerPwd = StandardSecurityHandler.PadPassword(ownerPassword ?? userPassword ?? "");

            OValue = ComputeOValue(userPwd, ownerPwd, revision, keyLength);
            _encryptionKey = StandardSecurityHandler.ComputeEncryptionKey(
                userPwd, OValue, PValue, FileId, revision, keyLength);
            UValue = StandardSecurityHandler.ComputeUValue(_encryptionKey, FileId, revision);
        }

        /// <summary>
        /// Encrypt data for a specific indirect object. Returns data unchanged for object 0
        /// (trailer) and the encrypt dict itself.
        /// </summary>
        public byte[] EncryptData(byte[] data, int objectNumber, int generation)
        {
            if (objectNumber == 0 || objectNumber == EncryptDictObjectNumber)
            {
                return data;
            }
            if (data.Length == 0)
            {
                return data;
            }

            byte[] objectKey = StandardSecurityHandler.DeriveObjectKey(
                _encryptionKey, _useAes, objectNumber, generation);

            return _useAes
                ? StandardSecurityHandler.AesEncryptCbc(objectKey, data)
                : StandardSecurityHandler.RC4Transform(objectKey, data);
        }

        private static int ComputePValue(PdfPermissions permissions)
        {
            // ISO 32000-1 Table 22: bits 7-8 and 13-32 reserved (1), bits 1-2 reserved (0).
            int p = (int)permissions;
            p |= unchecked((int)0xFFFFF0C0);
            p &= unchecked((int)0xFFFFFFFC);
            return p;
        }

        // Algorithm 3 (ISO 32000-1 §7.6.3.4): computing the O value.
        private static byte[] ComputeOValue(byte[] userPwd, byte[] ownerPwd, int revision, int keyLength)
        {
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(ownerPwd);
                if (revision >= 3)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        hash = md5.ComputeHash(hash);
                    }
                }
            }

            int keyBytes = keyLength / 8;
            byte[] key = new byte[keyBytes];
            Array.Copy(hash, key, keyBytes);

            byte[] result = new byte[32];
            Array.Copy(userPwd, result, 32);
            result = StandardSecurityHandler.RC4Transform(key, result);

            if (revision >= 3)
            {
                for (int n = 1; n <= 19; n++)
                {
                    byte[] modKey = new byte[keyBytes];
                    for (int j = 0; j < keyBytes; j++)
                    {
                        modKey[j] = (byte)(key[j] ^ n);
                    }
                    result = StandardSecurityHandler.RC4Transform(modKey, result);
                }
            }

            return result;
        }

    }
}
