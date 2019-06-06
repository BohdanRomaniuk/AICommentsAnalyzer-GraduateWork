using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace parser.Models
{
    [Serializable]
    public class Comment : INotifyPropertyChanged, IEquatable<Comment>
    {
        private int id;
        private string commentAuthor;
        private string commentText;
        private DateTime commentDate;
        private double sentiment;

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
        public double Sentiment
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

        public Comment(int _id, string _author, string _text, DateTime _date, double _sentiment = 0)
        {
            Id = _id;
            CommentAuthor = _author;
            CommentText = _text;
            CommentDate = _date;
            Sentiment = _sentiment;
        }

        private Color GetBrushColor()
        {
                return (Sentiment > 0.5) ? Color.FromRgb(196, 255, 196) : Color.FromRgb(255, 196, 196);
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(Comment other)
        {
            return Id == other.Id;
        }
    }
}
