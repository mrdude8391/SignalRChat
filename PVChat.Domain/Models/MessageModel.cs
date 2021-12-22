﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.Domain.Models
{
    public class MessageModel
    {
        public string MessageId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime SentTime { get; set; }
        public DateTime DeliveredTime { get; set; }



        public MessageModel()
        {
            MessageId = Guid.NewGuid().ToString();
            CreatedTime = DateTime.Now;
            Status = MessageStatus.Pending;
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
