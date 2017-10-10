using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace malayalamcinemacrawler
{
    struct MalayalamCinemaStar
    {
        string _name;
       // Array<string> _pictures = new Array<string>();
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
            System.Console.WriteLine(html);
            return html;
        }

        public bool ParsePage(string pageContent)
        {
            int lookUpindex  = 0;
            string table;
            bool hasNextPage = pageContent.IndexOf("Next &gt;&gt;") >= 0;
           while (   (lookUpindex = pageContent.IndexOf("newsbrdr", lookUpindex)) >= 0)
            {
                int endIndex = 0;
                endIndex = pageContent.IndexOf("</table>", lookUpindex);
      
                table = pageContent.Substring(lookUpindex, endIndex- lookUpindex);
                lookUpindex = endIndex;
                Console.WriteLine(table);
            }

            return hasNextPage;
        }
        public void Crawl()
        {
            Crawl c = new Crawl();
            string baseUrl = "http://www.malayalamcinema.com/meet-the-star.php?pageID=";
            int pageId = 0;
            bool baseHasNextPage = false;
            do
            {
                Task<string> t = c.GetStreamAsync(baseUrl + pageId.ToString());
                t.Wait();
                pageId++;
                baseHasNextPage = ParsePage(t.Result);
            }
            while (baseHasNextPage);

        }
    }
}
