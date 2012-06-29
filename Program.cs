using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace CoverMyMeds.Claims
{
    class Program
    {
        /// <summary>
        /// Basic console application to illustrate a .Net call to the CoverMyMeds Pharmacy Claim API
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Beginning Claim API Test");
            TestClaimAPI();
            Console.WriteLine("Claim API Test Complete");
            Console.ReadLine();
        }

        /// <summary>
        /// Allows a user to test sending a claim against CoverMyMeds using a .Net client.
        /// Uses a small utility class reference to handle the direct interface to the HTTP Post API.
        /// Provides an example for handling possible HTTP Response errors and suggested user messages.
        /// </summary>
        private static void TestClaimAPI()
        {
            // Collect claim API parameter values
            Console.Write("Enter in username to submit with claim (leave blank to use console app default): ");
            string username = Console.ReadLine();
            if (string.IsNullOrEmpty(username)) username = "YourUsernameHere";

            Console.Write("Enter in user password to submit with claim (leave blank to use console app default): ");// Backspace Should Not Work
            string password = ReceiveUserPassword();

            if (string.IsNullOrEmpty(password)) password = "YourPasswordHere";

            Console.Write("Enter in claims api key to submit with claim (leave blank to use console app default): ");
            string api_key = Console.ReadLine();
            if (string.IsNullOrEmpty(api_key)) api_key = "a4b05a8151b4ddda2739e355aefab48a";

            // Make sure to submit unique information each time or the claims server will return the previously generated request
            string ncpdp_claim = @"035079D0B1A4        1071804031        20120123          AM04C2H11443628C1CKYAC301C61AM01C419580518C52CATONA LCBDO<BCM1510 ST RT 176CNAOYWHERECOFLCP42345AM07EM1D21439264E103D753746025310E760000D530DE20110826EX6142328850AM03EZ01DB1679652648DRSMITHPM27075438802JRALPH2K101 MAIN STREET2MCENTRALX CITY2NKY2P42330";
            //string ncpdp_claim = @"035079D0B1A4        1071804031        20120123        AM07C2H11443628C1CKYAC301C61AM01C419580518C52CATONA LCBDO<BCM1510 ST RT 176CNAOYWHERECOFLCP42345AM07EM1D21439264E103D753746025310E760000D530DE20110826EX6142328850AM03EZ01DB1679652648DRSMITHPM27075438802JRALPH2K101 MAIN STREET2MCENTRALU CITY2NKY2P42330";

            string ClaimURI = @"https://claims.covermymeds.com/cmmimport";

            //string ClaimURI_JSON = @"https://claims.covermymeds.com/cmmimport/json";

            byte[] PostBytes = Utilities.ConstructClaimPostData(username, password, api_key, ncpdp_claim, ContructOptionalVariablesForTesting());

            try
            {
                WebResponse claim_response = Utilities.RequestClaim(ClaimURI, PostBytes);
                using (StreamReader sr = new StreamReader(claim_response.GetResponseStream()))
                {
                    string AnswerString = sr.ReadToEnd();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("Successful Claim Response");
                    Console.WriteLine("------------------------------------------------{0}", System.Environment.NewLine);
                    Console.WriteLine(AnswerString);
                    Console.WriteLine("");
                }
            }
            catch (System.Net.WebException ClaimWebException)
            {
                // Ooops, something went tragically wrong. Let's find out what it was
                HttpWebResponse ErrResponse = (HttpWebResponse)ClaimWebException.Response;

                Console.WriteLine("HTTP Web Exception Occurred!!{0}", System.Environment.NewLine);
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("Suggested Error Message");
                Console.WriteLine("------------------------------------------------{0}", System.Environment.NewLine);
                switch (ErrResponse.StatusCode)
                {
                    case HttpStatusCode.Ambiguous:
                    case HttpStatusCode.Moved:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.SeeOther:
                    case HttpStatusCode.NotModified:
                    case HttpStatusCode.UseProxy:
                    case HttpStatusCode.Unused:
                    case HttpStatusCode.TemporaryRedirect: //3xx error
                        Console.WriteLine("Oops, there was a connection problem. Please try one more time, then contact CoverMyMeds at 1-866-452-5017/help@covermymeds.com and they will help you diagnose this issue.");
                        break;
                    case HttpStatusCode.BadRequest:     //400
                        Console.WriteLine("Oops, there was a connection problem. Please try one more time, then contact CoverMyMeds at 1-866-452-5017/help@covermymeds.com and they will help you diagnose this issue.");
                        break;
                    case HttpStatusCode.Forbidden:      //403
                        Console.WriteLine("Oops, login failed for the username or password that was submitted. Please check the username and password in your account settings in your Pharmacy System and at the CMM website to make sure they match. If you still have trouble, please contact CoverMyMeds at 1-866-452-5017/help@covermymeds.com and they will help you fix this issue");
                        break;
                    case HttpStatusCode.NotFound:       //404
                        Console.WriteLine("Oops, there was a problem. Please check the username and password in your account settings in your Pharmacy System and at the CMM website to make sure they match. If you still have trouble, please contact CoverMyMeds at 1-866-452-5017/help@covermymeds.com and they will help you fix this issue.");
                        break;
                    case HttpStatusCode.RequestTimeout: //408
                        Console.WriteLine("Oops, there was a timeout. Please try the request again in one minute. If you still have trouble, please contact CoverMyMeds at 1-866-452-5017/help@covermymeds.com and they will help you fix this issue.");
                        break;
                    case HttpStatusCode.InternalServerError:    //500
                        Console.WriteLine("Oops, there was a problem. Please try the request again in one minute. If you still have trouble, please contact CoverMyMeds at 1-866-452-5017/help@covermymeds.com and they will help you diagnose this issue.");
                        break;
                    default:
                        Console.WriteLine("Unmapped StatusCode: {0}", ErrResponse.StatusCode.ToString());
                        break;
                }

                Console.WriteLine("{0}------------------------------------------------{0}Response Message{0}------------------------------------------------{0}", System.Environment.NewLine);
                Console.WriteLine(ClaimWebException.Message);

                string ErrorContent;
                Console.WriteLine("{0}------------------------------------------------{0}Response Content{0}------------------------------------------------{0}", System.Environment.NewLine);
                using (StreamReader sr = new StreamReader(ClaimWebException.Response.GetResponseStream()))
                {
                    ErrorContent = sr.ReadToEnd();
                    Console.WriteLine(ErrorContent);
                }
                Console.WriteLine("{0}------------------------------------------------{0}Response Content Parsed Message{0}------------------------------------------------{0}", System.Environment.NewLine);
                int ErrorCount = 1;
                foreach (string SingleError in ParseErrorResponseHTML(ErrorContent))
                {
                    Console.WriteLine("Error {0}: {1}", ErrorCount, SingleError);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        #region Console App Helper Functions

        /// <summary>
        /// A collection of values to add as optional parameters for the Claims API call
        /// </summary>
        /// <returns>List of KeyValuePairs encapsulating optional values to submit with claim</returns>
        private static List<KeyValuePair<string, string>> ContructOptionalVariablesForTesting()
        {
            List<KeyValuePair<string, string>> OptionalVariables = new List<KeyValuePair<string, string>>();
            OptionalVariables.Add(new KeyValuePair<string, string>("physician_npi", "1233001942"));
            OptionalVariables.Add(new KeyValuePair<string, string>("rejection_code", "75"));
            OptionalVariables.Add(new KeyValuePair<string, string>("rejection_msg", "Prior Authorization Required"));
            OptionalVariables.Add(new KeyValuePair<string, string>("physician_specialty", "Cardiology"));
            return OptionalVariables;
        }

        /// <summary>
        /// Sample function to pull the error message returned the response content stream
        /// </summary>
        /// <param name="ErrorResponseHTML">HTTP Error Response content streamed into a string</param>
        /// <returns>Error</returns>
        private static List<string> ParseErrorResponseHTML(string ErrorResponseHTML)
        {
            List<string> lsRet = new List<string>();
            Match m = Regex.Match(ErrorResponseHTML, @"<p>\s*(.+?)\s*</p>");
            while (m.Success)
            {
                lsRet.Add(m.Groups[1].Value);
                m = m.NextMatch();
            }

            return lsRet;
        }

        /// <summary>
        /// Just a quick little function to  hide passwords from prying eyes while testing
        /// </summary>
        /// <returns></returns>
        private static string ReceiveUserPassword()
        {
            string UserPassword = "";
            ConsoleKeyInfo key;
            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key != ConsoleKey.Backspace)
                {
                    UserPassword += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (UserPassword.Length > 0)
                    {
                        UserPassword = UserPassword.Substring(0, (UserPassword.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            Console.WriteLine();
            return UserPassword;
        }

        #endregion
    }
}
