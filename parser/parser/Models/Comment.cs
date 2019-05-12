using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace parser.Models
{
    [Serializable]
    public class Comment : INotifyPropertyChanged
    {
        private int id;
        private string commentAuthor;
        private string commentText;
        private DateTime commentDate;
        private int sentiment;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
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
                OnPropertyChanged(nameof(Brush));
            }
        }

        public SolidColorBrush Brush => new SolidColorBrush(GetBrushColor());

        public Comment()
        {
        }

        public Comment(int _id, string _author, string _text, DateTime _date, int _sentiment = 0)
        {
            Id = _id;
            CommentAuthor = _author;
            CommentText = _text;
            CommentDate = _date;
            Sentiment = _sentiment;
        }

        private Color GetBrushColor()
        {
            switch(Sentiment)
            {
                case 1:
                    return Color.FromRgb(196, 255, 196);
                case 0:
                    return Color.FromRgb(255, 196, 196);
                default:
                    return Color.FromRgb(255, 255, 196);
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
