using Microsoft.SharePoint.Client;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RateLimitDemo
{
    /// <summary>
    /// Implementation of SharePoint WebRequestExecutor that utilizes a provided HttpClient
    /// </summary>
    internal class HttpClientSPWebRequestExecutor : WebRequestExecutor
    {
        private readonly HttpWebRequest webRequest;
        private readonly HttpRequestMessage request;
        private readonly HttpClient httpClient;
        private HttpResponseMessage response;
        private string requestContentType;
        private RequestStream requestStream;

        /// <summary>
        /// Creates a WebRequestExecutorFactory that utilizes the specified HttpClient
        /// </summary>
        /// <param name="httpClientInstance">HttpClient to use when creating new web requests</param>
        /// <param name="context">A SharePoint ClientContext</param>
        /// <param name="requestUrl">The url to create the request for</param>
        public HttpClientSPWebRequestExecutor(HttpClient httpClientInstance, ClientRuntimeContext context, string requestUrl)
        {
            if (string.IsNullOrEmpty(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }
            
            httpClient = httpClientInstance;
            request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            webRequest = (HttpWebRequest)System.Net.WebRequest.Create(requestUrl);
#pragma warning restore SYSLIB0014 // Type or member is obsolete

            webRequest.Timeout = context.RequestTimeout;
            webRequest.Method = "POST";
            webRequest.Pipelined = false;
        }

        private class RequestStream : Stream
        {
            public override bool CanRead { get; } = true;

            public override bool CanSeek { get; } = true;

            public override bool CanWrite { get; } = true;

            public override long Length => BaseStream.Length;

            public Stream BaseStream { get; }

            public RequestStream(Stream baseStream)
            {
                BaseStream = baseStream;
            }

            public override long Position
            {
                get => BaseStream.Position;
                set => BaseStream.Position = value;
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return BaseStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return BaseStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                BaseStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                BaseStream.Write(buffer, offset, count);
            }
        }

        private async Task ExecuteImplementation()
        {
            foreach (string webRequestHeaderKey in webRequest.Headers.Keys)
            {
                request.Headers.Add(webRequestHeaderKey, webRequest.Headers[webRequestHeaderKey]);
            }

            if (webRequest.UserAgent != null)
            {
                request.Headers.UserAgent.ParseAdd(webRequest.UserAgent);
            }

            requestStream.Seek(0, SeekOrigin.Begin);
            request.Content = new StreamContent(requestStream);

            if (MediaTypeHeaderValue.TryParse(requestContentType, out var parsedValue))
            {
                request.Content.Headers.ContentType = parsedValue;
            }

            response = await httpClient.SendAsync(request);
        }

        public override HttpWebRequest WebRequest => webRequest;

        public override string RequestContentType
        {
            get => requestContentType;
            set => requestContentType = value;
        }

        public override string RequestMethod
        {
            get => request.Method.ToString();
            set => request.Method = new HttpMethod(value);
        }

        public override bool RequestKeepAlive
        {
            get => !request.Headers.ConnectionClose.GetValueOrDefault();
            set => request.Headers.ConnectionClose = !value;
        }

        public override WebHeaderCollection RequestHeaders => webRequest.Headers;

        public override Stream GetRequestStream()
        {
            if (requestStream == null)
            {
                requestStream = new RequestStream(new MemoryStream());
            }
            else if (!requestStream.BaseStream.CanWrite)
            {
                requestStream.Dispose();
                requestStream = new RequestStream(new MemoryStream());
            }
            return requestStream;
        }

        public override void Execute()
        {
            Task.Run(ExecuteImplementation).GetAwaiter().GetResult();
        }

        public override Task ExecuteAsync()
        {
            return ExecuteImplementation();
        }

        public override HttpStatusCode StatusCode
        {
            get
            {
                if (response == null)
                {
                    throw new InvalidOperationException();
                }

                return response.StatusCode;
            }
        }

        public override string ResponseContentType
        {
            get
            {
                if (response == null)
                {
                    throw new InvalidOperationException();
                }

                response.Content.Headers.TryGetValues("Content-Type", out var contentType);
                return contentType.FirstOrDefault();
            }
        }

        public override WebHeaderCollection ResponseHeaders
        {
            get
            {
                if (response == null)
                {
                    throw new InvalidOperationException();
                }

                var whc = new WebHeaderCollection();
                foreach (var header in response.Headers)
                {
                    foreach (var value in header.Value)
                    {
                        whc.Add(header.Key, value);
                    }
                }
                return whc;
            }
        }

        public override Stream GetResponseStream()
        {
            if (response == null)
            {
                throw new InvalidOperationException();
            }

            return response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
        }

        public override void Dispose()
        {
            request.Dispose();
            requestStream.Dispose();
            base.Dispose();
        }
    }
}
