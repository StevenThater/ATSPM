﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace MOE.Common.Models.Repositories
{
    public class ControllerEventLogRepository : IControllerEventLogRepository
    {
        private readonly SPM _db = new SPM();

        public ControllerEventLogRepository()
        {
            _db.Database.CommandTimeout = 180;
        }

        public int GetRecordCountByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime,
            List<int> eventParameters,
            List<int> eventCodes)
        {
            var query = _db.Controller_Event_Log.Where(c =>
                c.SignalID == signalId && c.Timestamp >= startTime && c.Timestamp <= endTime);
            if (eventParameters != null && eventParameters.Count > 0)
                query = query.Where(c => eventParameters.Contains(c.EventParam));
            if (eventCodes != null && eventCodes.Count > 0)
                query = query.Where(c => eventCodes.Contains(c.EventCode));
            return query.Count();
        }

        public List<Controller_Event_Log> GetRecordsByParameterAndEvent(string signalId, DateTime startTime,
            DateTime endTime, List<int> eventParameters, List<int> eventCodes)
        {
            var query = _db.Controller_Event_Log.Where(c =>
                c.SignalID == signalId && c.Timestamp >= startTime && c.Timestamp <= endTime);
            if (eventParameters != null && eventParameters.Count > 0)
                query = query.Where(c => eventParameters.Contains(c.EventParam));
            if (eventCodes != null && eventCodes.Count > 0)
                query = query.Where(c => eventCodes.Contains(c.EventCode));
            return query.ToList();
        }

        public List<Controller_Event_Log> GetAllAggregationCodes(string signalId, DateTime startTime, DateTime endTime)
        {
            var codes = new List<int> {150, 114, 113, 112, 105, 102, 1, 45};
            var records = _db.Controller_Event_Log
                .Where(c => c.SignalID == signalId && c.Timestamp >= startTime && c.Timestamp <= endTime &&
                            codes.Contains(c.EventCode))
                .ToList();
            return records;
        }

        public int GetDetectorActivationCount(string signalId,
            DateTime startTime, DateTime endTime, int detectorChannel)
        {
            var count = (from cel in _db.Controller_Event_Log
                where cel.Timestamp >= startTime
                      && cel.Timestamp < endTime
                      && cel.SignalID == signalId
                      && cel.EventParam == detectorChannel
                      && cel.EventCode == 82
                select cel).Count();
            return count;
        }

        public double GetTmcVolume(DateTime startDate, DateTime endDate, string signalId, int phase)
        {
            var repository =
                SignalsRepositoryFactory.Create();
            var signal = repository.GetVersionOfSignalByDate(signalId, startDate);
            var graphDetectors = signal.GetDetectorsForSignalByPhaseNumber(phase);

            var tmcChannels = new List<int>();
            foreach (var gd in graphDetectors)
            foreach (var dt in gd.DetectionTypes)
                if (dt.DetectionTypeID == 4)
                    tmcChannels.Add(gd.DetChannel);


            double count = (from cel in _db.Controller_Event_Log
                where cel.Timestamp >= startDate
                      && cel.Timestamp < endDate
                      && cel.SignalID == signalId
                      && tmcChannels.Contains(cel.EventParam)
                      && cel.EventCode == 82
                select cel).Count();

            return count;
        }

        public List<Controller_Event_Log> GetSplitEvents(string signalId, DateTime startTime, DateTime endTime)
        {
            var results = (from r in _db.Controller_Event_Log
                where r.SignalID == signalId && r.Timestamp > startTime && r.Timestamp < endTime
                      && r.EventCode > 130 && r.EventCode < 150
                select r).ToList();

            return results;
        }

        public List<Controller_Event_Log> GetSignalEventsBetweenDates(string signalId,
            DateTime startTime, DateTime endTime)
        {
            try
            {
                return (from r in _db.Controller_Event_Log
                    where r.SignalID == signalId
                          && r.Timestamp >= startTime
                          && r.Timestamp < endTime
                    select r).ToList();
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetSignalEventsBetweenDates";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }

        public List<Controller_Event_Log> GetTopNumberOfSignalEventsBetweenDates(string signalId, int numberOfRecords,
            DateTime startTime, DateTime endTime)
        {
            try
            {
                var events =
                (from r in _db.Controller_Event_Log
                    where r.SignalID == signalId
                          && r.Timestamp >= startTime
                          && r.Timestamp < endTime
                    select r).Take(numberOfRecords).ToList();

                if (events != null)
                    return events;
                var emptyEvents = new List<Controller_Event_Log>();
                return emptyEvents;
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetTopNumberOfSignalEventsBetweenDates";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }

        public int GetRecordCount(string signalId, DateTime startTime, DateTime endTime)
        {
            try
            {
                return
                (from r in _db.Controller_Event_Log
                    where r.SignalID == signalId
                          && r.Timestamp >= startTime
                          && r.Timestamp < endTime
                    select r).Count();
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetRecordCount";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }


        public List<Controller_Event_Log> GetSignalEventsByEventCode(string signalId,
            DateTime startTime, DateTime endTime, int eventCode)
        {
            try
            {
                return (from r in _db.Controller_Event_Log
                    where r.SignalID == signalId
                          && r.Timestamp >= startTime
                          && r.Timestamp < endTime
                          && r.EventCode == eventCode
                    select r).ToList();
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetSignalEventsByEventCode";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }

        public List<Controller_Event_Log> GetSignalEventsByEventCodes(string signalId,
            DateTime startTime, DateTime endTime, List<int> eventCodes)
        {
            try
            {
                var events = (from s in _db.Controller_Event_Log
                    where s.SignalID == signalId &&
                          s.Timestamp >= startTime &&
                          s.Timestamp <= endTime &&
                          eventCodes.Contains(s.EventCode)
                    select s).ToList();
                events.Sort((x, y) => DateTime.Compare(x.Timestamp, y.Timestamp));
                return events;
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetSignalEventsByEventCodes";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }

        public List<Controller_Event_Log> GetEventsByEventCodesParam(string signalId, DateTime startTime,
            DateTime endTime, List<int> eventCodes, int param)
        {
            try
            {
                var events = from s in _db.Controller_Event_Log
                    where s.SignalID == signalId &&
                          s.Timestamp >= startTime &&
                          s.Timestamp <= endTime &&
                          s.EventParam == param &&
                          eventCodes.Contains(s.EventCode)
                    select s;
                events = events.OrderBy(e => e.Timestamp);
                return events.ToList();
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetEventsByEventCodesParam";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }

        public List<Controller_Event_Log> GetTopEventsAfterDateByEventCodesParam(string signalId,
            DateTime timestamp, List<int> eventCodes, int param, int top)
        {
            try
            {
                var events = _db.Controller_Event_Log.Where(c =>
                        c.SignalID == signalId &&
                        c.Timestamp > timestamp &&
                        c.EventParam == param &&
                        eventCodes.Contains(c.EventCode))
                    .OrderBy(s => s.Timestamp)
                    .Take(top).ToList();
                return events;
            }
            catch (Exception e)
            {
                var errorLog = ApplicationEventRepositoryFactory.Create();
                errorLog.QuickAdd(Assembly.GetExecutingAssembly().FullName,
                    GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.Low, e.Message);
                return null;
            }
        }


        public int GetEventCountByEventCodesParamDateTimeRange(string signalId,
            DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute,
            List<int> eventCodes, int param)
        {
            try
            {
                return
                (from s in _db.Controller_Event_Log
                    where s.SignalID == signalId &&
                          s.Timestamp >= startTime &&
                          s.Timestamp <= endTime &&
                          (s.Timestamp.Hour > startHour && s.Timestamp.Hour < endHour ||
                           s.Timestamp.Hour == startHour && s.Timestamp.Hour == endHour &&
                           s.Timestamp.Minute >= startMinute && s.Timestamp.Minute <= endMinute ||
                           s.Timestamp.Hour == startHour && s.Timestamp.Hour < endHour &&
                           s.Timestamp.Minute >= startMinute ||
                           s.Timestamp.Hour < startHour && s.Timestamp.Hour == endHour &&
                           s.Timestamp.Minute <= endMinute)
                          &&
                          s.EventParam == param &&
                          eventCodes.Contains(s.EventCode)
                    select s).Count();
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetEventCountByEventCodesParamDateTimeRange";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }


        public List<Controller_Event_Log> GetEventsByEventCodesParamDateTimeRange(string signalId,
            DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute,
            List<int> eventCodes, int param)
        {
            try
            {
                var events = (from s in _db.Controller_Event_Log
                    where s.SignalID == signalId &&
                          s.Timestamp >= startTime &&
                          s.Timestamp <= endTime &&
                          (s.Timestamp.Hour > startHour && s.Timestamp.Hour < endHour ||
                           s.Timestamp.Hour == startHour && s.Timestamp.Hour == endHour &&
                           s.Timestamp.Minute >= startMinute && s.Timestamp.Minute <= endMinute ||
                           s.Timestamp.Hour == startHour && s.Timestamp.Hour < endHour &&
                           s.Timestamp.Minute >= startMinute ||
                           s.Timestamp.Hour < startHour && s.Timestamp.Hour == endHour &&
                           s.Timestamp.Minute <= endMinute)
                          &&
                          s.EventParam == param &&
                          eventCodes.Contains(s.EventCode)
                    select s).ToList();
                events.Sort((x, y) => DateTime.Compare(x.Timestamp, y.Timestamp));
                return events;
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetSignalEventsByEventCodesParamDateTimeRange";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }


        public List<Controller_Event_Log> GetEventsByEventCodesParamWithOffset(string signalId,
            DateTime startTime, DateTime endTime, List<int> eventCodes, int param, double offset,
            double latencyCorrection)
        {
            try
            {
                var events = (from s in _db.Controller_Event_Log
                    where s.SignalID == signalId &&
                          s.Timestamp >= startTime &&
                          s.Timestamp <= endTime &&
                          s.EventParam == param &&
                          eventCodes.Contains(s.EventCode)
                    select s).ToList();
                events.Sort((x, y) => DateTime.Compare(x.Timestamp, y.Timestamp));
                foreach (var cel in events)
                {
                    cel.Timestamp = cel.Timestamp.AddMilliseconds(offset);
                    cel.Timestamp = cel.Timestamp.AddSeconds(0 - latencyCorrection);
                }
                return events;
            }
            catch (Exception ex)
            {
                var logRepository =
                    ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetEventsByEventCodesParamWithOffset";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Timestamp = DateTime.Now;
                e.Description = ex.Message;
                logRepository.Add(e);
                throw;
            }
        }

        public Controller_Event_Log GetFirstEventBeforeDate(string signalId,
            int eventCode, DateTime date)
        {
            try
            {
                var tempDate = date.AddDays(-1);
                var lastEvent = _db.Controller_Event_Log.Where(c => c.SignalID == signalId &&
                                                                    c.Timestamp >= tempDate &&
                                                                    c.Timestamp < date &&
                                                                    c.EventCode == eventCode)
                    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
                return lastEvent;
            }
            catch (Exception ex)
            {
                var logRepository = ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetEventsByEventCodesParamWithOffset";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Description = ex.Message;
                e.Timestamp = DateTime.Now;
                logRepository.Add(e);
                return null;
            }
        }

        public Controller_Event_Log GetFirstEventBeforeDateByEventCodeAndParameter(string signalId, int eventCode,
            int eventParam, DateTime date)
        {
            try
            {
                var tempDate = date.AddDays(-1);
                var lastEvent = _db.Controller_Event_Log.Where(c => c.SignalID == signalId &&
                                                                    c.Timestamp >= tempDate &&
                                                                    c.Timestamp < date &&
                                                                    c.EventCode == eventCode &&
                                                                    c.EventParam == eventParam)
                    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
                return lastEvent;
            }
            catch (Exception ex)
            {
                var logRepository = ApplicationEventRepositoryFactory.Create();
                var e = new ApplicationEvent();
                e.ApplicationName = "MOE.Common";
                e.Class = GetType().ToString();
                e.Function = "GetEventsByEventCodesParamWithOffset";
                e.SeverityLevel = ApplicationEvent.SeverityLevels.High;
                e.Description = ex.Message;
                e.Timestamp = DateTime.Now;
                logRepository.Add(e);
                return null;
            }
        }

        public int GetSignalEventsCountBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            var results = (from r in _db.Controller_Event_Log
                where r.SignalID == signalId && r.Timestamp > startTime && r.Timestamp < endTime
                      && r.EventCode > 130 && r.EventCode < 150
                select r).Count();
            return results;
        }

        public int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime, 
            int phaseNumber)
        {
            var approachCodes = new List<int> {1, 8, 10};
            var ar = ApproachRepositoryFactory.Create();
            Approach approach = ar.GetApproachByApproachID(approachId);

            var results = _db.Controller_Event_Log.Where(r => 
                r.SignalID == approach.SignalID && r.Timestamp > startTime && r.Timestamp < endTime
                && approachCodes.Contains(r.EventCode) && r.EventParam == phaseNumber);

            return results.Count();
        }
    }
}