using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Image_set_Search_and_Download_from_HTML
{
    class Program
    {
        static void Main(string[] args)
        {
			//Note: Some code used for testing or old code that has been improved elsewhere has been commented out
			
			
			//Folder to be saved in
            string database = "F:/OneDrive/Pictures/Sad Panda/Favorites List/MangaDoujinshi";
            string[] databaseFolders = Directory.GetDirectories(database);
            List<string> galleriesNotFound = new List<string>();
			
			
            List<int> skip = new List<int>(new int[] {0, 1, 2, 4, 5, 6, 8});
			string sourceUrl = "" //Source Website Url
			
			
            //foreach (string folder in databaseFolders)
            //{
            //    string name = File.ReadAllText(folder + "/Info/Name.txt");
            //    Console.Write("Downloading: " + name);
            //    Console.WriteLine(" Into: " + folder);
            //    if (!(DownloadAllImages(name, folder) == null))
            //    {
            //        Console.WriteLine("Not Found: " + name);
            //        galleriesNotFound.Add(name + "Folder: " + folder);
            //    }
            //}



            for (int i = 0; i < databaseFolders.Length; i++)
            {
                if (/*skip.Contains(i)*/true)
                {
                    string folder = databaseFolders[i];
                    string name = File.ReadAllText(folder + "/Info/Name.txt");
                    Console.Write("Downloading: " + name);
                    Console.WriteLine(" Into: " + folder);
                    if (!(DownloadAllImages(name, folder) == null))
                    {
                        Console.WriteLine("*** Not Found: " + name);
                        galleriesNotFound.Add(name + "Folder: " + folder);
                        StreamWriter SearchNotFound = new StreamWriter(database + "/NotFound.txt", true);
                        SearchNotFound.WriteLine(name + " Folder: " + folder);
                        SearchNotFound.Close();
                    }
                }
            }

			//Make a list of image sets not found and save as .txt
            if (galleriesNotFound.Count > 0)
            {
               StreamWriter SearchNotFound = new StreamWriter(database + "/NotFound.txt", false);
               foreach(string title in galleriesNotFound)
               {
                   SearchNotFound.WriteLine(title);
               }
               SearchNotFound.Close();
            }

            string DownloadAllImages(string name, string dir)
            {
                //Gallery Search
                string InputTitle = name;

                string HtmlSearchData = GetHtml(sourceUrl + "/search/?q=" + InputTitle);

                //Console.WriteLine(HtmlSearchData);

                if(!HtmlSearchData.Contains("No results found"))
                {
					string pattern = "<div class=\"gallery\".*<a href=\"(.*?)\"";
					
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
