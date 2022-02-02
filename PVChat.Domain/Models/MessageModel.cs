using System;

namespace PVChat.Domain.Models
{
    public class MessageModel : ModelBase
    {
        public string MessageId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string Message { get; set; }
        public byte[] Image { get; set; }

        private MessageStatus _status;

        public MessageStatus Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public DateTime CreatedTime { get; set; }
        public DateTime SentTime { get; set; }
        public DateTime DeliveredTime { get; set; }

        private bool _unread;  // Unread can maybe be moved to MessageStatus

        public bool Unread
        {
            get { return _unread; }
            set { _unread = value; OnPropertyChanged(nameof(Unread)); }
        }

        public bool IsOriginNative { get; set; }
        public bool HasDateBreak { get; set; }

        public MessageModel()
        {
            MessageId = Guid.NewGuid().ToString();
            CreatedTime = DateTime.Now;
            Status = MessageStatus.Pending;
            Unread = true;
            HasDateBreak = false;
        }
    }

    public enum MessageStatus
    {
        Pending,
        Sent,
        Delivered,
        Failed
    }
}