﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOE.Common.Models.Repositories;
using MOE.CommonTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Common.Models.Repositories.Tests
{
    [TestClass()]
    public class SignalsRepositoryTests
    {
        public static CommonTests.Models.InMemoryMOEDatabase _db = new InMemoryMOEDatabase();
        ISignalsRepository SR = new CommonTests.Models.InMemorySignalsRepository(_db);
        

        [TestMethod()]
        public void GetAllSignalsTest()
        {
            Common.Models.Signal s = new Signal();

            SR.AddOrUpdate(s);

            var signals = SR.GetAllSignals();

            Assert.IsNotNull(signals);

            Assert.IsTrue(signals.Count > 0);

        }

        [TestMethod()]
        public void EagerLoadAllSignalsTest()
        {
            Common.Models.Signal s = new Signal();

            SR.AddOrUpdate(s);

            var signals = SR.EagerLoadAllSignals();

            Assert.IsNotNull(signals);

            Assert.IsTrue(signals.Count > 0);
        }

        [TestMethod()]
        public void GetAllEnabledSignalsTest()
        {
            Common.Models.Signal s = new Signal();

            SR.AddOrUpdate(s);

            var signals = SR.GetAllEnabledSignals();

            Assert.IsNotNull(signals);

            Assert.IsTrue(signals.Count > 0);
        }

        [TestMethod()]
        public void GetSignalLocationTest()
        {
            Common.Models.Signal s = new Signal();

            s.SignalID = "10001";
            s.PrimaryName = "PrimaryTestStreet";
            s.SecondaryName = "SecondaryTestStreet";

            SR.AddOrUpdate(s);

            string locationString = SR.GetSignalLocation("10001");


            Assert.IsTrue(locationString.Length > 0);
        }

   

        [TestMethod()]
        public void CheckReportAvialabilityForSignalTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSignalBySignalIDTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DoesSignalHaveDetectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest()
        {

            var s = CreateSignalForTest();

            SR.AddOrUpdate(s);

            string locationString = SR.GetSignalLocation("10001");

            s.PrimaryName = "UpdatedPrimaryTestStreet";
            s.SecondaryName = "UpdateSecondaryTestStreet";

            SR.AddOrUpdate(s);

            Assert.IsTrue(s.PrimaryName == "UpdatedPrimaryTestStreet");

        }

        [TestMethod()]
        public void GetPinInfoTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddOrUpdateTest()
        {
            var s = CreateSignalForTest();

            SR.AddOrUpdate(s);

            string locationString = SR.GetSignalLocation("10001");

            s.PrimaryName = "UpdatedPrimaryTestStreet";
            s.SecondaryName = "UpdateSecondaryTestStreet";

            SR.AddOrUpdate(s);

            var x = SR.GetSignalBySignalID("10001");

            Assert.IsTrue(x.PrimaryName == "UpdatedPrimaryTestStreet");
        }
        [TestMethod()]
        public void GetVersionOfSignalByDateTest()
        {
            AddMultipleVersionsOfMultipleSignalsForTest();

            var version = SR.GetVersionOfSignalByDate("10001", DateTime.Today.AddDays(-1));

            Assert.IsNotNull(version);

            Assert.IsTrue(version.Start <= DateTime.Today.AddDays(-1));



        }


        [TestMethod()]
        public void GetVersionOfSignalByDate_FutureDateGetsLaterVersionTest()
        {
            AddMultipleVersionsOfMultipleSignalsForTest();

            var version = SR.GetVersionOfSignalByDate("10001", DateTime.Today.AddDays(1));

            Assert.IsNotNull(version);

            Assert.IsTrue(version.Start >= DateTime.Today.AddDays(-1));



        }




        [TestMethod()]
        public void UpdateWithNewVersionTest()
        {
            var origSignal = CreateSignalForTest();
            SR.AddOrUpdate(origSignal);

            var alteredSignal = SR.CopySignalToNewVersion(origSignal);

            alteredSignal.PrimaryName = "alteredPrimaryName";
            alteredSignal.SecondaryName = "alteredSecondaryName";

            SR.AddOrUpdate(alteredSignal);

            var signals = SR.GetAllVersionsOfSignalBySignalID("10001");

            Assert.AreNotEqual(signals[0].PrimaryName, signals[1].PrimaryName);
        }

        [TestMethod()]
        public void GetLatestVersionOfAllSignalsTest()
        {


            AddMultipleVersionsOfMultipleSignalsForTest();

            

            List<Signal> latestVersionsfromDB = SR.GetLatestVersionOfAllSignals();

            Assert.IsTrue(latestVersionsfromDB.Count == 3);

            var areThereDuplicates = from r in latestVersionsfromDB
                                     where r.SignalID == "10001"
                                     select r;

            Assert.IsTrue(areThereDuplicates.Count() == 1);

            var areRecordsCurrent = from r in latestVersionsfromDB
                                    where r.Start == DateTime.Today
                                    select r;


            Assert.IsTrue(areRecordsCurrent.Count() == 3);

        }

        [TestMethod()]
        public void AddListTest()
        {
            var s1 = CreateSignalForTest();
            var s2 = CreateSignalForTest();
            var s3 = CreateSignalForTest();

            s2.SignalID = "10002";
            s3.SignalID = "10003";



            List<Signal> signals = new List<Signal>();
            signals.Add(s1);
            signals.Add(s2);
            signals.Add(s3);

            SR.AddList(signals);

            var retrievedSignals = SR.GetAllSignals();

            Assert.IsTrue(retrievedSignals.Contains(s2));

        }



        [TestMethod()]
        public void GetSignalFTPInfoByIDTest()
        {
            Assert.Inconclusive();
        }

        private Common.Models.Signal CreateSignalForTest()
        {
            Common.Models.Signal s = new Signal();

            

            s.SignalID = "10001";
            s.PrimaryName = "PrimaryTestStreet";
            s.SecondaryName = "SecondaryTestStreet";
            s.Start = s.FirstDate;

            return s;

        }

        private void AddMultipleVersionsOfMultipleSignalsForTest()
        {
            List<Signal> signals = CreateSignalListForTest();


            SR.AddList(signals);

            List<Signal> newVersionofSignals = CreateSignalListForTest();

            foreach (var s in newVersionofSignals)
            {
                s.VersionID = s.VersionID + 3;
                s.Start = DateTime.Today;
            }

            SR.AddList(newVersionofSignals);
        }

        private List<Common.Models.Signal> CreateSignalListForTest()
        {

            var s1 = CreateSignalForTest();
            var s2 = CreateSignalForTest();
            var s3 = CreateSignalForTest();

            s2.SignalID = "10002";
            s3.SignalID = "10003";
            s1.VersionID = 0;
            s2.VersionID = 1;
            s3.VersionID = 2;
            s1.Start = s1.FirstDate;
            s2.Start = s1.FirstDate;
            s3.Start = s1.FirstDate;


            List<Signal> signals = new List<Signal>();
            signals.Add(s1);
            signals.Add(s2);
            signals.Add(s3);

            return signals;

        }

    }
}