using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EtwPerformanceProfiler;

namespace SessionsActivity
{
    public partial class Form1 : Form
    {
        static readonly object timerLock = new object();

        private Dictionary<string, double> chartValues = new Dictionary<string, double>(); 

        public Form1()
        {
            InitializeComponent();

            var chartDataGeneratingTask = new Task(this.GenerateChartData);

            chartDataGeneratingTask.Start();
        }

        private void GenerateChartData()
        {
            const int SleepDuration = 1000;

            DynamicProfilerEventProcessor dynamicProfilerEventProcessor = new DynamicProfilerEventProcessor(DynamicProfilerEventProcessor.MultipleSessionsId);
            dynamicProfilerEventProcessor.Start();

            while (true)
            {

                Thread.Sleep(SleepDuration);
                dynamicProfilerEventProcessor.Suspend();

                List<string> xValues = new List<string>();
                List<double> yValues = new List<double>();

                foreach (AggregatedEventNode node in dynamicProfilerEventProcessor.FlattenCallTree())
                {
                    if (node.Depth == 0)
                    {
                        xValues.Add(node.StatementName);
                        yValues.Add(Math.Min(Math.Round(node.DurationMSec / SleepDuration * 100), 100));
                    }
                }

                lock (timerLock)
                {
                    if (this.removeInactiveSessions.Checked)
                    {
                        this.chartValues = new Dictionary<string, double>();
                    }
                    else
                    {
                        for (int i = 0; i < this.chartValues.Count; ++i)
                        {
                            this.chartValues[this.chartValues.ElementAt(i).Key] = 0;
                        }
                    }

                    for (int i = 0; i < xValues.Count; ++i)
                    {
                        this.chartValues[xValues[i]] = yValues[i];
                    }
                }

                dynamicProfilerEventProcessor.Initialize();
                dynamicProfilerEventProcessor.Resume();
            }

            dynamicProfilerEventProcessor.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (timerLock)
            {
                this.sessionsActivity.Series[0].Points.DataBindXY(this.chartValues.Keys, this.chartValues.Values);
            }
        }

        /// <summary>
        /// This function is used for debug purposes.
        /// </summary>
        /// <param name="values">List of values to be formated into string.</param>
        /// <returns>String value represent the list of value.</returns>
        private static string FormatValues(IEnumerable<double> values)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var value in values)
            {
                sb.Append(value).Append(", ");
            }

            return sb.ToString();
        }
    }
}
