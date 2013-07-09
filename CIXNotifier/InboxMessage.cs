using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CIXNotifier
{
    public class InboxMessages : IEnumerable<InboxMessage>
    {
        readonly Collection<InboxMessage> _messages = new Collection<InboxMessage>();

        public void Add(InboxMessage newMessage)
        {
            _messages.Add(newMessage);
        }

        public void Clear()
        {
            _messages.Clear();
        }

        public int Count
        {
            get { return _messages.Count; }
        }

        public InboxMessage this[int index]
        {
            get { return _messages[index]; }
        }

        /// <summary>
        /// Enumerator for all messages.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<InboxMessage> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        // Non-generic enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class InboxMessage
    {
        public int Id { get; set; }

        public string Sender { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public DateTime Date { get; set; }
    }
}
