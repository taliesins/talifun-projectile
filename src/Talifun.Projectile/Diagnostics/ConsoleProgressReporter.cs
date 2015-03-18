using System;

namespace Talifun.Projectile.Diagnostics
{
    public class ConsoleProgressReporter : IProgressReporter
    {
        private string _currentOperation;
        private int _progressPercentage;

        public void ReportProgress(string operation, long currentPosition, long total)
        {
            var percent = (int)((double)currentPosition/total * 100d + 0.5);
            if (_currentOperation != operation)
            {
                _progressPercentage = -1;
                _currentOperation = operation;
            }

            if (_progressPercentage != percent && percent % 10 == 0)
            {
                _progressPercentage = percent;
                Console.WriteLine("{0}: {1}%", _currentOperation, percent);
            }
        }
    }
}