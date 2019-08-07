using DataAPI.Interfaces;
using DataAPI.Models;
using HtmlAgilityPack;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataAPI.Services
{
    public class StockService
    {
        private readonly IMongoCollection<SNP500> _stock;

        public StockService(IConnectionSetting connection)
        {
            var client = new MongoClient(connection.ConnectionString);
            var database = client.GetDatabase(connection.DatabaseName);
            _stock = database.GetCollection<SNP500>("SNP500");
            Populate().ConfigureAwait(false).GetAwaiter().GetResult();
            
        }

        private async Task Populate()
        {
            HttpClient client = new HttpClient();
            using (var response = await client.GetAsync("https://finance.yahoo.com/quote/%5EGSPC/history?p=%5EGSPC"))
            {
                var pageContents = await response.Content.ReadAsStringAsync();

                HtmlDocument pageRead = new HtmlDocument();
                pageRead.LoadHtml(pageContents);
                if (pageRead.ParseErrors.Count() > 0)
                {
                    //report invalid HTML
                }
                else
                {
                    ExtractData(pageRead);
                }
            }
        }

        private void ExtractData(HtmlDocument pageRead)
        {
            var dataList = pageRead.DocumentNode.SelectNodes("//tr[@class]");
            dataList.RemoveAt(0);
            dataList.RemoveAt(dataList.Count - 1);
            
            foreach (var dailyData in dataList)
            {
                try
                {

                    var temp = _stock.AsQueryable().Any(date => date.TodaysDate == dailyData.ChildNodes[0].InnerText);
                    if (!temp)
                    {
                        _stock.InsertOne(new SNP500
                        {
                            TodaysDate = dailyData.ChildNodes[0].InnerText,
                            OpeningValue = float.Parse(dailyData.ChildNodes[1].InnerText),
                            HighValue = float.Parse(dailyData.ChildNodes[2].InnerText),
                            LowValue = float.Parse(dailyData.ChildNodes[3].InnerText),
                            CloseValue = float.Parse(dailyData.ChildNodes[4].InnerText),
                            AdjClose = float.Parse(dailyData.ChildNodes[5].InnerText),
                            Volume = float.Parse(dailyData.ChildNodes[6].InnerText)

                        });
                    }
                    else
                    {

                        break;
                    }
                }
                catch (Exception ex)
                {
                    //unable to create object
                }
            }
        }

        public List<SNP500> GetList() => _stock.Find(stock => true).ToList();

        public SNP500 GetIndividual(string id) => _stock.Find<SNP500>(stock => stock.Id.Equals(id)).FirstOrDefault();


    }
}
