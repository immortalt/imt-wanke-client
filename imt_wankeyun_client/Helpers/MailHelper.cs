using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Helpers
{
    public class MailHelper
    {
        // 设置发送方的邮件信息,例如使用网易的smtp.163.com
        internal static string smtpServer = ""; //SMTP服务器
        internal static string username = ""; //登陆用户名
        internal static string password = "";//登陆密码
        internal static int port = 25;//登陆密码
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailTo">要发送的邮箱</param>
        /// <param name="mailSubject">邮箱主题</param>
        /// <param name="mailContent">邮箱内容</param>
        /// <returns>返回发送邮箱的结果</returns>
        public static Task<string> SendEmail(string mailTo, string mailSubject, string mailContent)
        {
            return Task.Run(() =>
            {
                // 邮件服务设置
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
                smtpClient.Host = smtpServer; //指定SMTP服务器
                smtpClient.Port = port;
                smtpClient.Credentials = new System.Net.NetworkCredential(username, password);//用户名和密码

                // 发送邮件设置        
                MailMessage mailMessage = new MailMessage(username, mailTo); // 发送人和收件人
                mailMessage.Subject = mailSubject;//主题
                mailMessage.Body = mailContent;//内容
                mailMessage.BodyEncoding = Encoding.UTF8;//正文编码
                mailMessage.IsBodyHtml = true;//设置为HTML格式
                mailMessage.Priority = MailPriority.High;//优先级
                //mailMessage.CC.Add(username);//给自己抄送一份
                try
                {
                    smtpClient.Send(mailMessage); // 发送邮件
                    return "发送成功";
                }
                catch (SmtpException ex)
                {
                    Debug.Write(ex.Message);
                    if (ex.Message.Contains("Error: need EHLO and AUTH first !"))
                    {
                        return "登陆失败！用户名或密码不正确";
                    }
                    else
                    {
                        return "登陆失败！\n错误信息：" + ex.Message;
                    }
                }
            });
        }
    }
}
