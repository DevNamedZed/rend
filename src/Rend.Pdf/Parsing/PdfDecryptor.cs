#nullable enable
using Rend.Pdf.Internal;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Decrypts the strings and streams of a PDF protected by the Standard Security Handler
    /// (ISO 32000-1 §7.6), assuming the empty user password (the common "opens without a
    /// password" case). Supports V1/V2 (RC4-40/128) and V4 (RC4 or AESV2). V5/R6 (AES-256)
    /// is not handled by this type.
    /// </summary>
    internal sealed class PdfDecryptor
    {
        private readonly byte[] _fileKey;
        private readonly bool _useAes;
        private readonly byte[] _fileId;
        private readonly int _revision;

        public PdfDecryptor(int revision, int keyLengthBits, byte[] oValue, int permissions,
            byte[] fileId, bool useAes)
        {
            _useAes = useAes;
            _revision = revision;
            _fileId = fileId;
            int keyLength = keyLengthBits > 0 ? keyLengthBits : 40;
            byte[] paddedEmptyPassword = StandardSecurityHandler.PadPassword(string.Empty);
            _fileKey = StandardSecurityHandler.ComputeEncryptionKey(
                paddedEmptyPassword, oValue, permissions, fileId, revision, keyLength);
        }

        /// <summary>
        /// Validates the empty user password by recomputing the U value (Algorithm 4/5) from
        /// the derived file key and comparing it to the document's stored /U value. If this
        /// returns false the document requires a non-empty user password.
        /// </summary>
        public bool IsUserPasswordValid(byte[] uValue)
        {
            byte[] computed = StandardSecurityHandler.ComputeUValue(_fileKey, _fileId, _revision);
            int compareLength = _revision == 2 ? 32 : 16;
            if (uValue.Length < compareLength)
            {
                return false;
            }
            for (int i = 0; i < compareLength; i++)
            {
                if (computed[i] != uValue[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Decrypts a string or stream that belongs to the indirect object
        /// (<paramref name="objectNumber"/>, <paramref name="generation"/>).
        /// </summary>
        public byte[] Decrypt(byte[] data, int objectNumber, int generation)
        {
            if (data.Length == 0)
            {
                return data;
            }
            byte[] objectKey = StandardSecurityHandler.DeriveObjectKey(
                _fileKey, _useAes, objectNumber, generation);
            return _useAes
                ? StandardSecurityHandler.AesDecryptCbc(objectKey, data)
                : StandardSecurityHandler.RC4Transform(objectKey, data);
        }
    }
}
