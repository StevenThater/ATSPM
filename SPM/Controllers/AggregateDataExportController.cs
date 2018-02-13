﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;
using MOE.Common.Business;
using MOE.Common.Business.Bins;
using MOE.Common.Business.FilterExtensions;
using MOE.Common.Business.WCFServiceLibrary;
using SPM.Models;
using MOE.Common.Models;
using MOE.Common.Models.ViewModel.Chart;
using MOE.Common.Business.FilterExtensions;

namespace SPM.Controllers
{
    public class AggregateDataExportController : Controller
    {
        MOE.Common.Models.Repositories.IMetricTypeRepository metricTyperepository =
            MOE.Common.Models.Repositories.MetricTypeRepositoryFactory.Create();

        // GET: DataExportViewModels
        public ActionResult GetRouteSignals(int id)
        {
            AggDataExportViewModel aggDataExportViewModel = new AggDataExportViewModel();
            var routeRepository = MOE.Common.Models.Repositories.RouteRepositoryFactory.Create();
            var signalRepository = MOE.Common.Models.Repositories.SignalsRepositoryFactory.Create();
            Route route = routeRepository.GetRouteByID(id);
            foreach (var routeignal in route.RouteSignals)
            {
                var signal = signalRepository.GetLatestVersionOfSignalBySignalID(routeignal.SignalId);
                aggDataExportViewModel.FilterSignals.Add(GetFilterSignal(signal));
            }
            return PartialView(aggDataExportViewModel);
        }

        public ActionResult GetSignal(string id)
        {
            var signalRepository = MOE.Common.Models.Repositories.SignalsRepositoryFactory.Create();
            AggDataExportViewModel aggDataExportViewModel = new AggDataExportViewModel();
            var signal = signalRepository.GetLatestVersionOfSignalBySignalID(id);
            aggDataExportViewModel.FilterSignals.Add(GetFilterSignal(signal));
            return PartialView("GetRouteSignals", aggDataExportViewModel);
        }

        private static FilterSignal GetFilterSignal(MOE.Common.Models.Signal signal)
        {
            var filterSignal = new FilterSignal
            {
                Exclude = false,
                SignalId = signal.SignalID,
                VersionId = signal.VersionID,
                Description = signal.SignalDescription
            };
            foreach (var approach in signal.Approaches)
            {
                var filterApproach = new FilterApproach
                {
                    ApproachId = approach.ApproachID,
                    Exclude = false,
                    Description = approach.Description
                };
                foreach (var detector in approach.Detectors)
                {
                    var filterDetector = new FilterDetector
                    {
                        Id = detector.ID,
                        Exclude = false,
                        Description = detector.Description
                    };
                    filterApproach.FilterDetectors.Add(filterDetector);
                }
                filterSignal.FilterApproaches.Add(filterApproach);
            }

            return filterSignal;
        }

        // POST: AggDataExportViewModel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMetric(AggDataExportViewModel aggDataExportViewModel)
        {
            switch (aggDataExportViewModel.SelectedMetricTypeId)
            {
                case 20:
                    return GetSplitFailChart(aggDataExportViewModel);
                    break;
                case 22:
                    return GetPreemptionChart(aggDataExportViewModel);
                    default:
                        return Content("<h1 class='text-danger'>Unkown Chart Type</h1>");
            }
        }

        private ActionResult GetPreemptionChart(AggDataExportViewModel aggDataExportViewModel)
        {
            SignalPreemptionAggregationOptions options = new SignalPreemptionAggregationOptions();
            Enum.TryParse(aggDataExportViewModel.SelectedChartType, out SeriesChartType tempSeriesChartType);
            options.SelectedChartType = tempSeriesChartType;
            if (TryValidateModel(aggDataExportViewModel) && aggDataExportViewModel.FilterSignals.Count > 0)
            {
                options.StartDate = aggDataExportViewModel.StartDateDay;
                options.EndDate = aggDataExportViewModel.EndDateDay;
                options.SelectedAggregationType = aggDataExportViewModel.SelectedAggregationType;
                options.SelectedXAxisType = aggDataExportViewModel.SelectedXAxisType;
                options.SeriesWidth = aggDataExportViewModel.SelectedSeriesWidth;
                options.SelectedSeries = aggDataExportViewModel.SelectedSeriesType;
                options.SelectedDimension = aggDataExportViewModel.SelectedDimension;
                SetTimeOptionsFromViewModel(aggDataExportViewModel, options);
                options.FilterSignals = aggDataExportViewModel.FilterSignals;
                options.SelectedPreemptionData = aggDataExportViewModel.SelectedPreemptionData;
                return GetChartFromService(options);
            }
            else
            {
                return Content("<h1 class='text-danger'>Missing Parameters</h1>");
            }
        }

        private ActionResult GetSplitFailChart(AggDataExportViewModel aggDataExportViewModel)
        {
            ApproachSplitFailAggregationOptions options = new ApproachSplitFailAggregationOptions();
            Enum.TryParse(aggDataExportViewModel.SelectedChartType, out SeriesChartType tempSeriesChartType);
            options.SelectedChartType = tempSeriesChartType;
            if (TryValidateModel(aggDataExportViewModel) && aggDataExportViewModel.FilterSignals.Count > 0)
            {
                options.StartDate = aggDataExportViewModel.StartDateDay;
                options.EndDate = aggDataExportViewModel.EndDateDay;
                options.SelectedAggregationType = aggDataExportViewModel.SelectedAggregationType;
                options.SelectedXAxisType = aggDataExportViewModel.SelectedXAxisType;
                options.SeriesWidth = aggDataExportViewModel.SelectedSeriesWidth;
                options.SelectedSeries = aggDataExportViewModel.SelectedSeriesType;
                options.SelectedDimension = aggDataExportViewModel.SelectedDimension;
                SetTimeOptionsFromViewModel(aggDataExportViewModel, options);
                options.FilterSignals = aggDataExportViewModel.FilterSignals;
                options.FilterDirections = aggDataExportViewModel.FilterDirections;
                return GetChartFromService(options);
            }
            else
            {
                return Content("<h1 class='text-danger'>Missing Parameters</h1>");
            }
        }

        private ActionResult GetChartFromService(ApproachSplitFailAggregationOptions options)
        {
            Models.MetricResultViewModel result = new Models.MetricResultViewModel();
            MetricGeneratorService.MetricGeneratorClient client =
                    new MetricGeneratorService.MetricGeneratorClient();
            try
            {
                client.Open();
                result.ChartPaths = client.CreateMetric(options);
                client.Close();
            }
            catch (Exception ex)
            {
                client.Close();
                return Content("<h1>" + ex.Message + "</h1>");
            }
            return PartialView("~/Views/DefaultCharts/MetricResult.cshtml", result);
        }

        private static void SetTimeOptionsFromViewModel(AggDataExportViewModel aggDataExportViewModel, SignalAggregationMetricOptions options)
        {
            string[] startTime;
            string[] endTime;
            int? startHour = null;
            int? startMinute = null;
            int? endHour = null;
            int? endMinute = null;
            BinFactoryOptions.TimeOptions timeOptions = BinFactoryOptions.TimeOptions.StartToEnd;
            if (!String.IsNullOrEmpty(aggDataExportViewModel.StartTime) &&
                !String.IsNullOrEmpty(aggDataExportViewModel.EndTime))
            {
                startTime = aggDataExportViewModel.StartTime.Split(':');
                startHour = Convert.ToInt32(startTime[0]);
                if (aggDataExportViewModel.SelectedStartAMPM.Contains("PM"))
                {
                    startHour += 12;
                }
                startMinute = startTime.Length > 1 ? Convert.ToInt32(startTime[1]) : 0;
                endTime = aggDataExportViewModel.EndTime.Split(':');
                endHour = Convert.ToInt32(endTime[0]);
                if (aggDataExportViewModel.SelectedEndAMPM.Contains("PM"))
                {
                    endHour += 12;
                }
                endMinute = endTime.Length > 1 ? Convert.ToInt32(endTime[1]) : 0;
                timeOptions = BinFactoryOptions.TimeOptions.TimePeriod;
            }
            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
            if (aggDataExportViewModel.Weekends)
            {
                daysOfWeek.AddRange(new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday });
            }
            if (aggDataExportViewModel.Weekdays)
            {
                daysOfWeek.AddRange(new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });
            }
            BinFactoryOptions.BinSize binSize = (BinFactoryOptions.BinSize)aggDataExportViewModel.SelectedBinSize;

            options.TimeOptions = new BinFactoryOptions(
                aggDataExportViewModel.StartDateDay,
                aggDataExportViewModel.EndDateDay,
                startHour, startMinute, endHour, endMinute, daysOfWeek,
                binSize,
                timeOptions);
        }

        // GET: DataExportViewModels
        public ActionResult Index()
        {
            var metricRepository = MOE.Common.Models.Repositories.MetricTypeRepositoryFactory.Create();
            AggDataExportViewModel viewModel = new AggDataExportViewModel();
             SetAggregateDataViewModelLists(viewModel);
            viewModel.SelectedMetricTypeId =20;
            viewModel.SelectedChartType = SeriesChartType.StackedColumn.ToString();
            viewModel.SelectedBinSize = 0;
            viewModel.StartDateDay = Convert.ToDateTime("10/17/2017");
            viewModel.EndDateDay = Convert.ToDateTime("10/18/2017");
            viewModel.Weekdays = true;
            viewModel.Weekends = true;
            return View(viewModel);
        }

        private void SetAggregateDataViewModelLists(AggDataExportViewModel viewModel)
        {
            var routeRepository = MOE.Common.Models.Repositories.RouteRepositoryFactory.Create();
            viewModel.SetSeriesTypes();
            viewModel.SetAggregationTypes();
            viewModel.SetBinSizeList();
            viewModel.SetChartTypes();
            viewModel.SetDefaultDates();
            viewModel.SetDimensions();
            viewModel.SetSeriesWidth();
            viewModel.SetXAxisTypes();
            viewModel.SetDirectionTypes();
            viewModel.Routes = routeRepository.GetAllRoutes();
            viewModel.MetricTypes = metricTyperepository.GetAllToAggregateMetrics();
        }

        private int[] SplitHourMinute(String timeFromFrontEnd)
        {
            int[] HourMinute = new int[]{0, 0};
            string[] splitted = timeFromFrontEnd.Split(':');
            int.TryParse(splitted[0], out HourMinute[0]);
            int.TryParse(splitted[1], out HourMinute[1]);
            return HourMinute;
        }
        

        public static List<int> StringToIntList(string str)
        {
            return str.Split(',').Select(int.Parse).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
