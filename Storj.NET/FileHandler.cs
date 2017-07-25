using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Asn1.Sec;
using System.Configuration;
using System.Web;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Reflection;
using Org.BouncyCastle.Crypto.Encodings;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using StorjDotNet;
using StorjDotNet.Models;

namespace StorjDotNet
{
    public class FileHandler : IDisposable
    {
        private string _path;
        private string _filename;
        private string _temp;
        private CancellationToken _cancellationToken;
        private readonly HttpClient _client;

        public FileHandler(string path, string filename)
        {
            _path = path;
            _filename = filename;
            _cancellationToken = new CancellationToken(false);
            string temp = Path.GetTempPath();
            string fileSha = filename.GetSha256Hash();
            _temp = $"{temp}/{fileSha}";
            Directory.CreateDirectory(_temp);
            _client = new HttpClient();
        }

        public async Task FetchShards(IEnumerable<ShardPointer> pointers)
        {
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = ApplicationConstants.MaxConcurrentStreams,
                CancellationToken = _cancellationToken
            };
            Parallel.ForEach(pointers, options, async p =>
            {
                using (Stream stream = await FetchShard(p))
                {
                    string filePath = $"{_temp}/{p.Index}.shard";
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite,
                        FileShare.Read, p.Size))
                    {
                        stream.CopyTo(fs);
                    }
                }
            });
        }

        public async Task<Stream> FetchShard(ShardPointer pointer)
        {
            Uri requestUri = new Uri($"http://{pointer.Farmer.Address}:{pointer.Farmer.Port}/shards/{pointer.Hash}?token={pointer.Token}");
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = requestUri;
            request.Headers.Add("Content-Type", "application/octet-stream");
            request.Headers.Add("x-token", pointer.Token);
            var response = await _client.SendAsync(request, _cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancellationToken = new CancellationToken(true);
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FileHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
