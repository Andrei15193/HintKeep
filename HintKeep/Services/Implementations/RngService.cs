using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HintKeep.Services.Implementations
{
    public class RngService : IRngService, IDisposable
    {
        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider = new RNGCryptoServiceProvider();

        public string Generate(int length)
        {
            var saltBytes = new byte[(length + length % 2) / 2];
            _rngCryptoServiceProvider.GetNonZeroBytes(saltBytes);

            var hexStringBuilder = new StringBuilder(length);
            using (var saltByte = saltBytes.AsEnumerable<byte>().GetEnumerator())
                while (saltByte.MoveNext() && hexStringBuilder.Length < length)
                    hexStringBuilder.AppendFormat("{0:X2}", saltByte.Current);
            hexStringBuilder.Length = length;
            return hexStringBuilder.ToString();
        }

        public void Dispose()
            => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _rngCryptoServiceProvider.Dispose();
        }
    }
}