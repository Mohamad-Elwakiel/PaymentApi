using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public  class Message
    {
        public List<MailboxAddress> To { get; set; }    
        public string subject { get; set; } 
        public string content { get; set; }
        public Message(IEnumerable<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress("", x)));
            this.subject = subject;
            this.content = content;
            
        }
    }
}
