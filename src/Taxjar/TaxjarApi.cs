﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Reflection;
using Taxjar;
using Newtonsoft.Json;
using RestSharp;

namespace Taxjar
{
	public static class TaxjarConstants
	{
		public const string ApiUrl = "https://api.taxjar.com/v2/";
	}

	public class TaxjarApi
	{
		internal readonly string apiKey;
		internal readonly string apiUrl;

		public TaxjarApi(string apiKey = "", object parameters = null)
		{
			this.apiKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : ConfigurationManager.AppSettings["TaxjarApiKey"];
			this.apiUrl = TaxjarConstants.ApiUrl;

			if (parameters.GetType().GetProperty("apiUrl") != null)
			{
				this.apiUrl = parameters.GetType().GetProperty("apiUrl").ToString();
			}

			if (string.IsNullOrWhiteSpace(this.apiKey))
			{
				throw new ArgumentException("Please provide a TaxJar API key.", "apiKey");
			}
		}

		protected virtual RestClient CreateClient()
		{
			var client = new RestClient(apiUrl);
			return client;
		}

		protected virtual IRestRequest CreateRequest(string action, Method method = Method.POST)
		{
			var request = new RestRequest(action, method) {
				RequestFormat = DataFormat.Json
			};
			request.AddHeader("Authorization", "Bearer " + this.apiKey);
			return request;
		}

		public virtual string SendRequest(string endpoint, object body, Method httpMethod = Method.POST)
		{
			var client = CreateClient();
			var req = CreateRequest(endpoint, httpMethod).AddJsonBody(body);

			if (httpMethod == Method.GET)
			{
				foreach (var prop in body.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					req.AddQueryParameter(prop.Name, prop.GetValue(body, null).ToString());
				}
			}

			var res = client.Execute(req);

			if (res.StatusCode != HttpStatusCode.OK)
			{
				throw new TaxjarException(res.StatusCode, res.StatusDescription, res.Content);
			}

			return res.Content;
		}

		public virtual List<Category> Categories()
		{
			var res = SendRequest("categories", null, Method.GET);
			var categoryRequest = JsonConvert.DeserializeObject<CategoriesRequest>(res);
			return categoryRequest.Categories;
		}

		public virtual Rate RatesForLocation(string zip, object parameters)
		{
			var res = SendRequest("rates/" + zip, parameters, Method.GET);
			var rateRequest = JsonConvert.DeserializeObject<RateRequest>(res);
			return rateRequest.Rate;
		}

		public virtual Tax TaxForOrder(object parameters)
		{
			var res = SendRequest("taxes", parameters, Method.POST);
			var taxRequest = JsonConvert.DeserializeObject<TaxRequest>(res);
			return taxRequest.Tax;
		}

		public virtual List<String> ListOrders(object parameters)
		{
			var res = SendRequest("transactions/orders", parameters, Method.GET);
			var ordersRequest = JsonConvert.DeserializeObject<OrdersRequest>(res);
			return ordersRequest.Orders;
		}

		public virtual Order ShowOrder(int transactionId)
		{
			var res = SendRequest("transactions/orders/" + transactionId, Method.GET);
			var orderRequest = JsonConvert.DeserializeObject<OrderRequest>(res);
			return orderRequest.Order;
		}

		public virtual Order CreateOrder(object parameters)
		{
			var res = SendRequest("transactions/orders", parameters, Method.POST);
			var orderRequest = JsonConvert.DeserializeObject<OrderRequest>(res);
			return orderRequest.Order;
		}

		public virtual Order UpdateOrder(object parameters)
		{
			var res = SendRequest("transactions/orders", parameters, Method.PUT);
			var orderRequest = JsonConvert.DeserializeObject<OrderRequest>(res);
			return orderRequest.Order;
		}

		public virtual Order DeleteOrder(int transactionId)
		{
			var res = SendRequest("transactions/orders/" + transactionId, Method.DELETE);
			var orderRequest = JsonConvert.DeserializeObject<OrderRequest>(res);
			return orderRequest.Order;
		}

		public virtual List<String> ListRefunds(object parameters)
		{
			var res = SendRequest("transactions/orders", parameters, Method.GET);
			var refundsRequest = JsonConvert.DeserializeObject<RefundsRequest>(res);
			return refundsRequest.Refunds;
		}

		public virtual Refund ShowRefund(int transactionId)
		{
			var res = SendRequest("transactions/refunds/" + transactionId, Method.GET);
			var refundRequest = JsonConvert.DeserializeObject<RefundRequest>(res);
			return refundRequest.Refund;
		}

		public virtual Refund CreateRefund(object parameters)
		{
			var res = SendRequest("transactions/refunds", parameters, Method.POST);
			var refundRequest = JsonConvert.DeserializeObject<RefundRequest>(res);
			return refundRequest.Refund;
		}

		public virtual Refund UpdateRefund(object parameters)
		{
			var res = SendRequest("transactions/refunds", parameters, Method.PUT);
			var refundRequest = JsonConvert.DeserializeObject<RefundRequest>(res);
			return refundRequest.Refund;
		}

		public virtual Refund DeleteRefund(int transactionId)
		{
			var res = SendRequest("transactions/refunds/" + transactionId, Method.DELETE);
			var refundRequest = JsonConvert.DeserializeObject<RefundRequest>(res);
			return refundRequest.Refund;
		}

		public virtual List<NexusRegion> NexusRegions()
		{
			var res = SendRequest("nexus/regions", null, Method.GET);
			var nexusRegionsRequest = JsonConvert.DeserializeObject<NexusRegionsRequest>(res);
			return nexusRegionsRequest.Regions;
		}

		public virtual Validation Validate(object parameters)
		{
			var res = SendRequest("validation", parameters, Method.GET);
			var validationRequest = JsonConvert.DeserializeObject<ValidationRequest>(res);
			return validationRequest.Validation;
		}

		public virtual List<SummaryRate> SummaryRates()
		{
			var res = SendRequest("summary_rates", null, Method.GET);
			var summaryRatesRequest = JsonConvert.DeserializeObject<SummaryRatesRequest>(res);
			return summaryRatesRequest.SummaryRates;
		}
	}
}
