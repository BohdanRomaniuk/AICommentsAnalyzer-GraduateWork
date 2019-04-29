using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace parser.Models
{
    [Serializable]
    public class Comment : INotifyPropertyChanged
    {
        private string commentAuthor;
        private string commentText;
        private DateTime commentDate;
        private int sentiment;

        public string CommentAuthor
        {
            get => commentAuthor;
            set
            {
                commentAuthor = value;
                OnPropertyChanged(nameof(CommentAuthor));
            }
        }
        public string CommentText
        {
            get => commentText;
            set
            {
                commentText = value;
                OnPropertyChanged(nameof(commentText));
            }
        }
        public DateTime CommentDate
        {
            get => commentDate;
            set
            {
                commentDate = value;
                OnPropertyChanged(nameof(CommentDate));
            }
        }
        public int Sentiment
        {
            get => sentiment;
            set
            {
                sentiment = value;
                OnPropertyChanged(nameof(Sentiment));
            }
        }

        public Comment()
        {
        }

        public Comment(string _author, string _text, DateTime _date, int _sentiment = 0)
        {
            CommentAuthor = _author;
            CommentText = _text;
            CommentDate = _date;
            Sentiment = _sentiment;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
