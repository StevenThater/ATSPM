﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI.DataVisualization.Charting;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business
{
    public class ArriveOnRedChart
    {
        public Chart chart = new Chart();
        private double totalAoR;
        private double totalCars;
        private double totalPercentAoR;

        public ArriveOnRedChart(AoROptions options, SignalPhase signalPhase)
        {
            Options = options;
            var reportTimespan = Options.EndDate - Options.StartDate;

            //Set the chart properties
            ChartFactory.SetImageProperties(chart);

            //Create the chart legend
            var chartLegend = new Legend();
            chartLegend.Name = "MainLegend";
            chartLegend.Docking = Docking.Left;
            chart.Legends.Add(chartLegend);


            //Create the chart area
            var chartArea = new ChartArea();
            chartArea.Name = "ChartArea1";
            if (Options.YAxisMax != null)
                chartArea.AxisY.Maximum = Options.YAxisMax.Value;

            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Title = "Volume (Vehicles Per Hour)";
            chartArea.AxisY.Interval = 500;
            chartArea.AxisY2.Title = "Percent AoR";
            chartArea.AxisY2.Maximum = 100;
            chartArea.AxisY2.Interval = 10;
            chartArea.AxisY2.Enabled = AxisEnabled.True;


            chartArea.AxisX.Title = "Time (Hour of Day)";
            chartArea.AxisX.IntervalType = DateTimeIntervalType.Hours;
            chartArea.AxisX.LabelStyle.Format = "HH";
            chartArea.AxisX2.LabelStyle.Format = "HH";
            if (reportTimespan.Days < 1)
                if (reportTimespan.Hours > 1)
                {
                    chartArea.AxisX2.Interval = 1;
                    chartArea.AxisX.Interval = 1;
                }
                else
                {
                    chartArea.AxisX.LabelStyle.Format = "HH:mm";
                    chartArea.AxisX2.LabelStyle.Format = "HH:mm";
                }
            chartArea.AxisX2.Enabled = AxisEnabled.True;
            chartArea.AxisX2.MajorTickMark.Enabled = true;
            chartArea.AxisX2.IntervalType = DateTimeIntervalType.Hours;
            chartArea.AxisX2.LabelAutoFitStyle = LabelAutoFitStyles.None;

            chart.ChartAreas.Add(chartArea);


            //Add the point series

            var AoRSeries = new Series();
            AoRSeries.ChartType = SeriesChartType.Line;
            AoRSeries.BorderDashStyle = ChartDashStyle.Dash;
            AoRSeries.Color = Color.Red;
            AoRSeries.Name = "Arrivals on Red";
            AoRSeries.XValueType = ChartValueType.DateTime;

            var TVSeries = new Series();
            TVSeries.ChartType = SeriesChartType.Line;
            TVSeries.BorderDashStyle = ChartDashStyle.Dash;
            TVSeries.Color = Color.Black;
            TVSeries.Name = "Total Vehicles";
            TVSeries.XValueType = ChartValueType.DateTime;

            var PARSeries = new Series();
            PARSeries.ChartType = SeriesChartType.Line;
            PARSeries.Color = Color.Red;
            PARSeries.Name = "Percent Arrivals on Red";
            PARSeries.BorderWidth = 2;
            PARSeries.XValueType = ChartValueType.DateTime;
            PARSeries.YAxisType = AxisType.Secondary;


            var pointSeries = new Series();
            pointSeries.ChartType = SeriesChartType.Point;
            pointSeries.Color = Color.White;
            pointSeries.Name = "Posts";
            pointSeries.XValueType = ChartValueType.DateTime;
            pointSeries.IsVisibleInLegend = false;


            chart.Series.Add(pointSeries);
            chart.Series.Add(AoRSeries);
            chart.Series.Add(PARSeries);
            chart.Series.Add(TVSeries);


            //Add points at the start and and of the x axis to ensure
            //the graph covers the entire period selected by the user
            //whether there is data or not
            chart.Series["Posts"].Points.AddXY(Options.StartDate, 0);
            chart.Series["Posts"].Points.AddXY(Options.EndDate, 0);

            AddDataToChart(chart, signalPhase);
            SetPlanStrips(signalPhase.Plans, chart, Options.StartDate, Options.ShowPlanStatistics);
        }

        public AoROptions Options { get; set; }

        private void SetChartTitles(SignalPhase signalPhase, Dictionary<string, string> statistics)
        {
            chart.Titles.Add(ChartTitleFactory.GetChartName(Options.MetricTypeID));
            chart.Titles.Add(ChartTitleFactory.GetSignalLocationAndDateRange(
                Options.SignalID, Options.StartDate, Options.EndDate));
            if (!signalPhase.Approach.IsProtectedPhaseOverlap)
                chart.Titles.Add(ChartTitleFactory.GetPhaseAndPhaseDescriptions(
                    signalPhase.Approach.ProtectedPhaseNumber, signalPhase.Approach.DirectionType.Description));
            else
                chart.Titles.Add(
                    ChartTitleFactory.GetPhaseAndPhaseDescriptions(signalPhase.Approach.ProtectedPhaseNumber,
                        " Overlap"));
            chart.Titles.Add(ChartTitleFactory.GetStatistics(statistics));
        }


        protected void AddDataToChart(Chart chart, SignalPhase signalPhase)
        {
            double totalDetectorHits = 0;
            var yAxisHolder = 0;
            if (signalPhase.Cycles.Count > 0)
            {
                var dt = signalPhase.StartDate;
                while (dt < signalPhase.EndDate)
                {
                    double binTotalStops = 0;
                    double binPercentAoR = 0;
                    double binDetectorHits = 0;
                    var cycles = signalPhase.Cycles.Where(c =>
                        c.StartTime >= dt && c.EndTime < dt.AddMinutes(Options.SelectedBinSize));
                    foreach (var cycle in cycles)
                    {
                        totalDetectorHits += cycle.DetectorEvents.Count;
                        binDetectorHits += cycle.DetectorEvents.Count;
                        foreach (var detectorPoint in cycle.DetectorEvents)
                            if (detectorPoint.YPoint < cycle.GreenLineY)
                            {
                                binTotalStops++;
                                totalAoR++;
                            }
                        if (binDetectorHits > 0)
                            binPercentAoR = binTotalStops / binDetectorHits * 100;
                    }
                    chart.Series["Percent Arrivals on Red"].Points.AddXY(dt, binPercentAoR);
                    chart.Series["Total Vehicles"].Points.AddXY(dt, binDetectorHits * (60 / Options.SelectedBinSize));
                    chart.Series["Arrivals on Red"].Points.AddXY(dt, binTotalStops * (60 / Options.SelectedBinSize));
                    dt = dt.AddMinutes(Options.SelectedBinSize);
                    if (yAxisHolder < binTotalStops * (60 / Options.SelectedBinSize) && Options.YAxisMax == null)
                    {
                        yAxisHolder = Convert.ToInt16(binDetectorHits * (60 / Options.SelectedBinSize));
                        yAxisHolder = RoundToNearest(yAxisHolder, 100);
                        chart.ChartAreas[0].AxisY.Maximum = yAxisHolder + 250;
                    }
                }
            }
            totalCars = totalDetectorHits;

            if (totalDetectorHits > 0)
                totalPercentAoR = totalAoR / totalCars * 100;
            var statistics = new Dictionary<string, string>();
            statistics.Add("Total Detector Hits", totalCars.ToString());
            statistics.Add("Total AoR", totalAoR.ToString());
            statistics.Add("Percent AoR for the select period", Math.Round(totalPercentAoR).ToString());
            SetChartTitles(signalPhase, statistics);
        }


        protected void SetPlanStrips(List<PlanPcd> planCollection, Chart chart, DateTime graphStartDate,
            bool showPlanStatistics)
        {
            var backGroundColor = 1;
            foreach (var plan in planCollection)
            {
                var stripline = new StripLine();
                //Creates alternating backcolor to distinguish the plans
                if (backGroundColor % 2 == 0)
                    stripline.BackColor = Color.FromArgb(120, Color.LightGray);
                else
                    stripline.BackColor = Color.FromArgb(120, Color.LightBlue);

                //Set the stripline properties
                stripline.IntervalOffsetType = DateTimeIntervalType.Hours;
                stripline.Interval = 1;
                stripline.IntervalOffset = (plan.StartTime - graphStartDate).TotalHours;
                stripline.StripWidth = (plan.EndTime - plan.StartTime).TotalHours;
                stripline.StripWidthType = DateTimeIntervalType.Hours;

                chart.ChartAreas["ChartArea1"].AxisX.StripLines.Add(stripline);

                //Add a corrisponding custom label for each strip
                var Plannumberlabel = new CustomLabel();
                Plannumberlabel.FromPosition = plan.StartTime.ToOADate();
                Plannumberlabel.ToPosition = plan.EndTime.ToOADate();
                switch (plan.PlanNumber)
                {
                    case 254:
                        Plannumberlabel.Text = "Free";
                        break;
                    case 255:
                        Plannumberlabel.Text = "Flash";
                        break;
                    case 0:
                        Plannumberlabel.Text = "Unknown";
                        break;
                    default:
                        Plannumberlabel.Text = "Plan " + plan.PlanNumber;

                        break;
                }

                Plannumberlabel.ForeColor = Color.Black;
                Plannumberlabel.RowIndex = 3;

                chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(Plannumberlabel);

                if (showPlanStatistics)
                {
                    var aogLabel = new CustomLabel();
                    aogLabel.FromPosition = plan.StartTime.ToOADate();
                    aogLabel.ToPosition = plan.EndTime.ToOADate();
                    aogLabel.Text = 100 - plan.PercentArrivalOnGreen + "% AoR\n";
                    aogLabel.LabelMark = LabelMarkStyle.LineSideMark;
                    aogLabel.ForeColor = Color.Blue;
                    aogLabel.RowIndex = 2;
                    chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(aogLabel);

                    var statisticlabel = new CustomLabel();
                    statisticlabel.FromPosition = plan.StartTime.ToOADate();
                    statisticlabel.ToPosition = plan.EndTime.ToOADate();
                    statisticlabel.Text = 100 - plan.PercentGreenTime + "% RT";
                    statisticlabel.ForeColor = Color.Red;
                    statisticlabel.RowIndex = 1;
                    chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(statisticlabel);
                }
                //Change the background color counter for alternating color
                backGroundColor++;
            }
        }

        private static int RoundToNearest(int iNumberToRound, int iToNearest)
        {
            //int iToNearest = 100;
            var iNearest = 0;
            var bIsUpper = false;

            var iRest = iNumberToRound % iToNearest;
            if (iNumberToRound == 550) bIsUpper = true;

            if (bIsUpper)
            {
                iNearest = iNumberToRound - iRest + iToNearest;
                return iNearest;
            }
            if (iRest > iToNearest / 2)
            {
                iNearest = iNumberToRound - iRest + iToNearest;
                return iNearest;
            }
            if (iRest < iToNearest / 2)
            {
                iNearest = iNumberToRound - iRest;
                return iNearest;
            }

            return 0;
        }
    }
}