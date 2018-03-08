using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Transformer;
using Kontur.ImageTransformer.Transformer.Model;
using System.Diagnostics;
using NLog;
namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        
        public AsyncHttpServer()
        {
            listener = new HttpListener();
  
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
                    // TODO: log errors
                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Error(error.Message);
                    Console.WriteLine(error.Message);
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
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
            if(inputImg.LongLength> 100000)// проверка размера файла
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    await writer.WriteLineAsync();
                throw new ArgumentException("Слишком большой файл");
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
            catch(ArgumentException argEx)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    await writer.WriteLineAsync();
                throw new ArgumentException(argEx.Message);
            }

            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var writer = new BinaryWriter(listenerContext.Response.OutputStream))
                writer.Write(outputImg);
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}