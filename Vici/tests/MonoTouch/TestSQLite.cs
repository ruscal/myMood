using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Vici.CoolStorage.UnitTests
{
    [TestFixture]
    public class TestSQLite : CommonTests
    {
        [TestFixtureSetUp]
        public void SetupServer()
        {
            string path = "coolstorage.sqlite";

			File.Delete(path);
			
			CSConfig.SetDB(path,SqliteOption.CreateIfNotExists|SqliteOption.UseConnectionPooling, () => {

			CSDatabase.ExecuteNonQuery(
                "CREATE TABLE tblCustomers (CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT(50) NOT NULL)");

            CSDatabase.ExecuteNonQuery(
                @"CREATE INDEX tblCustomers_Name ON tblCustomers (Name)");

            CSDatabase.ExecuteNonQuery(
                @"CREATE TABLE tblCustomerPaymentMethodLinks (
            	                CustomerID integer NOT NULL,
            	                PaymentMethodID integer NOT NULL,
                                primary key (CustomerID,PaymentMethodID)
                                )");

            

            CSDatabase.ExecuteNonQuery(
@"CREATE TABLE tblOrderItems (
            	OrderItemID INTEGER PRIMARY KEY AUTOINCREMENT,
            	OrderID integer NOT NULL,
            	Qty integer NOT NULL,
            	Price real NOT NULL,
            	Description TEXT(200) NOT NULL
                )
            ");

            CSDatabase.ExecuteNonQuery(
                @"CREATE INDEX tblOrderItems_OrderID ON tblOrderItems (OrderID)");

            CSDatabase.ExecuteNonQuery(
@"CREATE TABLE tblOrders (
            	OrderID INTEGER PRIMARY KEY AUTOINCREMENT,
            	Date TEXT(30) NOT NULL DEFAULT CURRENT_TIMESTAMP,
            	CustomerID integer NOT NULL,
            	SalesPersonID integer NULL,
            	DataState text(50))");

            CSDatabase.ExecuteNonQuery(
    @"CREATE INDEX tblOrders_CustomerID ON tblOrders (CustomerID)");

            CSDatabase.ExecuteNonQuery(
@"CREATE INDEX tblOrders_SalesPersonID ON tblOrders (SalesPersonID)");

            CSDatabase.ExecuteNonQuery(
                @"CREATE TABLE tblPaymentMethods (
            	PaymentMethodID integer primary key autoincrement,
            	Name text(50) NOT NULL,
            	MonthlyCost integer NOT NULL
             )");

            CSDatabase.ExecuteNonQuery(
@"CREATE TABLE tblSalesPeople (
            	SalesPersonID integer primary key autoincrement,
            	Name text(50) NOT NULL,
            	SalesPersonType integer NULL)
             ");

            CSDatabase.ExecuteNonQuery(
@"CREATE TABLE tblCoolData (
            	CoolDataID text(50) PRIMARY KEY,
            	Name text(50) NULL)");
			});

        }
    }
}
