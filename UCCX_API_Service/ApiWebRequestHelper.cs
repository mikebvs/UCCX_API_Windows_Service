using System;
using System.Xml.Serialization;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace UCCX_API_Service
{
    class ApiWebRequestHelper : APIHandler
    {
        public static T GetJsonRequest<T>(string requestUrl, string username, string password, CredentialManager cm)
        {
            try
            {
                WebRequest apiRequest = WebRequest.Create(requestUrl);
                string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                apiRequest.Headers.Add("Authorization", "Basic " + encoded);
                HttpWebResponse apiResponse = (HttpWebResponse)apiRequest.GetResponse();

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    string jsonOutput;
                    using (StreamReader sr = new StreamReader(apiResponse.GetResponseStream()))
                        jsonOutput = sr.ReadToEnd();

                    var jsResult = JsonConvert.DeserializeObject<T>(jsonOutput);

                    if (jsResult != null)
                        return jsResult;
                    else
                        return default(T);
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                cm.LogMessage("Error Occurred while requesting JSON Data: " + ex.Message.ToString());
                return default(T);
            }
        }
        public static T GetJsonRequest<T>(string endpoint, CredentialManager cm)
        {
            try
            {
                string requestUrl = cm.RootURL;
                if (endpoint.StartsWith("/"))
                {
                    requestUrl += endpoint;
                }
                else
                {
                    requestUrl += "/" + endpoint;
                }
                WebRequest apiRequest = WebRequest.Create(requestUrl);
                string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(cm.Username + ":" + cm.Password));
                apiRequest.Headers.Add("Authorization", "Basic " + encoded);
                HttpWebResponse apiResponse = (HttpWebResponse)apiRequest.GetResponse();

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    string jsonOutput;
                    using (StreamReader sr = new StreamReader(apiResponse.GetResponseStream()))
                        jsonOutput = sr.ReadToEnd();

                    var jsResult = JsonConvert.DeserializeObject<T>(jsonOutput);

                    if (jsResult != null)
                        return jsResult;
                    else
                        return default(T);
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                cm.LogMessage("Error Occurred while requesting JSON Data: " + ex.Message.ToString());
                return default(T);
            }
        }
        public static T GetXmlRequest<T>(string requestUrl, string username, string password, CredentialManager cm)
        {
            try
            {
                WebRequest apiRequest = WebRequest.Create(requestUrl);
                string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                apiRequest.Headers.Add("Authorization", "Basic " + encoded);
                HttpWebResponse apiResponse = (HttpWebResponse)apiRequest.GetResponse();

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    string xmlOutput;
                    using (StreamReader sr = new StreamReader(apiResponse.GetResponseStream()))
                        xmlOutput = sr.ReadToEnd();

                    XmlSerializer xmlSerialize = new XmlSerializer(typeof(T));

                    var xmlResult = (T)xmlSerialize.Deserialize(new StringReader(xmlOutput));

                    if (xmlResult != null)
                        return xmlResult;
                    else
                        return default(T);
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                cm.LogMessage("Error Occurred while requesting XML Data: " + ex.Message.ToString());
                return default(T);
            }
        }
        public static T GetXmlRequest<T>(string endpoint, CredentialManager cm)
        {
            try
            {
                string requestUrl = cm.RootURL;
                if (endpoint.StartsWith("/"))
                {
                    requestUrl += endpoint;
                }
                else
                {
                    requestUrl += "/" + endpoint;
                }
                WebRequest apiRequest = WebRequest.Create(requestUrl);
                string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(cm.Username + ":" + cm.Password));
                apiRequest.Headers.Add("Authorization", "Basic " + encoded);
                HttpWebResponse apiResponse = (HttpWebResponse)apiRequest.GetResponse();

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    string xmlOutput;
                    using (StreamReader sr = new StreamReader(apiResponse.GetResponseStream()))
                        xmlOutput = sr.ReadToEnd();

                    XmlSerializer xmlSerialize = new XmlSerializer(typeof(T));

                    var xmlResult = (T)xmlSerialize.Deserialize(new StringReader(xmlOutput));

                    if (xmlResult != null)
                        return xmlResult;
                    else
                        return default(T);
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                cm.LogMessage("Error Occurred while requesting XML Data: " + ex.Message.ToString());
                return default(T);
            }
        }
    }
}
