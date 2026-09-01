using Cert4Update.Core;
using System.Diagnostics;

namespace CoreTest
{

    public class ProgressCmd: IProgress<ProgressData>
    {
        public ProgressCmd() { }

        public void Report(ProgressData value)
        {
            Console.ForegroundColor = value.Color;
            Console.BackgroundColor = value.BackColor;
            Console.WriteLine(value.Text);
            Debug.WriteLine(value.Text);
        }
    }
}
