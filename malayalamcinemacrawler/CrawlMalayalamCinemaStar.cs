using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace malayalamcinemacrawler
{
    class MalayalamCinemaStar
    {
       public string _name;
        public string _blurp;
        public ArrayList _pictures = new ArrayList();
    }
    class CrawlMalayalamCinemaStar
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
            int lookUpindex  = 0;
            string table;
            bool hasNextPage = pageContent.IndexOf("Next &gt;&gt;") >= 0;
            Regex rx = new Regex(".*star-details\\.php\\?member_id=(\\d+).*");
           
 
            while (   (lookUpindex = pageContent.IndexOf("newsbrdr", lookUpindex)) >= 0)
            {
                int endIndex = 0;
                endIndex = pageContent.IndexOf("</table>", lookUpindex);
      
                table = pageContent.Substring(lookUpindex, endIndex- lookUpindex);
                lookUpindex = endIndex;
               
                table.Replace("<td width=\"19 % \" rowspan=\"2\" valign=\"top\" >", "");

                //string str = "newsbrdr\">                <tr>                  <td width=\"19%\" rowspan=\"2\" valign=\"top\" >";
                
                table = table.Replace("\n", "");
             
                table = table.Substring(90);
                var match = rx.Match(table);
                if (match.Success)
                {
                    id.Add(match.Groups[1]);
                }
                //Console.WriteLine(table);
            }

            return hasNextPage;
        }

        private async Task<MalayalamCinemaStar> ParseStarDetail(string id)
        {
            string starPage = "http://www.malayalamcinema.com/star-details.php?member_id=";
            Task<string> t = GetStreamAsync(starPage + id);
            string html = await t;
            Regex rx = new Regex("<span class=\\\"morenews\\\">(.*)<\\/span>");
            Regex blurp = new Regex("<td align=\\\"justify\\\"><p>(.*)<\\/p><\\/td>");
            Regex image = new Regex("member\\/" + id + "\\/membgal\\/(.*).jpg");
            MalayalamCinemaStar mcs = new MalayalamCinemaStar();
            var match = rx.Match(html);
            
            if (match.Success)
            {
                mcs._name = match.Groups[1].Value;
            }
            match = blurp.Match(html);
            if (match.Success)
            {
                mcs._blurp = match.Groups[1].Value;
            }
            MatchCollection matchCollection = image.Matches(html);
            if (matchCollection.Count > 0)
            {
                foreach (var c in matchCollection)
                {
                     match = (Match)c;
                    
                    if (!match.Value.Contains("thumb_"))
                    {
                        mcs._pictures.Add(match.Value);
                    }
                }
           
            }
 
            return mcs;
        }

        private static async Task<ArrayList> GetAllStarDetail(ArrayList idList)
        {
            ArrayList startDetailTasks = new ArrayList();
            ArrayList startDetails = new ArrayList();


            foreach (var id in idList)
            {
                CrawlMalayalamCinemaStar cm = new CrawlMalayalamCinemaStar();
                 Task<MalayalamCinemaStar> startDetailTask = cm.ParseStarDetail(id.ToString());
                startDetailTasks.Add(startDetailTask);
            }

            foreach(var task in startDetailTasks)
            {
                Task<MalayalamCinemaStar> taskStar = (Task<MalayalamCinemaStar>)task;
                MalayalamCinemaStar star = await taskStar;
                startDetails.Add(star);

            }
            return startDetails;
        }
        public static void Crawl()
        {
            CrawlMalayalamCinemaStar c = new CrawlMalayalamCinemaStar();
            string baseUrl = "http://www.malayalamcinema.com/meet-the-star.php?pageID=";
            int pageId = 1;
            bool baseHasNextPage = false;
    
            ArrayList idList = new ArrayList();
            do
            {

                Task<string> t = c.GetStreamAsync(baseUrl + pageId.ToString());
                t.Wait();
                pageId++;
                baseHasNextPage = c.ParsePage(t.Result, ref idList);
            }
            while (baseHasNextPage);



            Console.WriteLine("Finished star");

       
            Task<ArrayList> details = CrawlMalayalamCinemaStar.GetAllStarDetail(idList);
            details.Wait();
            Console.WriteLine("<xml>");
            foreach( var detail in details.Result)
            {
                Console.WriteLine("<actor>");
                MalayalamCinemaStar mcs = (MalayalamCinemaStar)detail;
                Console.WriteLine("<name>{0}</name>", mcs._name);
                Console.WriteLine("<blurp>{0}</blurp>", mcs._blurp);
                Console.WriteLine("<images>");
                foreach(var image in mcs._pictures)
                {
                    Console.WriteLine("<image>http://www.malayalamcinema.com/{0}</image>", image.ToString());
                }
                Console.WriteLine("</images>");

                Console.WriteLine("</actor>");
            }
            Console.WriteLine("</xml>");

        }
    }
}
