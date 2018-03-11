using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Transformer.Model;
using NLog;
using Metrics;
using Metrics.MetricData;


namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        
        public AsyncHttpServer(long accepabletTimeOfService)
        {
            _acceptableTimeOfService = accepabletTimeOfService;
            _listener = new HttpListener();
            Metric.Config
            .WithHttpEndpoint("http://localhost:1234/")
            .WithAllCounters();
            _processingTimer= Metric.Timer("Image process time", Unit.Requests,SamplingType.LongTerm,TimeUnit.Seconds,TimeUnit.Milliseconds);
        }
        
        public void Start(string prefix)
        {  
            lock (_listener)
            {
                if (!_isRunning)
                {
                    _listener.Prefixes.Clear();
                    _listener.Prefixes.Add(prefix);
                    _listener.Start();

                    _listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    _listenerThread.Start();
                    
                    _isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (_listener)
            {
                if (!_isRunning)
                    return;

                _listener.Stop();

                _listenerThread.Abort();
                _listenerThread.Join();
                
                _isRunning = false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Stop();

            _listener.Close();
        }
        
        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (_listener.IsListening)
                    { 
                        var context = _listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    //log errors
                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Error(error.Message);
                    Console.WriteLine(error.Message);
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            //request handling
            using (_processingTimer.NewContext())
            {
                var time = ValueReader.GetCurrentValue(_processingTimer)
                .Scale(TimeUnit.Milliseconds,TimeUnit.Milliseconds)
                .Histogram.Percentile95;
            
                if(time < _acceptableTimeOfService)
                {
                    string paramString = listenerContext.Request.RawUrl;
                    if (listenerContext.Request.HttpMethod == "POST")
                    {
                        if (paramString.StartsWith("/process/"))
                        {
                            var result = ProcessImage(listenerContext);
                            using (var writer = new BinaryWriter(result.Item1.Response.OutputStream))
                                writer.Write(result.Item2);
                            return;
                        }
                    }

                    listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                        await writer.WriteLineAsync();
                }
                else
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                        await writer.WriteLineAsync();
                }
            }
        }

        private Tuple<HttpListenerContext,byte[]> ProcessImage(HttpListenerContext httpContext)
        {
            string paramString = httpContext.Request.RawUrl;
            byte[] inputImg;
            byte[] outputImg;
            paramString = paramString.Remove(0, 9);
            paramString = paramString.Replace('/', ' ');
            TransformationParametrs tParams;
            try
            {
                MemoryStream inStream = new MemoryStream();
                httpContext.Request.InputStream.CopyTo(inStream);
                inputImg = inStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
            if (inputImg.LongLength > 100000)// проверка размера файла
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new Tuple<HttpListenerContext, byte[]>(httpContext, null);
            }
            try
            {
                tParams = TransformationParametrs.Parse(paramString);
            }
            catch (ArgumentException ae)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new Tuple<HttpListenerContext, byte[]>(httpContext, null);
            }

            ImageTransformer.Transformer.ImageTransformer transformer = new ImageTransformer.Transformer.ImageTransformer();

            try
            {
                outputImg = transformer.Transform(inputImg, tParams);
            }
            catch (ArgumentOutOfRangeException aoorEx)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                return new Tuple<HttpListenerContext, byte[]>(httpContext, null);
            }
            catch (ArgumentException argEx)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new Tuple<HttpListenerContext, byte[]>(httpContext, null);
            }

            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            return new Tuple<HttpListenerContext, byte[]>(httpContext, outputImg);
        }
        private readonly HttpListener _listener;
        private Thread _listenerThread;
        private bool _disposed;
        private volatile bool _isRunning;
        private volatile Metrics.Timer _processingTimer;
        private Double _acceptableTimeOfService;

    }
}