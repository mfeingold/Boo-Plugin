using System;
using System.Diagnostics;
using System.IO;

namespace Hill30.BooProject.BooISh
{
    public class BooIShWrapper
    {
        private readonly Process booInterpreter;
        private readonly Action<string> recorder;
        public BooIShWrapper(Action<string> recorder)
        {
            this.recorder = recorder;
            booInterpreter =
                new Process
                    {
                        StartInfo =
                            {
                                UseShellExecute = false,
                                FileName =
                                    @"C:\Users\mfeingol\Documents\Projects\Boo Plugin\BooProject\Boo_files\booi.exe",
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                RedirectStandardInput = true
                            }
                    };
            booInterpreter.Start();
            booInterpreter.OutputDataReceived += booInterpreter_OutputDataReceived;
            booInterpreter.ErrorDataReceived += booInterpreter_OutputDataReceived;
            booInterpreter.BeginOutputReadLine();
        }

        private void booInterpreter_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            recorder(e.Data + "\n");
        }

        public StreamWriter Input { get { return booInterpreter.StandardInput; } }

    }
}
