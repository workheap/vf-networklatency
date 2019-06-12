using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLatency.Console
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Verbose")]
            public bool Verbose { get; set; }

            [Option('m', "mode", Required = false, HelpText = "Execution mode: NoContext (default) or UnmanagedWin32", Default = Mode.NoContext)]
            public Mode Mode { get; set; }

            [Option('t', "times", Required = false, HelpText = "How many times to execute", Default = 10)]
            public int Times { get; set; }

            [Option('s', "source", Required = true, HelpText = "Source file path")]
            public string Source { get; set; }

            [Option('u', "username", Required = false, HelpText = "Username")]
            public string Username { get; set; }

            [Option('p', "password", Required = false, HelpText = "Password")]
            public string Password { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                               .WithParsed<Options>(o =>
                               {
                                   if (o.Mode == Mode.NoContext)
                                   {
                                       ReadFileNoContext(o.Source, o.Times, o.Verbose);
                                   }
                                   else if (o.Mode == Mode.UnmanagedWin32)
                                   {
                                       ReadFileUnamanagedWin32(o.Source, o.Times, o.Username, o.Password, o.Verbose);
                                   }
                               });
        }

        /// <summary>
        /// Read file without security context
        /// </summary>
        /// <param name="path"></param>
        /// <param name="times"></param>
        private static void ReadFileNoContext(string path, int times, bool verbose)
        {
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                sw.Start();
                try
                {
                    using (var ms = new MemoryStream())
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (verbose)
                        {
                            sw.Stop();
                            System.Console.WriteLine($"Opening {path}. Elapsed time (ms): {sw.ElapsedMilliseconds}");
                            sw.Restart();
                        }

                        Copy(fs, ms, 4096);
                    }
                }
                finally
                {
                    sw.Stop();
                    System.Console.WriteLine($"Copying {path}. Elapsed time (ms): {sw.ElapsedMilliseconds}");
                    sw.Reset();
                }
            }
        }

        /// <summary>
        /// Read file using context NetworkCredentials
        /// </summary>
        /// <param name="path"></param>
        /// <param name="times"></param>
        private static void ReadFileUnamanagedWin32(string path, int times, string username, string password, bool verbose)
        {
            var sw = new Stopwatch();

            for (var i = 0; i < times; i++)
            {
                var networkPath = Path.GetDirectoryName(path);
                var credentials = new NetworkCredential(username, password);
                sw.Start();
                using (new ConnectToSharedFolder(networkPath, credentials))
                {
                    if (verbose)
                    {
                        sw.Stop();
                        System.Console.WriteLine($"Establish connection to network shared folder {networkPath}. Elapsed time(ms): {sw.ElapsedMilliseconds}");
                        sw.Restart();
                    }

                    try
                    {
                        using (var ms = new MemoryStream())
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            if (verbose)
                            {
                                sw.Stop();
                                System.Console.WriteLine($"Opening {path}. Elapsed time (ms): {sw.ElapsedMilliseconds}");
                                sw.Restart();
                            }

                            Copy(fs, ms, 4096);
                        }
                    }
                    finally
                    {
                        sw.Stop();
                        System.Console.WriteLine($"Copying {path}. Elapsed time (ms): {sw.ElapsedMilliseconds}");
                        sw.Reset();
                    }
                }
            }
        }

        private static int Copy(Stream source, Stream target, int chunkSize)
        {
            if (source.CanSeek && source.Position != 0)
            {
                source.Seek(0, SeekOrigin.Begin);
            }

            var buffer = new byte[chunkSize];
            var offset = 0;

            var bytesRead = 0;

            while ((bytesRead = source.Read(buffer, 0, chunkSize)) > 0)
            {
                // save to output stream
                target.Write(buffer, 0, bytesRead);
                offset += bytesRead;
            }

            return offset;
        }

    }

    public enum Mode
    {
        NoContext,
        UnmanagedWin32
    }
}
