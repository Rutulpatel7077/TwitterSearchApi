using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WZWVAPI
{
    public class Feedback
    {
        public enum FeedbackType { Error, Message };

        public List<string> ErrorList { get; private set; }
        public List<string> MessageList { get; private set; }

        public Feedback()
        {
            this.ErrorList = new List<string>();
            this.MessageList = new List<string>();
        }

        public void AddFeedback(FeedbackType feedbackType, string message)
        {
            if (feedbackType == FeedbackType.Error)
            {
                ErrorList.Add(message);
            }
            else
            {
                MessageList.Add(message);
            }
        }
    }
}