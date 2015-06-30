using System;
using System.Text;

namespace ImageHunter
{
    public static class ErrorExtensions
    {
        public static string BuildLog(this AggregateException ae)
        {
            var flattenedException = ae.Flatten();
            var errorBuilder = new StringBuilder();

            foreach (var e in flattenedException.InnerExceptions)
            {
                errorBuilder.AppendLine(BuildLog(e));
            }
            return errorBuilder.ToString();
        }

        public static string BuildLog(this Exception ex)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendLine(ex.Message);

            var inspectedError = ex.InnerException;
            while (inspectedError != null)
            {
                errorBuilder.AppendLine(string.Format(" - {0}", inspectedError.Message));
                inspectedError = inspectedError.InnerException;
            }

            return errorBuilder.ToString();
        }
    }
}