using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace Html_List_to_Folders
{
    class Program
    {
        static void Main(string[] args)
        {
            //Notes: The Input File is the Entire Html body text file of the Favorites page with the Display Setting set to: Extended

			string htmlTextLocation = ""; //"All Html.txt" location here
            string HtmlText = File.ReadAllText(htmlTextLocation + "/All Html.txt");

            string Database = ""; //Location for folder database

            var pattern = "<table class=\"itg glte\">.*</table>";
            MatchCollection Table = Regex.Matches(HtmlText, pattern);

            List<TheContent> TheContentList = new List<TheContent>();

            foreach (Match TableElement in Table)
            {
                string TableText = TableElement.ToString();
                var ContentPattern = "style=\"width:250px\">.*?<tr.*?><td class=\"gl1e\"";
                MatchCollection Content = Regex.Matches(TableText, ContentPattern);
                List<string> trList = new List<string>();
                string forLast = TableText;
                foreach (var toAdd in Content)
                {
                    trList.Add(toAdd.ToString());
                }

                int indexForLast = forLast.LastIndexOf("style=\"width:250px\">");
                
                forLast = forLast.Substring(indexForLast);
                int indexForLast2 = forLast.LastIndexOf("</table>");
                forLast = forLast.Substring(0, indexForLast2);
                int indexForLast3 = forLast.LastIndexOf("</table>");
                forLast = forLast.Substring(0, indexForLast3);
                trList.Add(forLast);

                for (int i = 0; i < trList.Count; i++) 
                {
                    trList[i] = Regex.Match(trList[i], "<a href.*</tr>").ToString();
                }

                //List<TheContent> TheContentList = new List<TheContent>();
                foreach (string SingleContent in trList)
                {
                    TheContent TheContent = new TheContent();

                    string Title = Regex.Match(SingleContent, "glink.*?<").ToString().Replace("glink\">", "").Replace("<", "");
                    TheContent.Title = Title;

                    string pattern2 = "<img.*?src=\".*?\"";
                    string image = Regex.Match(SingleContent, pattern2).ToString();
                    string imageLink = Regex.Match(image, "https.*").ToString().Replace("\"", "");
                    TheContent.CoverLink = imageLink;

                    string Tagbody = Regex.Match(SingleContent, "<table>.*</table>").ToString();
                    XmlDocument TagXml = new XmlDocument();
                    TagXml.LoadXml(Tagbody);
                    XmlNodeList TagXmlList = TagXml.SelectNodes("//tr");

                    foreach (XmlNode tr in TagXmlList)
                    {
                        string TagCategory = tr.FirstChild.InnerXml.Replace(":","");
                        //XmlNode InsideTags = tr.LastChild;
                        XmlNodeList InsideTags = tr.LastChild.ChildNodes;


                        List<string> InsideTagNames = new List<string>();
                        foreach (XmlNode InsideTag in InsideTags)
                        {
                            InsideTagNames.Add(InsideTag.InnerXml);
                        }
                        TheContent.Tags.Add(TagCategory, InsideTagNames);
                    }

                    TheContentList.Add(TheContent);
                }
            }

            //foreach (TheContent TheContent in TheContentList)
            //{
            //    Console.WriteLine(TheContent.Title);
            //    Console.WriteLine(TheContent.CoverLink);

            //    foreach (KeyValuePair<string, List<string>> TagPair in TheContent.Tags)
            //    {
            //        Console.Write(TagPair.Key + ": ");
            //        foreach (string Tag in TagPair.Value)
            //        {
            //            Console.Write(Tag + " ");
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine();
            //}

            using (WebClient WebClient = new WebClient())
            {
                foreach (TheContent TheContent in TheContentList)
                {
                    string separators = "[<>:\"/\\?|*]";
                    string FolderFriendlyTitle = new Regex(separators).Replace(TheContent.Title,"");
                    string CurrentPath = Database + "/" + FolderFriendlyTitle;
                    string CurrentInfoPath = CurrentPath + "/Info";

                    Console.Write("Writing \"" + TheContent.Title + "\"...");

                    System.IO.Directory.CreateDirectory(CurrentInfoPath);

                    string CoverFormat = TheContent.CoverLink.Substring(TheContent.CoverLink.LastIndexOf("."));

                    string imagePath = CurrentPath + "/Cover" + CoverFormat;
                    while (File.Exists(imagePath))
                    {
                        imagePath = imagePath.Substring(0, imagePath.LastIndexOf(".")) + "_" + CoverFormat;
                    }
                    WebClient.DownloadFile(TheContent.CoverLink, imagePath);

                    StreamWriter NameWriter = new StreamWriter(CurrentInfoPath+"/Name.txt", false);
                    NameWriter.Write(TheContent.Title);
                    NameWriter.Close();

                    string TagsToWrite = "";
                    foreach (KeyValuePair<string, List<string>> TagPair in TheContent.Tags)
                    {
                        TagsToWrite += TagPair.Key + ": ";
                        foreach (string Tag in TagPair.Value)
                        {
                            TagsToWrite += Tag + " ";
                        }
                        TagsToWrite += System.Environment.NewLine;
                    }
                    StreamWriter TagsWriter = new StreamWriter(CurrentInfoPath + "/Tags.txt", false);
                    TagsWriter.Write(TagsToWrite);
                    TagsWriter.Close();

                    Console.WriteLine("Done");
                }
            }
                
        }
        public class TheContent
        {
            public string Title = "";
            public string CoverLink = "";
            public Dictionary<string, List<string>> Tags = new Dictionary<string, List<string>>();

        }
    }
}