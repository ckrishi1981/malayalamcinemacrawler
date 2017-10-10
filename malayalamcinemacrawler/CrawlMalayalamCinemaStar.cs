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
        public ArrayList _pictures = new ArrayList();
    }
    class CrawlMalayalamCinemaStar
    {
       

        private async System.Threading.Tasks.Task<string> GetStreamAsync(string url)
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
                Console.WriteLine(table);
            }

            return hasNextPage;
        }

        private async void ParseStarDetail(string id)
        {
            string starPage = "http://www.malayalamcinema.com/star-details.php?member_id=";
            Task<string> t = GetStreamAsync(starPage + id);
            string html = await t;
            


        }
        public static void Crawl()
        {
            CrawlMalayalamCinemaStar c = new CrawlMalayalamCinemaStar();
            string baseUrl = "http://www.malayalamcinema.com/meet-the-star.php?pageID=";
            int pageId = 0;
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


            

        }
    }
}
