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
        public string _director;
        public string _movieName;
        public ArrayList _producers;
        public string[] _casts;
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
            Regex rx = new Regex("(gallery_.*htm)");
            MatchCollection matchcollection;

            matchcollection = rx.Matches(pageContent);
            foreach (var match in matchcollection)
            {
                string s = ((Match)match).Value;
                id.Add(s);
               // Console.WriteLine(s);
                
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
                mmd._movieName = match.Groups[1].Value.Trim();
            }
            match = yearOfRelease.Match(html);
            if (match.Success)
            {
                mmd._dateOfRelease = match.Groups[1].Value.Trim();
            }

            //director

            int index = html.IndexOf("<td class=\"subpageheads\"><strong>Director</strong></td>");
            int endIndex = html.IndexOf("</tr>", index);
            string str = html.Substring(index, endIndex - index);
            //<td class=\"normaltext\">
            Regex directors = new Regex("<td class=\"normaltext\">(.*)</td>");
            match = directors.Match(html);
            if (match.Success)
            {
                mmd._director = match.Groups[1].Value.Trim();
            }

            //<td class="normaltext" valign="top">
            index = html.IndexOf("<td class=\"subpageheads\" valign=\"top\"><strong>Cast</strong></td>");
            endIndex = html.IndexOf("</tr>", index);
            str = html.Substring(index, endIndex - index);
            str = str.Replace("\n", "");
            str = str.Replace("\r", "");
            Regex cast = new Regex("<td class=\"normaltext\" valign=\"top\">(.*)</td>");
            match = cast.Match(str);
            if (match.Success)
            {
                string actors = match.Groups[1].Value;
                string[] split = { "<br />" };
                mmd._casts =  actors.Split(split, StringSplitOptions.RemoveEmptyEntries);
            }

            //end of directorC:\Users\ck_ri\projects\malayalamcinemacrawler\malayalamcinemacrawler\CrawlMalayalamMovieList.cs


            return mmd;
        }

        private static async Task<ArrayList> GetMovieDetails(ArrayList idList)
        {
            Task<MalayalamMovieDetail>[] movieDetails = new Task<MalayalamMovieDetail>[idList.Count];
            ArrayList startDetailTasks = new ArrayList();
            ArrayList startDetails = new ArrayList();

            int count = 0;
            foreach (var id in idList)
            {
                CrawlMalayalamMovieList cm = new CrawlMalayalamMovieList();
                Task<MalayalamMovieDetail> startDetailTask = cm.ParseMovieDetail(id.ToString());
                //startDetailTasks.Add(startDetailTask);
                movieDetails[count] = startDetailTask;
                count++;
            }
            Task.WaitAll(movieDetails);

            foreach (Task<MalayalamMovieDetail> movieDetail in movieDetails)
            {
     
                //MalayalamMovieDetail star = await taskStar;
                startDetails.Add(movieDetail.Result);

            }
            return startDetails;
        }

        public static async Task<ArrayList> GetMovieList()
        {
            CrawlMalayalamMovieList c = new CrawlMalayalamMovieList();
            string baseUrl = "http://www.malayalamcinema.com/filmList.php?pageID=";
            int pageId = 1;
            const int size = 5;
            bool baseHasNextPage = false;
            Task[] taskList = new Task[size];
            int count = 0;

            ArrayList idList = new ArrayList();
            do
            {
            //    Console.WriteLine("Page {0}", pageId);

                Task<string> t = c.GetStreamAsync(baseUrl + pageId.ToString());
                taskList[pageId - 1] = t;
                pageId++;
               
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
            foreach(var task in taskList)
            {
                Task<string> t = (Task<string>)task;
               
                 c.ParsePage(t.Result, ref idList);
                //Console.WriteLine("Page {0}", count);
                count++;

            }

            return idList;


        }
        public static void Crawl()
        {
            CrawlMalayalamMovieList c = new CrawlMalayalamMovieList();
            var v = c.ParseMovieDetail("gallery_lechmi.htm");

            v.Wait();
            return;

            Task<ArrayList> idlist = GetMovieList();
            idlist.Wait();

            Task<ArrayList> movieDetails =GetMovieDetails(idlist.Result);
            movieDetails.Wait();

            foreach(MalayalamMovieDetail movieDetail in movieDetails.Result)
            {
                Console.WriteLine(movieDetail._movieName);
            }

      
        }
    }


}
