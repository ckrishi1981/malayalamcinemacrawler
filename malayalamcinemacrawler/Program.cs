using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace malayalamcinemacrawler
{
    class Crawl
    {
        public async System.Threading.Tasks.Task<string> GetStreamAsync(string url)
        {
            WebRequest wr = WebRequest.Create(url);
            wr.Method = "GET";
            System.Threading.Tasks.Task<WebResponse> response = wr.GetResponseAsync();
            WebResponse rr = await response;
            System.IO.Stream dataStream  = rr.GetResponseStream();
           
            StreamReader streamReader = new StreamReader(dataStream);
            string html = streamReader.ReadToEnd();
            System.Console.WriteLine(html);
            return html;


        }

    }
    class Program
    {


        static void Main(string[] args)
        {
          

            Crawl c = new Crawl();
            Task t = c.GetStreamAsync("http://www.malayalamcinema.com/meet-the-star.php");
            t.ContinueWith(str
                 => Console.WriteLine(str));
            t.Wait();



         //   string s = await c.GetStreamAsync("http://www.malayalamcinema.com/meet-the-star.php");

        }

      
    }
}
