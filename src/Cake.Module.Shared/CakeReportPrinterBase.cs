using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cake.Core;
using Cake.Core.Diagnostics;

namespace Cake.Module.Shared
{
    public abstract class CakeReportPrinterBase : ICakeReportPrinter
    {

        protected readonly ICakeContext _context;
        protected readonly IConsole _console;

        public CakeReportPrinterBase(IConsole console, ICakeContext context)
        {
            _context = context;
            _console = console;
        }
        public abstract void Write(CakeReport report);

        protected void WriteToConsole(CakeReport report)
        {
            var maxTaskNameLength = 29;
            foreach (var item in report)
            {
                if (item.TaskName.Length > maxTaskNameLength)
                {
                    maxTaskNameLength = item.TaskName.Length;
                }
            }

            maxTaskNameLength++;
            string lineFormat = "{0,-" + maxTaskNameLength + "}{1,-20}";
            _console.ForegroundColor = System.ConsoleColor.Green;

            // Write header.
            _console.WriteLine();
            _console.WriteLine(lineFormat, "Task", "Duration");
            _console.WriteLine(new string('-', 20 + maxTaskNameLength));

            // Write task status.
            foreach (var item in report)
            {
                if (ShouldWriteTask(item))
                {
                    _console.ForegroundColor = GetItemForegroundColor(item);
                    _console.WriteLine(lineFormat, item.TaskName, FormatDuration(item));
                }
            }

            // Write footer.
            _console.ForegroundColor = System.ConsoleColor.Green;
            _console.WriteLine(new string('-', 20 + maxTaskNameLength));
            _console.WriteLine(lineFormat, "Total:", FormatTime(GetTotalTime(report)));
        }

        protected bool ShouldWriteTask(CakeReportEntry item)
        {
            if (item.ExecutionStatus == CakeTaskExecutionStatus.Delegated)
            {
                return _context.Log.Verbosity >= Verbosity.Verbose;
            }

            return true;
        }

        protected static string FormatTime(TimeSpan time)
        {
            return time.ToString("c", CultureInfo.InvariantCulture);
        }

        protected static TimeSpan GetTotalTime(IEnumerable<CakeReportEntry> entries)
        {
            return entries.Select(i => i.Duration)
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }

        protected static string FormatDuration(CakeReportEntry item)
        {
            if (item.ExecutionStatus == CakeTaskExecutionStatus.Skipped)
            {
                return "Skipped";
            }

            return FormatTime(item.Duration);
        }

        protected static ConsoleColor GetItemForegroundColor(CakeReportEntry item)
        {
            return item.ExecutionStatus == CakeTaskExecutionStatus.Executed ? ConsoleColor.Green : ConsoleColor.Gray;
        }
    }
}