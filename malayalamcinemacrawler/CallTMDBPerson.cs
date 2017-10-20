using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace malayalamcinemacrawler
{

    
    class CallTMDBPerson
    {
        private static async Task<string> GetStreamAsync(string pageId)
        {
            string url = "https://api.themoviedb.org/3/person/popular?api_key=&language=en-US&page=" + pageId;
            WebRequest wr = WebRequest.Create(url);
            wr.Method = "GET";
           
            System.Threading.Tasks.Task<WebResponse> response = wr.GetResponseAsync();
            WebResponse rr = await response;
            System.IO.Stream dataStream = rr.GetResponseStream();

            StreamReader streamReader = new StreamReader(dataStream);
            string html = streamReader.ReadToEnd();
            return html;
        }

        public static void Get()
        {
            int pageId = 1;
            bool hasMore = true;
            while (hasMore)
            {
                Task<string> t = GetStreamAsync( pageId.ToString());
                pageId++;
                t.Wait();
                System.Threading.Thread.Sleep(200);
                 

            }
        }
    }
}
