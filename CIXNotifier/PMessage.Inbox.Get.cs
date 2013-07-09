namespace CIXNotifier
{
    /// <remarks/>
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public class ConversationInboxSet
    {
        /// <summary>
        /// Total count of messages
        /// </summary>
        public string Count { get; set; }

        /// <summary>
        /// Index of the Start message.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// An array of CIXInboxItems that represent messages in the inbox.
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute("ConversationInbox", typeof (CIXInboxItem), IsNullable = false)]
        public CIXInboxItem[] Conversations { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    public class CIXInboxItem
    {
        /// <summary>
        /// Body of the message
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Date of the message in ISO format.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// ID of the message.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Actually, not quite sure what this is.
        /// </summary>
        public string LastMsgBy { get; set; }

        /// <summary>
        /// Nickname of the sender.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// The message subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// true or false value which indicates whether or not
        /// this message has been read.
        /// </summary>
        public string Unread { get; set; }
    }
}