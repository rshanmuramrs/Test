using System.Configuration;
using System.Net.Mail;
using FADV.Configuration;
using Fadv.CAST.CA.Util.Mail;
using System.Collections.Generic;
using System;
using System.Text;
using Fadv.Cast.App.Service;
using Fadv.Cast.Shared.BusinessObjects;
using Fadv.Cast.App.Mapper;
using System.Data;
using Fadv.CAST.CA.Util;


namespace FADV.CASTCAN.Order.Business
{

    //Shanmugam
    public class SmtpParameters : ISmtpParameters
    {
        private string smtpServer;

        public SmtpParameters()
        {
            string ID = "1";
            //smtpServer = Services.Cfg.Get((object)ID).Smtpserver.ToString(); // ConfigurationManager.AppSettings["MailServer"].ToString();
            smtpServer = ConfigurationUtil.GetConfigAsString("CandMailSmtp");
        }

        public string SmtpServer
        {
            get { return smtpServer; }
            set { smtpServer = value; }
        }
    }
        
    class SendOrderNotify
    {
        private const string NOTIFICATION_FROM_EMAIL_ADDRESS = "CASTNotificationService@fadv.com";
        
        /// <summary>
        ///     Generic Method that handles all the calls for sending an email
        /// </summary>
        /// <param name="emailKey">Email Key which identifies where the email should be sent</param>
        /// <param name="msg">Message that needs to be sent across</param>
        /// <param name="fromAddress">Email Address from where the email is originating from</param>
        public static void SendMail(string emailKey, string msg, string fromAddress)
        {
            CASTNotificationProvider notificationSection = ConfigurationManager.GetSection("NotificationSettings") as CASTNotificationProvider;
            foreach (CASTNotificationItemElement Element in notificationSection.CASTNotificationItems)
            {
                if (Element.Active && emailKey.Equals(Element.EmailKey))
                {
                    SendEmail SM = new SendEmail();
                    SM.ToAddress.Add(Element.EmailToName);
                    SM.Port = Element.ServerPort.ToString();
                    SM.Body = msg;
                    SM.IsBodyHtml = Element.IsHTMLBody;
                    SM.MailFrom = new MailAddress(fromAddress);
                    SM.MailSubject = Element.Subject;
                    SM.Priority = System.Net.Mail.MailPriority.Normal;
                    SM.ServerName = Element.ServerName;
                    SM.SendMessage();
                    return;
                }
            }
        }

        /// <summary>
        /// Utility Message to notify succesful order creation
        /// </summary>
        /// <param name="msg">Success Message</param>
        public static void SendSuccessOrderNotification(string msg)
        {
            SendMail("NewOrderSuccess", msg, NOTIFICATION_FROM_EMAIL_ADDRESS);
        }

        /// <summary>
        /// Utility Method to notify failure in order creation and the error message
        /// </summary>
        /// <param name="msg">Error Message</param>
        public static void SendFailedOrderNotification(string msg)
        {
            SendMail("NewOrderFailed", msg, NOTIFICATION_FROM_EMAIL_ADDRESS);
        }

        /// <summary>
        /// Utility Method to notify failure in order creation
        /// </summary>
        /// <param name="msg">Failed Error Message</param>
        public static void SendFailedOrderSupportNotification(string msg)
        {
            SendMail("OrderSupport", msg, NOTIFICATION_FROM_EMAIL_ADDRESS);
        }

        /// <summary>
        /// Send Order Cancellation Notification via Email
        /// </summary>
        /// <param name="msg"></param>
        public static void SendCanceledOrderNotification(string msg)
        {
            SendMail("OrderCanceled", msg, NOTIFICATION_FROM_EMAIL_ADDRESS);
        }

        # region PeopleClickIntegration
        public static void SendCandidateMail(string emailTemplateID, string sendMailTo, string ClntNm, string candidateName, string userID, string password, string msgCC, bool addForms, string messageFrom, string requestor, string expiryDate, string pkgName, bool isAttach)
        {
            string loginURL = string.Empty;
            //if (Session["ClntCountryId"].ToString() == "23")
            //{
            //    loginURL = "";// System.Configuration.ConfigurationSettings.AppSettings["CAloginURL"].ToString();
            //}
            //else
            //{
            //    loginURL = "";// System.Configuration.ConfigurationSettings.AppSettings["UKloginURL"].ToString();
            //}

            string attachment;
            attachment = string.Empty;

            if (addForms)
            {
                if (isAttach)
                {
                    DataSet ds = new DataSet();
                    ds = Services.ClntCntc.GETConsentForms(userID);
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            attachment = ds.Tables[0].Rows[0]["ConsentForm"].ToString();
                        }
                    }
                }
            }
            messageFrom = messageFrom.Replace("''", "'");
            string[] mailFrom = messageFrom.Trim().Split(';');
            int incr;
            for (incr = 0; incr < mailFrom.Length; incr++)
            {
                messageFrom = mailFrom[0].Trim();
            }
            SendCandidateMail(emailTemplateID, sendMailTo, ClntNm, candidateName, userID, password, System.DateTime.Today.Date.ToShortDateString().ToString(), msgCC, attachment, messageFrom, requestor, expiryDate, loginURL, pkgName);
        }


        private static void SendCandidateMail(string emailTemplateID, string messageTo, string clientName, string candidateName, string userId, string password, string createdDt, string msgCC, string attacment, string messageFrom, string requestor, string expiryDate, string loginURL, string pkgName)
        {
            string ID = "1";
            Cfg ocfg = Services.Cfg.Get((object)ID);
            string disclaimer = ocfg.DsclmrText.ToString();
            string disclaimerfrench = ocfg.DsclmrTextFrench.ToString();
            string companyshort = ocfg.CompanyShort.ToString();
            string FaxConsent = ocfg.Faxconsent.ToString();
            string RequestEmail = ocfg.RequestEmail.ToString();
            string applicantBliandCopy = ocfg.ApplicantBlindCopy.ToString();
            string costServEmail = ocfg.CostServEmail.ToString();

            if (ocfg.WebURL.Substring(ocfg.WebURL.Length - 1, 1) == "/")
            {
                loginURL = ocfg.WebURL + "Security/CandidateLogin.aspx";
            }
            else
            {
                loginURL = ocfg.WebURL + "/Security/CandidateLogin.aspx";
            }


            Dictionary<string, string> tokens = new Dictionary<string, string>();

            tokens.Add("<%ClientName%>", clientName);
            tokens.Add("<%CandidateName%>", candidateName);
            tokens.Add("<%UserId%>", userId);
            tokens.Add("<%CreatedDt%>", createdDt);
            tokens.Add("<%Password%>", password);
            tokens.Add("<%Requestor%>", requestor);
            tokens.Add("<%ExpiryDate%>", expiryDate);
            tokens.Add("<%Disclaimer%>", disclaimer);
            tokens.Add("<%DisclaimerFrench%>", disclaimerfrench);
            tokens.Add("<%LoginURL%>", loginURL);
            tokens.Add("<%LoginURLFrench%>", loginURL + "?lang=fr");
            tokens.Add("<%Companyshort%>", companyshort);
            tokens.Add("<%FaxConsent%>", FaxConsent);
            tokens.Add("<%RequestEmail%>", RequestEmail);
            tokens.Add("<%PackageName%>", pkgName);
            tokens.Add("<%ApplicantBlindCopy%>", applicantBliandCopy);
            tokens.Add("<%CompanyShort%>", companyshort);
            tokens.Add("<%RecruiterEmail%>", messageFrom);
            tokens.Add("<%CostServEmail%>", costServEmail);


            messageFrom = string.Empty;

            Send(emailTemplateID, messageTo, tokens, msgCC, attacment, messageFrom);
        }

        private static void Send(string emailTemplateID, string messageTo, Dictionary<string, string> tokens, string msgCC, string attacment, string messageFrom)
        {
            Send(emailTemplateID, messageFrom, messageTo, null, null, null, tokens, null, msgCC, attacment);
        }

        private static void Send(string emailTemplateID, string messageFrom, string messageTo, string messageSubject, string messageBody, bool? isHtml, Dictionary<string, string> tokens, Attachment attachment, string msgCC, string attacment)
        {
            Services.EmailTemplate.Send(new SmtpParameters(), emailTemplateID, messageFrom, messageTo, messageSubject, messageBody, isHtml, tokens, attachment, msgCC, attacment);
        }
        # endregion



    }
}

