using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Net;
using System.Text;
using System.IO;

namespace CTCClassSchedule
{
	public class CourseHPQuery
	{
    public WebRequest request;
    public string result, seatsAvailable;
    public int index, seats;

    //really exciting constructor
    public CourseHPQuery()
    {

    }

    //code to grab the open seats and return as string
    public int findOpenSeats (string itemNumber, string YRQ)
    {
			//much of this is from: http://msdn.microsoft.com/en-us/library/debx8sh9.aspx

      string postData;
			string colCode = ConfigurationManager.AppSettings["CollegeCode"]; // College Code to construct URL


      //set the location the post is going to
      request = WebRequest.Create("https://www.ctc.edu/cgi-bin/rq" + colCode);
			request.Method = "POST";
      postData = getPostData(itemNumber, YRQ);

      byte[] byteArray = Encoding.UTF8.GetBytes(postData);

			// Set the ContentType property of the WebRequest.
			request.ContentType = "application/x-www-form-urlencoded";
			// Set the ContentLength property of the WebRequest.
			request.ContentLength = byteArray.Length;
			// Get the request stream.
			Stream dataStream = request.GetRequestStream();
			// Write the data to the request stream.
			dataStream.Write(byteArray, 0, byteArray.Length);
			// Close the Stream object.
			dataStream.Close();
			// Get the response.
			WebResponse response = request.GetResponse();
			// Display the status.
			Console.WriteLine(((HttpWebResponse)response).StatusDescription);
			// Get the stream containing content returned by the server.
			dataStream = response.GetResponseStream();
			// Open the stream using a StreamReader for easy access.
			StreamReader reader = new StreamReader(dataStream);
			// Read the content.
			result = reader.ReadToEnd();

			// Clean up the streams.
			reader.Close();
			dataStream.Close();
			response.Close();

      //grab the start of the 'seats available' string
      index = result.IndexOf("Seats Available: ");

      //if 'seats available' was found
      if (index != -1)
      {
        seatsAvailable = result.Substring(index + 17, 3);
        seats = Convert.ToInt16(seatsAvailable);

        return seats;
      }

      //if there are no seats available, return 0
      else
					return 0;
    }

    private static string getPostData(string itemNumber, string YRQ)
    {
      string postData = "";
      string sessionData = "";
      string yearData = "";
      int yearDataInt = 0;

      string quarter = YRQ[3].ToString();
      string yearFrom = YRQ[1].ToString();
      string yearTo = YRQ[2].ToString();
      string decade = YRQ[0].ToString();

      switch (decade)
      {
        case "A":
          yearDataInt = 2000;
          break;
        case "B":
          yearDataInt = 2010;
          break;
        default:
          yearDataInt = 2010;
          break;
      }

      switch (quarter)
      {
        case "1":
          sessionData = "1 - summer";
          break;
        case "2":
          sessionData = "2 - fall";
          break;
        case "3":
          sessionData = "3 - winter";
          break;
        case "4":
          sessionData = "4 - spring";
          break;
        default:
          sessionData = "";
          break;

      }

      yearData += (yearDataInt + Int32.Parse(yearFrom)).ToString() + " - ";
      yearData += (yearDataInt + Int32.Parse(yearFrom) + 1).ToString()[2];
      yearData += (yearDataInt + Int32.Parse(yearFrom) + 1).ToString()[3];


			string schoolURL = ConfigurationManager.AppSettings["currentSchoolUrl"]; // School URL for returnURL
			string scheduleDir = ConfigurationManager.AppSettings["currentAppSubdirectory"]; // App SubDir for returnURL


      //set up the post parameters
			postData = "item=" + itemNumber;
      postData += "&request=classchd";
      postData += "&ayr=" + yearData; // "&ayr=2009 - 10";
      postData += "&sess=" + sessionData; // "&sess=4 - spring";
      postData += "&returnurl=" + schoolURL + scheduleDir;
      return postData;
    }
  }
}
