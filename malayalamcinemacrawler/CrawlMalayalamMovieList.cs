using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace malayalamcinemacrawler
{
    class MalayalamMovieDetail
    {
        public string _dateOfRelease;
        public string _movieName;
        public ArrayList _producers;
        public ArrayList _casts;
        public ArrayList _musicDirectors;
        public ArrayList _cinematographers;
        public ArrayList _screenPlayWriters;
        public ArrayList _Editors;
    }
    class CrawlMalayalamMovieList
    {


        private async Task<string> GetStreamAsync(string url)
        {
            WebRequest wr = WebRequest.Create(url);
            wr.Method = "GET";
            System.Threading.Tasks.Task<WebResponse> response = wr.GetResponseAsync();
            WebResponse rr = await response;
            System.IO.Stream dataStream = rr.GetResponseStream();

            StreamReader streamReader = new StreamReader(dataStream);
            string html = streamReader.ReadToEnd();
            return html;
        }

        public bool ParsePage(string pageContent, ref ArrayList id)
        {
            bool hasNextPage = pageContent.IndexOf("Next &gt;&gt;") >= 0;
            Regex rx = new Regex("\"(gallery_.*htm)");
            MatchCollection matchcollection;

            matchcollection = rx.Matches(pageContent);
            foreach (var match in matchcollection)
            {
                string s = ((Match)match).Value;
                id.Add(s);
                Console.WriteLine(s);
                
            }
            
            
            return hasNextPage;
        }

        private async Task<MalayalamMovieDetail> ParseMovieDetail(string id)
        {
            string starPage = "http://www.malayalamcinema.com/";
            Task<string> t = GetStreamAsync(starPage + id);
            string html = await t;
            Regex title = new Regex("<td valign=\\\"middle\\\" id=\\\"c-bg\\\"><span class=\\\"head\\\">(.*)<\\/span><\\/td>");
            Regex yearOfRelease = new Regex("<td.*width=\\\"299\\\">(.*)<\\/td>");
           
             MalayalamMovieDetail mmd = new MalayalamMovieDetail();
            var match = title.Match(html);

            if (match.Success)
            {
                mmd._movieName = match.Groups[1].Value;
            }
            match = yearOfRelease.Match(html);
            if (match.Success)
            {
                mmd._dateOfRelease = match.Groups[1].Value;
            }

            return mmd;
        }

        private static async Task<ArrayList> GetMovieDetails(ArrayList idList)
        {
            ArrayList startDetailTasks = new ArrayList();
            ArrayList startDetails = new ArrayList();


            foreach (var id in idList)
            {
                CrawlMalayalamMovieList cm = new CrawlMalayalamMovieList();
                Task<MalayalamMovieDetail> startDetailTask = cm.ParseMovieDetail(id.ToString());
                startDetailTasks.Add(startDetailTask);
            }

            foreach (var task in startDetailTasks)
            {
                Task<MalayalamMovieDetail> taskStar = (Task<MalayalamMovieDetail>)task;
                MalayalamMovieDetail star = await taskStar;
                startDetails.Add(star);

            }
            return startDetails;
        }

        public static async Task<ArrayList> GetMovieList()
        {
            CrawlMalayalamMovieList c = new CrawlMalayalamMovieList();
            string baseUrl = "http://www.malayalamcinema.com/filmList.php?pageID=";
            int pageId = 1;
            const int size = 862;
            bool baseHasNextPage = false;
            Task[] taskList = new Task[size];
            int count = 0;

            ArrayList idList = new ArrayList();
            do
            {
                Console.WriteLine("Page {0}", pageId);

                Task<string> t = c.GetStreamAsync(baseUrl + pageId.ToString());
                taskList[pageId - 1] = t;
                pageId++;
                t.ContinueWith(result => { c.ParsePage(result.Result, ref idList); count++; });
                
                //taskList.Add(t);
                
                //string result = await t;
                //c.ParsePage(result, ref idList);

            }
            while (pageId <= size);
            //foreach(var t in taskList)
            //{
            //    var p = await (Task<string>)t;
            //}
            Task.WaitAll(taskList);
            while (count != size) ;
        
            return idList;


        }
        public static void Crawl()
        {

            //Task<ArrayList> idList = GetMovieList();
            //idList.Wait();
            //foreach(var id in idList.Result)
            //{
            //    Console.WriteLine("{0}", id);
            //}
            ArrayList idlist = new ArrayList();
            idlist.Add("gallery_bangalore-days.htm");
            var t =GetMovieDetails(idlist);
            t.Wait();
            Console.WriteLine("Finished Movies");
            //throw new Exception("What is htis:");




        }
    }


}
