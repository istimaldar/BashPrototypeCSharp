using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace BashExecutorPrototype
{
    class Program
    {
        static void Main(string[] args)
        {
            var prog = new Program();
            BashResult result = prog.Command("(echo -e $'5 * * * * ( echo $(date) && bash /srv/scripts/li_15.sh ) >> /var/log/cron/job_15.log\n5 * * * * ( echo $(date) && bash /srv/scripts/li_15.sh ) >> /var/log/cron/job_15.log' | crontab -)");

            Console.WriteLine($"Exit code: {result.ExitCode}");
            foreach (var line in result.Lines)
            {
                Console.WriteLine($"{line}");
            }
            
            Console.WriteLine($"Errors: {result.ErrorMsg}");

            Console.ReadLine();
        }

        public BashResult Command(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"").Replace("\\'", "\\\\'").Replace("'", "\\'");

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c $'{escapedArgs}'",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.OutputDataReceived += (sender, e) => { stdout.AppendLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { stderr.AppendLine(e.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return new BashResult
            {
                Lines = Regex.Split(stdout.ToString(), @"\r?\n|\r"),
                ErrorMsg = stderr.ToString(),
                ExitCode = process.ExitCode
            };
        }

        
    }
    public class BashResult
    {
        public int ExitCode { get; set; }

        public string[] Lines { get; set; }

        public string ErrorMsg { get; set; }
    }
}
