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
        /// <remarks/>
        public string Count { get; set; }

        /// <remarks/>
        public string Start { get; set; }

        /// <remarks/>
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
        /// <remarks/>
        public string Body { get; set; }

        /// <remarks/>
        public string Date { get; set; }

        /// <remarks/>
        public int ID { get; set; }

        /// <remarks/>
        public string LastMsgBy { get; set; }

        /// <remarks/>
        public string Sender { get; set; }

        /// <remarks/>
        public string Subject { get; set; }

        /// <remarks/>
        public string Unread { get; set; }
    }
}