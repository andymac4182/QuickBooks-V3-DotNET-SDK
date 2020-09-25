﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intuit.Ipp.Data;
using Intuit.Ipp.Core;
using Intuit.Ipp.EntitlementService.Test.Common;

namespace Intuit.Ipp.EntitlementService.Test
{
    [TestClass]
    public class EntitlementServiceTest
    {
        private static EntitlementService entitlementServiceTestCases;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            ServiceContext context = Initializer.InitializeServiceContextQbo(); 
            entitlementServiceTestCases = new EntitlementService(context);

        }

        [TestMethod]
        public void GetEntitlementsTest()
        {
            try
            {
                EntitlementsResponse entitlements = entitlementServiceTestCases.GetEntitlements("https://sandbox-quickbooks.api.intuit.com/manage");
                Assert.IsNotNull(entitlements);

            }
            catch (SystemException ex)
            {
                Assert.Fail(ex.ToString());
            }
        }
    }
}
