﻿using HttpMock;
using NUnit.Framework;

namespace Taxjar.Tests
{
	[TestFixture]
	public class TaxesTests
	{
		internal TaxjarApi client;

		[SetUp]
		public void Init()
		{
			this.client = new TaxjarApi("foo123", new { apiUrl = "http://localhost:9191/v2/" });
		}

		[Test]
		public void when_calculating_sales_tax_for_an_order()
		{
			var stubHttp = HttpMockRepository.At("http://localhost:9191");

			stubHttp.Stub(x => x.Post("/v2/taxes"))
					.Return(TaxjarFixture.GetJSON("taxes.json"))
					.OK();

			var rates = client.TaxForOrder(new {
				from_country =  "US",
				from_zip = "07001",
				from_state = "NJ",
				to_country = "US",
				to_zip = "07446",
				to_state = "NJ",
				amount = 16.50,
				shipping = 1.50,
				line_items = new[] {
					new
					{
						quantity = 1,
						unit_price = 15.0,
						product_tax_code = "31000"
					}
				}
			});

			Assert.AreEqual(16.5, rates.OrderTotalAmount);
			Assert.AreEqual(1.5, rates.Shipping);
			Assert.AreEqual(16.5, rates.TaxableAmount);
			Assert.AreEqual(1.16, rates.AmountToCollect);
			Assert.AreEqual(0.07, rates.Rate);
			Assert.AreEqual(true, rates.HasNexus);
			Assert.AreEqual(true, rates.FreightTaxable);
			Assert.AreEqual("destination", rates.TaxSource);

			// Breakdowns
			Assert.AreEqual(1.5, rates.Breakdown.Shipping.TaxableAmount);
			Assert.AreEqual(0.11, rates.Breakdown.Shipping.TaxCollectable);
			Assert.AreEqual(0.07, rates.Breakdown.Shipping.CombinedTaxRate);
			Assert.AreEqual(1.5, rates.Breakdown.Shipping.StateTaxableAmount);
			Assert.AreEqual(0.11, rates.Breakdown.Shipping.StateAmount);
			Assert.AreEqual(0.07, rates.Breakdown.Shipping.StateSalesTaxRate);
			Assert.AreEqual(0, rates.Breakdown.Shipping.CountyTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.Shipping.CountyAmount);
			Assert.AreEqual(0, rates.Breakdown.Shipping.CountyTaxRate);
			Assert.AreEqual(0, rates.Breakdown.Shipping.CityTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.Shipping.CityAmount);
			Assert.AreEqual(0, rates.Breakdown.Shipping.CityTaxRate);
			Assert.AreEqual(0, rates.Breakdown.Shipping.SpecialDistrictTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.Shipping.SpecialDistrictAmount);
			Assert.AreEqual(0, rates.Breakdown.Shipping.SpecialDistrictTaxRate);

			Assert.AreEqual(16.5, rates.Breakdown.TaxableAmount);
			Assert.AreEqual(1.16, rates.Breakdown.TaxCollectable);
			Assert.AreEqual(0.07, rates.Breakdown.CombinedTaxRate);
			Assert.AreEqual(16.5, rates.Breakdown.StateTaxableAmount);
			Assert.AreEqual(0.07, rates.Breakdown.StateTaxRate);
			Assert.AreEqual(1.16, rates.Breakdown.StateTaxCollectable);
			Assert.AreEqual(0, rates.Breakdown.CountyTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.CountyTaxRate);
			Assert.AreEqual(0, rates.Breakdown.CountyTaxCollectable);
			Assert.AreEqual(0, rates.Breakdown.CityTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.CityTaxRate);
			Assert.AreEqual(0, rates.Breakdown.CityTaxCollectable);
			Assert.AreEqual(0, rates.Breakdown.SpecialDistrictTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.SpecialDistrictTaxRate);
			Assert.AreEqual(0, rates.Breakdown.SpecialDistrictTaxCollectable);

			// Line Items
			Assert.AreEqual(1, rates.Breakdown.LineItems[0].Id);
			Assert.AreEqual(15, rates.Breakdown.LineItems[0].TaxableAmount);
			Assert.AreEqual(1.05, rates.Breakdown.LineItems[0].TaxCollectable);
			Assert.AreEqual(0.07, rates.Breakdown.LineItems[0].CombinedTaxRate);
			Assert.AreEqual(15, rates.Breakdown.LineItems[0].StateTaxableAmount);
			Assert.AreEqual(0.07, rates.Breakdown.LineItems[0].StateSalesTaxRate);
			Assert.AreEqual(1.05, rates.Breakdown.LineItems[0].StateAmount);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].CountyTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].CountyTaxRate);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].CountyAmount);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].CityTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].CityTaxRate);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].CityAmount);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].SpecialDistrictTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].SpecialTaxRate);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].SpecialDistrictAmount);
		}

		[Test]
		public void when_calculating_sales_tax_for_an_international_order()
		{
			var stubHttp = HttpMockRepository.At("http://localhost:9191");

			stubHttp.Stub(x => x.Post("/v2/taxes"))
					.Return(TaxjarFixture.GetJSON("taxes_international.json"))
					.OK();

			var rates = client.TaxForOrder(new
			{
				from_country = "FI",
				from_zip = "00150",
				to_country = "FI",
				to_zip = "00150",
				amount = 16.95,
				shipping = 10,
				line_items = new[] {
					new
					{
						quantity = 1,
						unit_price = 16.95
					}
				}
			});

			Assert.AreEqual(26.95, rates.OrderTotalAmount);
			Assert.AreEqual(6.47, rates.AmountToCollect);
			Assert.AreEqual(true, rates.HasNexus);
			Assert.AreEqual(true, rates.FreightTaxable);
			Assert.AreEqual("destination", rates.TaxSource);

			// Breakdowns
			Assert.AreEqual(26.95, rates.Breakdown.TaxableAmount);
			Assert.AreEqual(6.47, rates.Breakdown.TaxCollectable);
			Assert.AreEqual(0.24, rates.Breakdown.CombinedTaxRate);
			Assert.AreEqual(26.95, rates.Breakdown.CountryTaxableAmount);
			Assert.AreEqual(0.24, rates.Breakdown.CountryTaxRate);
			Assert.AreEqual(6.47, rates.Breakdown.CountryTaxCollectable);

			Assert.AreEqual(10, rates.Breakdown.Shipping.TaxableAmount);
			Assert.AreEqual(2.4, rates.Breakdown.Shipping.TaxCollectable);
			Assert.AreEqual(0.24, rates.Breakdown.Shipping.CombinedTaxRate);
			Assert.AreEqual(10, rates.Breakdown.Shipping.CountryTaxableAmount);
			Assert.AreEqual(0.24, rates.Breakdown.Shipping.CountryTaxRate);
			Assert.AreEqual(2.4, rates.Breakdown.Shipping.CountryTaxCollectable);

			// Line Items
			Assert.AreEqual(16.95, rates.Breakdown.LineItems[0].TaxableAmount);
			Assert.AreEqual(4.07, rates.Breakdown.LineItems[0].TaxCollectable);
			Assert.AreEqual(0.24, rates.Breakdown.LineItems[0].CombinedTaxRate);
			Assert.AreEqual(16.95, rates.Breakdown.LineItems[0].CountryTaxableAmount);
			Assert.AreEqual(0.24, rates.Breakdown.LineItems[0].CountryTaxRate);
			Assert.AreEqual(4.07, rates.Breakdown.LineItems[0].CountryTaxCollectable);
		}

		[Test]
		public void when_calculating_sales_tax_for_a_canadian_order()
		{
			var stubHttp = HttpMockRepository.At("http://localhost:9191");

			stubHttp.Stub(x => x.Post("/v2/taxes"))
					.Return(TaxjarFixture.GetJSON("taxes_canada.json"))
					.OK();

			var rates = client.TaxForOrder(new
			{
				from_country = "CA",
				from_zip = "V6G 3E",
				from_state = "BC",
				to_country = "CA",
				to_zip = "M5V 2T6",
				to_state = "ON",
				amount = 16.95,
				shipping = 10,
				line_items = new[] {
					new
					{
						quantity = 1,
						unit_price = 16.95
					}
				}
			});

			Assert.AreEqual(26.95, rates.OrderTotalAmount);
			Assert.AreEqual(10, rates.Shipping);
			Assert.AreEqual(26.95, rates.TaxableAmount);
			Assert.AreEqual(3.5, rates.AmountToCollect);
			Assert.AreEqual(0.13, rates.Rate);
			Assert.AreEqual(true, rates.HasNexus);
			Assert.AreEqual(true, rates.FreightTaxable);
			Assert.AreEqual("destination", rates.TaxSource);

			// Breakdowns
			Assert.AreEqual(26.95, rates.Breakdown.TaxableAmount);
			Assert.AreEqual(3.5, rates.Breakdown.TaxCollectable);
			Assert.AreEqual(0.13, rates.Breakdown.CombinedTaxRate);
			Assert.AreEqual(26.95, rates.Breakdown.GSTTaxableAmount);
			Assert.AreEqual(0.05, rates.Breakdown.GSTTaxRate);
			Assert.AreEqual(1.35, rates.Breakdown.GST);
			Assert.AreEqual(26.95, rates.Breakdown.PSTTaxableAmount);
			Assert.AreEqual(0.08, rates.Breakdown.PSTTaxRate);
			Assert.AreEqual(2.16, rates.Breakdown.PST);
			Assert.AreEqual(0, rates.Breakdown.QSTTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.QSTTaxRate);
			Assert.AreEqual(0, rates.Breakdown.QST);

			Assert.AreEqual(10, rates.Breakdown.Shipping.TaxableAmount);
			Assert.AreEqual(1.3, rates.Breakdown.Shipping.TaxCollectable);
			Assert.AreEqual(0.13, rates.Breakdown.Shipping.CombinedTaxRate);
			Assert.AreEqual(10, rates.Breakdown.Shipping.GSTTaxableAmount);
			Assert.AreEqual(0.05, rates.Breakdown.Shipping.GSTTaxRate);
			Assert.AreEqual(0.5, rates.Breakdown.Shipping.GST);
			Assert.AreEqual(10, rates.Breakdown.Shipping.PSTTaxableAmount);
			Assert.AreEqual(0.08, rates.Breakdown.Shipping.PSTTaxRate);
			Assert.AreEqual(0.8, rates.Breakdown.Shipping.PST);
			Assert.AreEqual(0, rates.Breakdown.Shipping.QSTTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.Shipping.QSTTaxRate);
			Assert.AreEqual(0, rates.Breakdown.Shipping.QST);

			// Line Items
			Assert.AreEqual(16.95, rates.Breakdown.LineItems[0].TaxableAmount);
			Assert.AreEqual(2.2, rates.Breakdown.LineItems[0].TaxCollectable);
			Assert.AreEqual(0.13, rates.Breakdown.LineItems[0].CombinedTaxRate);
			Assert.AreEqual(16.95, rates.Breakdown.LineItems[0].GSTTaxableAmount);
			Assert.AreEqual(0.05, rates.Breakdown.LineItems[0].GSTTaxRate);
			Assert.AreEqual(0.85, rates.Breakdown.LineItems[0].GST);
			Assert.AreEqual(16.95, rates.Breakdown.LineItems[0].PSTTaxableAmount);
			Assert.AreEqual(0.08, rates.Breakdown.LineItems[0].PSTTaxRate);
			Assert.AreEqual(1.36, rates.Breakdown.LineItems[0].PST);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].QSTTaxableAmount);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].QSTTaxRate);
			Assert.AreEqual(0, rates.Breakdown.LineItems[0].QST);
		}
	}
}