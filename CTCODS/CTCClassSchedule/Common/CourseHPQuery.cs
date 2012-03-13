using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Encoder = Microsoft.Security.Application.Encoder;

namespace CTCClassSchedule
{
	public class CourseHPQuery
	{
		//code to grab the open seats and return as string
    public int FindOpenSeats (string itemNumber, string YRQ)
    {
			//much of this is from: http://msdn.microsoft.com/en-us/library/debx8sh9.aspx

			// TODO: gracefully handle missing AppSetting value
	string colCode = ConfigurationManager.AppSettings["collegeCode"]; // College Code to construct URL

      //set the location the post is going to
	WebRequest request = WebRequest.Create("https://www.ctc.edu/cgi-bin/rq" + colCode);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			string postData = GetPostData(itemNumber, YRQ);
      byte[] byteArray = Encoding.UTF8.GetBytes(postData);

			request.ContentLength = byteArray.Length;

	string result;
			try
			{
				using (Stream requestStream = request.GetRequestStream())
				{
					requestStream.Write(byteArray, 0, byteArray.Length);
					requestStream.Close();	// do we need to close the stream before we can GetResponse()?

					// Get the response.
					using (WebResponse response = request.GetResponse())
					{
						if (response != null)
						{
							string status = ((HttpWebResponse)response).StatusDescription;

							if (status == "OK")	// HTTP 200
							{
								using (Stream responseStream = response.GetResponseStream())
								{
									if (responseStream != null)
									{
										using (StreamReader reader = new StreamReader(responseStream))
										{
											result = reader.ReadToEnd();
										}

										// Detect "server busy"
										if (Regex.IsMatch(result, "503 Service Unavailable", RegexOptions.IgnoreCase))
										{
											Trace.WriteLine(string.Format("Call to '{0}' returned '{1}'", request.RequestUri, "503 Service Unavailable"));
											return -1;
										}

										//grab the start of the 'seats available' string
										int index = result.IndexOf("Seats Available: ");

										//if 'seats available' was found
										if (index > -1)
										{
											string seatsAvailable = result.Substring(index + 17, 3);
											int seats = Convert.ToInt16(seatsAvailable);

											return seats;
										}

										//if there are no seats available, return 0
										return 0;
									}
								}
							}
							else
							{
								Trace.WriteLine(string.Format("Call to '{0}' returned '{1}'", request.RequestUri, ((HttpWebResponse)response).StatusCode));
							}
						}
					}
				}
				return -1;
			}
			catch
			{
				//if the service cannot be reached, return a -1 for seats available.
				return -1;
			}

    }

		// TODO: Use API's YearQuarter methods for these calculations
    private static string GetPostData(string itemNumber, string YRQ)
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
          sessionData = "1+-+summer";
          break;
        case "2":
          sessionData = "2+-+fall";
          break;
        case "3":
          sessionData = "3+-+winter";
          break;
        case "4":
          sessionData = "4+-+spring";
          break;
        default:
          sessionData = "";
          break;

      }

      yearData += (yearDataInt + Int32.Parse(yearFrom)).ToString() + "+-+";
      yearData += (yearDataInt + Int32.Parse(yearFrom) + 1).ToString()[2];
      yearData += (yearDataInt + Int32.Parse(yearFrom) + 1).ToString()[3];

			// TODO: gracefully handle missing AppSetting value
			string schoolURL = ConfigurationManager.AppSettings["currentSchoolUrl"]; // School URL for returnURL
			string scheduleDir = ConfigurationManager.AppSettings["currentAppSubdirectory"]; // App SubDir for returnURL


      //set up the post parameters
			postData = "item=" + itemNumber;
      postData += "&request=classchd";
      postData += "&ayr=" + yearData; // "&ayr=2009 - 10";
      postData += "&sess=" + sessionData; // "&sess=4 - spring";
      postData += "&returnurl=" + Encoder.HtmlFormUrlEncode(schoolURL) + Encoder.HtmlFormUrlEncode(scheduleDir);
      return postData;
    }
  }
}
