using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Transformer;
using Kontur.ImageTransformer.Transformer.Model;
using System.Diagnostics;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        private PerformanceCounter performanceCounter;
        private long _acceptableTimeOfService;
        public AsyncHttpServer()
        {
            listener = new HttpListener();
            performanceCounter = new PerformanceCounter(1000, 100);
            _acceptableTimeOfService = 1000;
        }
        
        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();
                    
                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();
                
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }
        
        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        if (performanceCounter.GetAvgTime() <= _acceptableTimeOfService)
                            Task.Run(() => HandleContextAsync(context));
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                            using (var writer = new StreamWriter(context.Response.OutputStream))
                                writer.WriteLine();
                            //throw new ArgumentException("Неправильный запрос");
                        }
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    // TODO: log errors
                    Console.WriteLine(error.Message);
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            Stopwatch stopwatch=new Stopwatch();
            stopwatch.Start();
            // TODO: implement request handling
            string paramString = listenerContext.Request.RawUrl;
            if(listenerContext.Request.HttpMethod!="POST")
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    await writer.WriteLineAsync();
                throw new ArgumentException("Неправильный запрос");
            }
            paramString=paramString.Trim('/');
            if(!paramString.StartsWith("process/"))
            {
                //Console.WriteLine(ae.Message);
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    await writer.WriteLineAsync();
                throw new ArgumentException("Неправильный запрос");
            }
            paramString=paramString.Remove(0, 8);
            Console.WriteLine(paramString);
            paramString = paramString.Replace('/', ' ');
            byte[] inputImg;
            byte[] outputImg;
            TransformationParametrs tParams;
            try
            {
                MemoryStream inStream = new MemoryStream();
                await listenerContext.Request.InputStream.CopyToAsync(inStream);
                inputImg = inStream.ToArray();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
            try
            {
                tParams = TransformationParametrs.Parse(paramString);
            }
            catch(ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    await writer.WriteLineAsync();
                throw new ArgumentException(ae.Message);
            }

            ImageTransformer.Transformer.ImageTransformer transformer = new ImageTransformer.Transformer.ImageTransformer();
           
            try
            {
               outputImg = transformer.Transform(inputImg, tParams);
            }
            catch(ArgumentOutOfRangeException aoorEx)
            {
                Console.WriteLine(aoorEx.Message);
                listenerContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    await writer.WriteLineAsync();
                throw new ArgumentOutOfRangeException(aoorEx.Message);
            }

            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var writer = new BinaryWriter(listenerContext.Response.OutputStream))
                writer.Write(outputImg);
            await performanceCounter.AddTime(stopwatch.ElapsedMilliseconds);
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}