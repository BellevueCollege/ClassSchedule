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
    public HttpWebRequest request;
    public string result, seatsAvailable;
    public int index, seats;

    //really exciting constructor
    public CourseHPQuery()
    {

    }

    //code to grab the open seats and return as string
    public int findOpenSeats (string itemNumber, string YRQ)
    {
      string postData;

      //set the location the post is going to
      request = (HttpWebRequest)WebRequest.Create("https://www.ctc.edu/cgi-bin/rq080");

      ASCIIEncoding encoding = new ASCIIEncoding();

      postData = getPostData(itemNumber, YRQ);

      byte[] data = encoding.GetBytes(postData);

      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded";
      request.ContentLength = data.Length;

      //Create a delegate to send content into the request
      AsyncCallback contentLoader = delegate(IAsyncResult asynchronousResult)
      {
        //Pull request out of async state
        HttpWebRequest req = (HttpWebRequest)asynchronousResult.AsyncState;

        //Turn our content into binary data and send it down the request stream
        byte[] content = Encoding.UTF8.GetBytes(postData);
        System.IO.Stream postStream = request.EndGetRequestStream(asynchronousResult);
        postStream.Write(content, 0, content.Length);
        postStream.Close();
      };

      //Use the delegate to pour our content down the request
      IAsyncResult res = request.BeginGetRequestStream(contentLoader, request);

      //lock while we wait for a response
      res.AsyncWaitHandle.WaitOne();

      //Pull the final content back out of the response
      WebResponse resPostback = request.GetResponse();
      StreamReader streamFinalContent = new StreamReader(resPostback.GetResponseStream());
      result = streamFinalContent.ReadLine();

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



      //set up the post parameters
			postData = "item=" + itemNumber;
      postData += "&request=classchd";
      postData += "&ayr=" + yearData; // "&ayr=2009 - 10";
      postData += "&sess=" + sessionData; // "&sess=4 - spring";
      postData += "&returnurl=http://bellevuecollege.edu/schedule/";
      return postData;
    }
  }
}
