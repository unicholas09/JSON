using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Exchange.WebServices.Data;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using System.IO;
using System.Web.Script.Serialization;

namespace MobileApp
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public string connString = ConfigurationManager.ConnectionStrings["Enpower"].ConnectionString;
        public bool invalid = false;
      
        public string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }

        public Boolean GetAccessCode(string pin, string phoneNumber)
        {
            Boolean flag;
            using (SqlConnection sqlConn = new SqlConnection(connString))
            {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand("Exec LEG_GetAccessCode @rsapin, @phoneNumber, @flag output");
                sqlCmd.Connection = sqlConn;
                sqlCmd.Parameters.AddWithValue("@rsapin", pin);
                sqlCmd.Parameters.Add("@flag", SqlDbType.Bit);
                sqlCmd.Parameters["@flag"].Direction = ParameterDirection.Output;
                sqlCmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                sqlCmd.ExecuteNonQuery();
                flag = (Boolean)sqlCmd.Parameters["@flag"].Value;
            }
            return flag;
        }

        public string GetDetailedReport(int accessCode, DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable("DetailedReport");
            using (SqlConnection sqlConn = new SqlConnection(connString))
            {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand("exec LEG_GetDetailedReport @accessCode, @StartDate, @EndDate");
                sqlCmd.Connection = sqlConn;
                sqlCmd.Parameters.AddWithValue("@accessCode", accessCode);

                SqlParameter StartDate = new SqlParameter("@StartDate", SqlDbType.DateTime);
                sqlCmd.Parameters.Add(StartDate);
                sqlCmd.Parameters["@StartDate"].Value = startDate;

                SqlParameter EndDate = new SqlParameter("@EndDate", SqlDbType.DateTime);
                sqlCmd.Parameters.Add(EndDate);
                sqlCmd.Parameters["@EndDate"].Value = endDate;

                SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                da.Fill(dt);
                return DataTableToJSONWithJSONNet(dt);
            }
        }

        public string GetSummaryReports(int accessCode)
        {
            DataTable dt = new DataTable("SummaryReport");
            using (SqlConnection sqlConn = new SqlConnection(connString))
            {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand("exec LEG_GetSummaryReport @accesscode");
                sqlCmd.Connection = sqlConn;
                sqlCmd.Parameters.AddWithValue("@accesscode", accessCode);

                SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                da.Fill(dt);
                return DataTableToJSONWithJSONNet(dt);
            }
        }

        public string UnitPriceRange(string FundID, DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable("UnitPrice");
            using (SqlConnection sqlConn = new SqlConnection(connString))
            {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand("Select ValueDate,IsNull(UnitPrice,BidPrice) as UnitPrice from UnitPrice Where FundID = @FundID and ValueDate between @StartDate and @EndDate");
                sqlCmd.Connection = sqlConn;
                sqlCmd.Parameters.AddWithValue("@FundID", FundID);

                SqlParameter StartDate = new SqlParameter("@StartDate", SqlDbType.DateTime);
                sqlCmd.Parameters.Add(StartDate);
                sqlCmd.Parameters["@StartDate"].Value = startDate;

                SqlParameter EndDate = new SqlParameter("@EndDate", SqlDbType.DateTime);
                sqlCmd.Parameters.Add(EndDate);
                sqlCmd.Parameters["@EndDate"].Value = endDate;

                SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                da.Fill(dt);
                return DataTableToJSONWithJSONNet(dt);
            }
        }

        public string UnitPrices(int FundID)
        {
            DataTable dt = new DataTable("UnitPrice");
            using (SqlConnection sqlConn = new SqlConnection(connString))
            {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand("Select ValueDate,IsNull(UnitPrice,BidPrice) as UnitPrice from UnitPrice Where FundID = @FundID and ValueDate = (select max(valuedate) from UnitPrice Where FundID = @FundID)");
                sqlCmd.Connection = sqlConn;
                sqlCmd.Parameters.AddWithValue("@FundID", FundID);
                SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                da.Fill(dt);
                return DataTableToJSONWithJSONNet(dt);                
            }
        }

        public string ValidateCustomer(string pin, string accessCode, string phoneNumber)
        {
            DataTable dt = new DataTable("Employee");
            using (SqlConnection sqlConn = new SqlConnection(connString))
            {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand("exec LEG_Validate_Customer @rsapin, @accesscode, @phoneNumber");
                sqlCmd.Connection = sqlConn;
                sqlCmd.Parameters.AddWithValue("@rsapin", pin);
                sqlCmd.Parameters.AddWithValue("@accesscode", accessCode);
                sqlCmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                da.Fill(dt);
                return DataTableToJSONWithJSONNet(dt);
            }
        }

        public Boolean CreateNewUser(string firstName, string middleName, string lastName, DateTime dateOfBirth, string phone1, string phone2, string email, string employerName, string officeAddress1, string officeAddress2, string officeState)
        {
            bool flag;
            //use OWA to send out email and log the entry into a database table
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
            service.Credentials = new WebCredentials("ebusiness@legacypension.com", "legacy12.");
            service.TraceEnabled = true;
            service.TraceFlags = TraceFlags.All;
            service.AutodiscoverUrl("ebusiness@legacypension.com", RedirectionUrlValidationCallback);

            string receipient = "info@legacypension.com";
            EmailMessage emailSender = new EmailMessage(service);
            emailSender.ToRecipients.Add(receipient);
            emailSender.Subject = "NEW ACCOUNT GENERATION (LEAD DERIVED FROM FCMB MOBILE APPLICATION)";
            emailSender.Body = "<html xmlns=" + "http://www.w3.org/1999/xhtml" + ">" +
                "<head runat=" + "server" + ">" +
                    "<title></title>" +
                    "<style type=" + "text/css" + ">" +
                        ".style1" +
                        "{" +
                            "width: 175px;" +
                        "}" +
                        ".style2" +
                        "{" +
                            "width: 159px;" +
                        "}" +
                    "</style>" +
                "</head>" +
                "<body>" +
                    "<form id=" + "form1" + "runat=" + "server" + ">" +
                    "<div>" +
                       "<table >" +

                        "<tr>" +
                        "<td class=" + "style1" + "></td>" +
                        "<td class=" + "style2" + "></td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Firstname</td>" +
                        "<td class=" + "style2" + ">" + firstName + "</td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Middlename</td>" +
                        "<td class=" + "style2" + ">" + middleName + "</td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Lastname</td>" +
                        "<td class=" + "style2" + ">" + lastName + "</td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Date of Birth</td>" +
                        "<td class=" + "style2" + ">" + dateOfBirth + "</td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Phone1</td>" +
                        "<td class=" + "style2" + ">" + phone1 + "</td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Phone2</td>" +
                        "<td class=" + "style2" + ">" + phone2 + "</td>" +
                        "</tr>" +

                         "<tr>" +
                        "<td class=" + "style1" + ">Email</td>" +
                        "<td class=" + "style2" + ">" + email + "</td>" +
                        "</tr>" +

                         "<tr>" +
                        "<td class=" + "style1" + ">Employername</td>" +
                        "<td class=" + "style2" + ">" + employerName + "</td>" +
                        "</tr>" +

                         "<tr>" +
                        "<td class=" + "style1" + ">Office Address 1</td>" +
                        "<td class=" + "style2" + ">" + officeAddress1 + "</td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Office Address 2</td>" +
                        "<td class=" + "style2" + ">" + officeAddress2 + "</td>" +
                        "</tr>" +

                        "<tr>" +
                        "<td class=" + "style1" + ">Office State</td>" +
                        "<td class=" + "style2" + ">" + officeState + "</td>" +
                        "</tr>" +

                         "<tr>" +
                        "<td class=" + "style1" + "></td>" +
                        "<td class=" + "style2" + "></td>" +
                        "</tr>" +
                     "</table>" +
                    "</div>" +
                    "</form>" +
                "</body>" +
                "</html>";

            try
            {
                if (IsValid(receipient))
                {
                    emailSender.Send();
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                flag = false;
            }

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(connString))
                {
                    sqlConn.Open();
                    SqlCommand sqlCmd = new SqlCommand("Insert Into LEG_PENSION_LEAD (firstname, middlename, lastname, dateofbirth, phone1,phone2,email,EmployerName, OfficeAddress1,OfficeAddress2,OfficeState) select @FirstName, @MiddleName, @LastName, @DateOfBirth, @Phone1, @Phone2, @Email,@EmployerName @OfficeAddress1,@OfficeAddress2, @OfficeState");
                    sqlCmd.Connection = sqlConn;
                    sqlCmd.Parameters.AddWithValue("@firstname", firstName);
                    sqlCmd.Parameters.AddWithValue("@middlename", middleName);
                    sqlCmd.Parameters.AddWithValue("@lastname", lastName);
                    sqlCmd.Parameters.AddWithValue("@dateofbirth", dateOfBirth);
                    sqlCmd.Parameters.AddWithValue("@phone1", phone1);
                    sqlCmd.Parameters.AddWithValue("@phone2", phone2);
                    sqlCmd.Parameters.AddWithValue("@email", email);
                    sqlCmd.Parameters.AddWithValue("@EmployerName", employerName);
                    sqlCmd.Parameters.AddWithValue("@OfficeAddress1", officeAddress1);
                    sqlCmd.Parameters.AddWithValue("@OfficeAddress2", officeAddress2);
                    sqlCmd.Parameters.AddWithValue("@OfficeState", officeState);
                    sqlCmd.BeginExecuteNonQuery();
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        private static bool CertificateValidationCallBack(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }
            else
            {
                // In all other cases, return false.
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }

        private bool IsValid(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.             
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper, RegexOptions.None);
            }
            catch (Exception ex)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                      RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
