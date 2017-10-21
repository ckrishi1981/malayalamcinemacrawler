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
        public ArrayList _casts;
        public ArrayList _musicDirectors;
        public ArrayList _lyrics;
        public ArrayList _cinematographers;
        public ArrayList _screenPlayWriters;
        public ArrayList _Editors;
        public ArrayList _thumbNailImages;
        public ArrayList _previewImages;
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

        private  string GetSubString(string html, string startIndexString, string endIndexString )
        {
            int index = html.IndexOf(startIndexString);
            if (index < 0) return String.Empty;
            int endIndex = html.IndexOf(endIndexString, index);
            string str = html.Substring(index, endIndex - index);
            str = str.Replace("\n", "");
            str = str.Replace("\r", "");
            return str;
        }

        private ArrayList GetPeopleFromSubString(string subString, Regex regex)
        {
            var match = regex.Match(subString);
            if (match.Success)
            {
                string actors = match.Groups[1].Value;
                string[] split = { "<br />" };
                ArrayList modifiedList = new ArrayList();
                ArrayList castList = new ArrayList();
                castList.AddRange(actors.Split(split, StringSplitOptions.RemoveEmptyEntries));

                if (actors.Contains(","))
                {
                    foreach (string actor in castList)
                    {
                        char[] charArray = { ',' };
                        modifiedList.AddRange(actor.Split(charArray));

                    }
                }
                else
                {
                    modifiedList.AddRange(castList);
                }
                return modifiedList;
            }
            return null;
        }

        private ArrayList GetCast(string html)
        {
            
            Regex cast = new Regex("<td class=\"normaltext\" valign=\"top\">(.*)</td>");
            string str = GetSubString(html,
                "<td class=\"subpageheads\" valign=\"top\"><strong>Cast</strong></td>", 
                "</tr>");
            return GetPeopleFromSubString(str, cast);
        }

        private ArrayList GetProducer(string html)
        {
            Regex cast = new Regex("<td class=\"normaltext\">(.*)</td>");
            string str = GetSubString(html, 
                "<td class=\"subpageheads\"><strong>Producer</strong></td>", 
                "</tr>");
            var match = cast.Match(str);
            return GetPeopleFromSubString(str, cast) ; 
        }

        private ArrayList GetLyrics(string html)
        {
            Regex cast = new Regex("<td class=\"normaltext\">(.*)</td>");
            string str = GetSubString(html,
                "<td class=\"subpageheads\"><strong>Lyrics</strong></td>",
                "</tr>");
            var match = cast.Match(str);
            return GetPeopleFromSubString(str, cast);
        }


        private ArrayList GetGenericsData(string html, string typeOfJob)
        {
            Regex cast = new Regex("<td class=\"normaltext\">(.*)</td>");
            string str = GetSubString(html,
                "<td class=\"subpageheads\"><strong>"+ typeOfJob +"</strong></td>",
                "</tr>");

            if (String.IsNullOrEmpty(str))
            {
                str = GetSubString(html,
                "<td ><strong>" + typeOfJob + "</strong></td>",
                "</tr>");
            }
            str = str.Replace("<td>:</td>", "");
            var match = cast.Match(str);
            if (!match.Success)
            {
                cast = new Regex("<td>(.*)</td>");
                match = cast.Match(str);
            }
            return GetPeopleFromSubString(str, cast);
        }
        private async Task<MalayalamMovieDetail> ParseMovieDetail(string id)
        {

            MalayalamMovieDetail mmd = new MalayalamMovieDetail();
            try
            {
                string starPage = "http://www.malayalamcinema.com/";
                Task<string> t = GetStreamAsync(starPage + id);
                string html = await t;
                Regex title = new Regex("<td valign=\\\"middle\\\" id=\\\"c-bg\\\"><span class=\\\"head\\\">(.*)<\\/span><\\/td>");
                Regex yearOfRelease = new Regex("<td.*width=\\\"299\\\">(.*)<\\/td>");

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
                ArrayList director = GetGenericsData(html, "Director");
                if (director != null )
                {
                    mmd._director = (string) director[0];
                }


                mmd._producers = GetProducer(html);
                mmd._casts = GetCast(html);

                mmd._lyrics = GetGenericsData(html, "Lyrics");
                mmd._musicDirectors = GetGenericsData(html, "Music");
                mmd._Editors = GetGenericsData(html, "Editing");
                mmd._cinematographers = GetGenericsData(html, "Cinematography");
                mmd._screenPlayWriters = GetGenericsData(html, "Story/Writer");
                //
                //end of directorC:\Users\ck_ri\projects\malayalamcinemacrawler\malayalamcinemacrawler\CrawlMalayalamMovieList.cs
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

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
            const int size = 863;
            bool baseHasNextPage = false;
            Task[] taskList = new Task[size];
            int count = 0;

            ArrayList idList = new ArrayList();
            do
            {
           
                Task<string> t = c.GetStreamAsync(baseUrl + pageId.ToString());
                taskList[pageId - 1] = t;
                pageId++;

            }
            while (pageId <= size);
         
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
            //var v = c.ParseMovieDetail("gallery_premam.htm");
            var v = c.ParseMovieDetail("gallery_money-ratnam.htm");

            v.Wait();
            return;

            Task<ArrayList> idlist = GetMovieList();
            idlist.Wait();

            Task<ArrayList> movieDetails =GetMovieDetails(idlist.Result);
            movieDetails.Wait();

            foreach(MalayalamMovieDetail movieDetail in movieDetails.Result)
            {
                if (String.IsNullOrEmpty(movieDetail._movieName))
                {
                    continue;
                }
                Console.WriteLine("Name {0}", movieDetail._movieName);
                if (!String.IsNullOrEmpty(movieDetail._director))
                {
                    Console.WriteLine("Director {0}", movieDetail._director); ;
                }
                if (movieDetail._casts != null)
                {
                    foreach (var cast in movieDetail._casts)
                    {
                        Console.WriteLine("Actor {0}", cast);
                    }
                }
                
            }

      
        }
    }


}
