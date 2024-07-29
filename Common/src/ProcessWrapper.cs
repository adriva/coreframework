using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Provides functionality to start and track the status of an external process.
    /// </summary>
    public sealed class ProcessWrapper
    {
        private readonly string ExecutableFile;

        private readonly string[] Arguments;

        /// <summary>
        /// Creates a new instance of the ProcessWrapper class targeting an executable file.
        /// </summary>
        /// <param name="executableFile">The name or path of the executable file that will be used to start the process.</param>
        /// <param name="args">The command line arguments that will be passed to the executable.</param>
        public ProcessWrapper(string executableFile, params string[] args)
        {
            this.ExecutableFile = executableFile;

            if (null != args)
            {
                this.Arguments = args.Select(arg => $"\"{arg}\"").ToArray();
            }
        }

        /// <summary>
        /// Runs the given executable and waits until the timeout has elapsed or the process has exited, whichever comes first.
        /// </summary>
        /// <param name="timeoutInSeconds">The number of seconds to wait for the process to exit.</param>
        /// <param name="logger">A logger instance that is used to perform logging operations.</param>
        /// <param name="workingDirectory">The working directory of the process to be started.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the Result parameter indicates if the process has exited within the given timeout value or not.</returns>
        public async Task<bool> RunAsync(int timeoutInSeconds, ILogger logger, string workingDirectory = null)
        {
            using (StringWriter output = new StringWriter())
            {
                return await this.RunAsync(timeoutInSeconds, output, logger, workingDirectory);
            }
        }

        /// <summary>
        /// Runs the given executable and waits until the timeout has elapsed or the process has exited, whichever comes first.
        /// </summary>
        /// <param name="timeoutInSeconds">The number of seconds to wait for the process to exit.</param>
        /// <param name="output">A text writer that is used to write stdout and stderr output of the process to.</param>
        /// <param name="logger">A logger instance that is used to perform logging operations.</param>
        /// <param name="workingDirectory">The working directory of the process to be started.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the Result parameter indicates if the process has exited within the given timeout value or not.</returns>
        public async Task<bool> RunAsync(int timeoutInSeconds, TextWriter output, ILogger logger, string workingDirectory = null)
        {
            timeoutInSeconds = Math.Max(0, timeoutInSeconds);

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    Arguments = string.Join(" ", this.Arguments),
                    FileName = this.ExecutableFile,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                process.Start();

                bool hasGracefulExit = await Task<bool>.Run(() => process.WaitForExit(timeoutInSeconds * 1000));

                if (!hasGracefulExit)
                {
                    bool hasKilled = false;
                    int retryCount = 3;

                    while (!hasKilled && 0 < retryCount)
                    {
                        try
                        {
                            process.Kill();
                            hasKilled = true;
                        }
                        catch (Exception error)
                        {
                            logger.LogWarning($"Couldn't kill process {this.ExecutableFile}. Error {error.GetType().FullName}");
                            logger.LogWarning(error, "May retry.");
                            hasKilled = false;
                            --retryCount;
                            Thread.Yield();
                        }
                    }
                }

                var outputData = await process.StandardOutput.ReadToEndAsync();
                var errorData = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrWhiteSpace(outputData))
                {
                    await output.WriteAsync(outputData);
                }

                if (!string.IsNullOrWhiteSpace(errorData))
                {
                    await output.WriteAsync(errorData);
                }

                logger.LogTrace($"Process output from {process.Id} [{process.StartInfo.FileName}]");
                logger.LogTrace(output.ToString());

                return hasGracefulExit ? 0 == process.ExitCode : false;
            }
        }
    }
}
