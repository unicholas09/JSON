using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Data.SqlTypes;

namespace MobileApp
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "authentication/{pin}/{accessCode}/{phoneNumber}")]
        string ValidateCustomer(string pin, string accessCode, string phoneNumber);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "summarizedReport/?accessCode={accessCode}")]
        string GetSummaryReports(int accessCode);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "detailedReport/?accessCode={accessCode}&startDate={startDate}&endDate={endDate}")]
        string GetDetailedReport(int accessCode, DateTime startDate, DateTime endDate);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "unitPrice/?FundID={FundID}")]
        string UnitPrices(int FundID);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "unitPriceArray/?FundID={FundID}&startDate={startDate}&endDate={endDate}")]
        string UnitPriceRange(string FundID, DateTime startDate, DateTime endDate);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "accessCode/{pin}/{phoneNumber}")]
        Boolean GetAccessCode(string pin, string phoneNumber);

        [OperationContract]
        [WebInvoke(Method = "PUT",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "newLead/?firstName={firstName}&middleName={middleName}&lastName={lastName}&dateOfBirth={dateOfBirth}&phone1={phone1}&phone2={phone2}&email={email}&employerName={employerName}&officeAddress1={officeAddress1}&officeAddress2={officeAddress2}&officeState={officeState}")]
        Boolean CreateNewUser(string firstName, string middleName, string lastName, DateTime dateOfBirth, string phone1, string phone2, string email, string employerName, string officeAddress1, string officeAddress2, string officeState);

    }
}