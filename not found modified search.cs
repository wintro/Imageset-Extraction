using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NotFound_Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string Database = ""; //Database location
			string sourceUrl = "" //Source Website Url

            //List<string> NotFoundList = new List<string>();

            //dic = name, folder
            Dictionary<string, string> NotFoundList = new Dictionary<string, string>();

            StreamReader NotFoundReader = new StreamReader(Database + "/NotFound.txt");

            Dictionary<string, string> StillNotFound = new Dictionary<string, string>();

            string readLine = "";
            while((readLine = NotFoundReader.ReadLine()) != null)
            {
                string pattern = "(.*) Folder: (.*)";
                Match match = Regex.Match(readLine, pattern);
                string name = match.Groups[1].ToString();
                string folder = match.Groups[2].ToString();
                if (!NotFoundList.ContainsKey(name))
                {
                    NotFoundList.Add(name, folder);
                }
            }

            NotFoundReader.Close();

            foreach (KeyValuePair<string, string> item in NotFoundList)
            {
                Console.WriteLine(item.Key);
                Console.WriteLine(item.Value);
            }


            foreach (KeyValuePair<string,string> item in NotFoundList)
            {
                bool found = false;
                string nameToSearch = item.Key;
                int charDeleteEnd = nameToSearch.Length/3;
                int charLeft = nameToSearch.Length;
                nameToSearch = nameToSearch.Replace(",", " ").Replace(")", " ").Replace("(", " ").Replace("?"," ").Replace("!"," ").Replace("="," ").Replace("…"," ").Replace("@"," ");
                string[] nameProcessing = nameToSearch.Split(' ');
                for (int i = 0; i < nameProcessing.Length; i++)
                {
                    if (nameProcessing[i].Contains('.'))
                    {
                        nameProcessing[i] = "";
                    }
                }
                nameToSearch = string.Join(' ', nameProcessing);

                while (/*!found || charLeft < charDeleteEnd*/false)
                {
                    Console.WriteLine("Searching: " + nameToSearch);
                    //change second argument to item.Value
                    if(DownloadAllImages(nameToSearch,Database + "/" + item.Key) == null)
                    {
                        found = true;
                        StreamWriter Writer = new StreamWriter(Database + "/" + item.Key + "/NotFound Downloader.txt");
                        Writer.Write("true");
                        Writer.Close();
                    }
                    else
                    {
                        nameToSearch = nameToSearch.Substring(0, nameToSearch.Length - 1);
                        //charLeft--;
                    }
                }

                Console.WriteLine("Searching: " + nameToSearch);
                if (DownloadAllImages(nameToSearch, item.Value) == null)
                {
                    //StreamWriter Writer = new StreamWriter(Database + "/" + item.Key + "/NotFound Downloader.txt");
                    StreamWriter Writer = new StreamWriter(item.Value + "/NotFound Downloader.txt");
                    Writer.Write("true");
                    Writer.Close();
                }
                else
                {
                    Console.WriteLine("*** Not Found: " + nameToSearch);
                    StreamWriter Writer = new StreamWriter(Database + "/NotFound2.txt", true);
                    Writer.WriteLine(item.Key + " Folder: " + item.Value);
                    Writer.Close();
                }
            }

            string DownloadAllImages(string name, string dir)
            {
                //Gallery Search
                string InputTitle = name;

                string HtmlSearchData = GetHtml(sourceUrl + "/search/?q=" + InputTitle);

                //Console.WriteLine(HtmlSearchData);

                string pattern = "<div class=\"gallery\".*<a href=\"(.*?)\"";

                if (!HtmlSearchData.Contains("No results found"))
                {
                    string href = Regex.Match(HtmlSearchData, pattern).Groups[1].ToString();

                    //Console.WriteLine(href);

                    string HtmlGalleryData = GetHtml(sourceUrl + href + "1");

                    //Console.WriteLine(HtmlGalleryData);

                    int numPages = 0;

                    string pagePattern = "num_pages\":(\\d*)";

                    numPages = Int32.Parse(Regex.Match(HtmlGalleryData, pagePattern).Groups[1].ToString());

                    string databasePattern = "image-container.*\\s.*\\s.*\\s.*\\s.*\\s.*\\s.*\\s.*\\s.*\\s.*\\s<img src=\"(.*?)\"";

                    string databaseUrl = Regex.Match(HtmlGalleryData, databasePattern).Groups[1].ToString();

                    //Console.WriteLine(databaseUrl);

                    string imageType = databaseUrl.Substring(databaseUrl.LastIndexOf("."));

                    //Console.WriteLine(imageType);

                    string urlNoNum = databaseUrl.Substring(0, databaseUrl.LastIndexOf(".") - 1);

                    using (WebClient WebClient = new WebClient())
                    {
                        for (int i = 1; i <= numPages; i++)
                        {
                            System.IO.Directory.CreateDirectory(dir + "\\Images");
                            bool success = false;
                            Console.WriteLine("Downloading Page: " + i);
                            while (!success)
                            {
                                try
                                {
                                    string saveDir = dir + "\\Images\\" + i + imageType;
                                    string url = urlNoNum + i + imageType;
                                    WebClient.DownloadFile(url, saveDir);
                                    success = true;
                                }
                                catch (Exception ex)
                                {
                                    if (imageType == ".jpg")
                                    {
                                        imageType = ".png";
                                    }
                                    else
                                    {
                                        imageType = ".jpg";
                                    }
                                    Console.WriteLine(ex);
                                }
                            }
                        }
                    }
                    return null;
                }
                else
                {
                    return name;
                }


            }

            string GetHtml(string url)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string HtmlData = "Html Read Error";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    HtmlData = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();

                }

                return HtmlData;
            }
        }
    }
}
